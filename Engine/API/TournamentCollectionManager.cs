/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Api
{
    public class ActiveCollectionInfo
    {
        public int index;
        public string name;
        public long expiryTime;

        public ActiveCollectionInfo(int index, string name, long expiryTime)
        {
            this.index = index;
            this.name = name;
            this.expiryTime = expiryTime;
        }
    }

    public static class TournamentCollectionManager
    {
        public static ActiveCollectionInfo GetCurrentActiveCollection(SocialEdgeTournamentContext socialEdgeTournament, string tournamentShortCode)
        {
             TournamentLiveData tournamentLive = socialEdgeTournament.TournamentLiveModel.Get(tournamentShortCode);
             if (tournamentLive == null)
                return null;

            long absoluteStartTime = tournamentLive.startTime; // in milliseconds
            long tournamentDuration = tournamentLive.durationMinutes * 60; // in seconds
            return FindCurrentActiveCollection(tournamentLive, absoluteStartTime / 1000, tournamentDuration);
        }

        public static ActiveCollectionInfo FindCurrentActiveCollection(TournamentLiveData tournamentConfig, long absoluteStartTimeSeconds, long tournamentDurationSeconds)
        {
            long currentTimeSeconds = (long)Math.Floor(Utils.UTCNow() / 1000.0);
            long currentStartTimeInSeconds = 0;
            long expiryTimeMilliseconds = 0;

            int i = 0;
            string collectionName = "";
            bool found = false;
            int numCollections = tournamentConfig.noOfCollections;
            while (!found && i < numCollections)
            {
                var currentAbsoluteStartTime = absoluteStartTimeSeconds - (tournamentDurationSeconds * i);
                var tournamentGap = tournamentDurationSeconds * numCollections;
                var currentTimeGap = (currentTimeSeconds - currentAbsoluteStartTime) % tournamentGap;

                if (currentTimeGap < tournamentDurationSeconds)
                {
                    currentStartTimeInSeconds = currentTimeSeconds - currentTimeGap;
                    var timeLeftInExpiration = tournamentDurationSeconds * (numCollections - 1);
                    expiryTimeMilliseconds = (currentStartTimeInSeconds + timeLeftInExpiration) * 1000;
                    collectionName = tournamentConfig.collectionPrefix;
                    found = true;
                }
                else
                {
                    i++;
                }
            }

            return found ? new ActiveCollectionInfo(i, collectionName + i.ToString(), expiryTimeMilliseconds) : null;
        }

        public static int GetSlot(List<TournamentSlot> slotsData, long maxScore)
        {
            bool found = false;
            int i = 0;
            while (!found && i < slotsData.Count)
            {
                found = maxScore < slotsData[i].max;
                i++;
            }
            
            return found ? i - 1 : 1;
        }

        public static void RegisterEntry(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, int score, string retentionDayString, string collectionName)
        {
            TournamentEntryData playerEntry = socialEdgeTournament.TournamentEntryModel.Get(socialEdgePlayer.PlayerDBId, collectionName);
            if (playerEntry != null)
                return;

            string tournamentShortCode = socialEdgeTournament.TournamentLiveModel.GetActiveShortCode(socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot);
            var activeTournaments = socialEdgePlayer.PlayerModel.Tournament.activeTournaments;
            var activeTournament = activeTournaments != null && activeTournaments.ContainsKey(tournamentShortCode) ? activeTournaments[tournamentShortCode] : null;
            int tournamentNewScore = activeTournament != null ? activeTournament.score : 0;
            int tournamentMaxScore = socialEdgePlayer.PlayerModel.Tournament.tournamentMaxScore;
            if (tournamentNewScore > tournamentMaxScore)
            {
                socialEdgePlayer.PlayerModel.Tournament.tournamentMaxScore = tournamentNewScore;
                tournamentMaxScore = tournamentNewScore;
            }

            var tournamentLive = socialEdgeTournament.TournamentLiveModel.Get(tournamentShortCode);
            var slot = GetSlot(tournamentLive.slotsData, tournamentMaxScore);

            // Create a tournament entry
            TournamentEntryData entry = socialEdgeTournament.TournamentEntryModel.Create(socialEdgePlayer.PlayerDBId, collectionName);
            entry.playerId = socialEdgePlayer.PlayerId;
            entry.displayName = socialEdgePlayer.CombinedInfo.PlayerProfile.DisplayName;
            entry.country = socialEdgePlayer.CombinedInfo.PlayerProfile.Locations[0].CountryCode.ToString();

            entry.eloScore = socialEdgePlayer.PlayerModel.Info.eloScore;
            entry.rnd = Math.Floor((new Random().NextDouble() * 100));
            entry.expireAt = GetCurrentActiveCollection(socialEdgeTournament, tournamentShortCode).expiryTime;
            entry.score = score;
            entry.retentionDay = retentionDayString;
            entry.tournamentMaxScore = tournamentMaxScore;
            entry.tournamentSlot = slot;
            entry.lastActiveTime = Utils.UTCNow();
            entry.joinTime = Utils.UTCNow();
            entry.playerTimeZoneSlot = socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot;
            entry._playerMiniProfile = socialEdgePlayer.MiniProfile;
        }

        public static int GetPlayerEntryRank(SocialEdgePlayerContext socialEdgePlayer, string collectionName, List<string> ids)
        {
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<TournamentEntryModelDocument>(collectionName);
            FilterDefinition<TournamentEntryModelDocument> filter = Builders<TournamentEntryModelDocument>.Filter.In<string>("_id", ids);
            var sortByScore = Builders<TournamentEntryModelDocument>.Sort.Descending(typeof(TournamentEntryData).Name + ".score");
            var projection = Builders<TournamentEntryData>.Projection.Include("_id").Include(typeof(TournamentEntryData).Name + ".score");
            var entries = collection.Find(filter).Sort(sortByScore).ToList<TournamentEntryModelDocument>();
            return entries.FindIndex(0, entries.Count, x => x._id.ToString() == socialEdgePlayer.PlayerDBId) + 1;
        }
        
        public static List<TournamentLeaderboardEntry> GetSortedLeaderboardEntries(string collectionName, List<string> ids)
        {
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<TournamentEntryModelDocument>(collectionName);
            FilterDefinition<TournamentEntryModelDocument> filter = Builders<TournamentEntryModelDocument>.Filter.In<string>("_id", ids);
            var sortByScore = Builders<TournamentEntryModelDocument>.Sort.Descending(typeof(TournamentEntryData).Name + ".score");
            var projection = Builders<TournamentEntryModelDocument>.Projection.Expression(item => 
                                                                        new TournamentLeaderboardEntry(
                                                                                item._model.playerId, 
                                                                                item._model.score, 
                                                                                item._model.playerMiniProfile,
                                                                                item._model.country,
                                                                                item._model.displayName));
            return collection.Find(filter).Project(projection).Sort(sortByScore).ToList<TournamentLeaderboardEntry>();
        }
    }
}
