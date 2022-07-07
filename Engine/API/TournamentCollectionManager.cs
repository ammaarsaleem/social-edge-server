/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
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
        public DateTime expiryTime;

        public ActiveCollectionInfo(int index, string name, DateTime expiryTime)
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

            return found ? new ActiveCollectionInfo(i, collectionName + i.ToString(), new DateTime(expiryTimeMilliseconds)) : null;
        }

        public static BsonDocument GetPlayerFromCurrentTournament(SocialEdgePlayerContext socialEdgePlayer, string collectionName)
        {
            var collection = SocialEdge.DataService.GetCollection<BsonDocument>(collectionName);
            var collectionT = collection.FindOneById(socialEdgePlayer.PlayerDBId);
            collectionT.Wait();
            return collectionT.Result;
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
            var collection = SocialEdge.DataService.GetCollection<TournamentEntryData>(collectionName);
            var taskT = collection.FindOneById(socialEdgePlayer.PlayerDBId);
            taskT.Wait();
            if (taskT.Result != null)
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
            socialEdgeTournament.TournamentEntryModel.DBId = socialEdgePlayer.PlayerDBId;
            socialEdgeTournament.TournamentEntryModel.CollectionName = collectionName;
            TournamentEntryData entry = socialEdgeTournament.TournamentEntryModel.TournamentEntry;

            entry.eloScore = 1000;// socialEdgePlayer.PlayerModel.Info.eloScore;
            //publicProfile: PlayerModel.getPublicProfileByPlayer(sparkPlayer),
            entry.rnd = Math.Floor((new Random().NextDouble() * 100));
            entry.expireAt = GetCurrentActiveCollection(socialEdgeTournament, tournamentShortCode).expiryTime;
            entry.score = score;
            entry.retentionDay = retentionDayString;
            entry.league = socialEdgePlayer.PlayerModel.Info.league;
            entry.tournamentMaxScore = tournamentMaxScore;
            entry.tournamentSlot = slot;
            entry.lastActiveTime = Utils.UTCNow();
            entry.joinTime = Utils.UTCNow();
            entry.playerTimeZoneSlot = socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot;
        }
    }
}
