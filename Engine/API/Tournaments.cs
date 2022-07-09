/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ServerModels;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Api
{
    public static class Tournaments
    {
        public static string Join(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, string tournamentShortCode, int score)
        {
            TournamentLiveData liveTournament = socialEdgeTournament.TournamentLiveModel.Get(tournamentShortCode);
            if (liveTournament == null)
                return null;

            ActiveCollectionInfo activeCollectionInfo = TournamentCollectionManager.GetCurrentActiveCollection(socialEdgeTournament, tournamentShortCode);
            var playerEntry = TournamentCollectionManager.GetPlayerFromCurrentTournament(socialEdgePlayer, activeCollectionInfo.name);
            if (playerEntry != null)
                return null;

            int retentionDay = GetPlayerRetentionDays(socialEdgePlayer);
            if (retentionDay >= 7) 
                retentionDay = 7;
            string retentionDayString = "D"+retentionDay;

            socialEdgePlayer.PlayerModel.Tournament.reportingChampionshipCollectionIndex = activeCollectionInfo.index;
            TournamentCollectionManager.RegisterEntry(socialEdgePlayer, socialEdgeTournament, 0, retentionDayString, activeCollectionInfo.name);
            socialEdgePlayer.PlayerModel.Tournament.isReportingInChampionship = true;

            // Calculating current start time
            var startTimeSeconds = liveTournament.startTime / 1000;
            var durationSeconds = liveTournament.duration * 60;
            var waitTimeSeconds = liveTournament.waitTime * 60;
            var currentStartTimeUTCSeconds = CalculateCurrentStartTime(waitTimeSeconds, durationSeconds, startTimeSeconds) * 1000;
            TournamentDataModel tournamentModel = SetupTournamentModel(socialEdgeTournament, tournamentShortCode, liveTournament, currentStartTimeUTCSeconds, activeCollectionInfo.expiryTime, activeCollectionInfo.index);

            int tournamentMaxScore = socialEdgePlayer.PlayerModel._tournament.tournamentMaxScore;
            var tournamentSlot = TournamentCollectionManager.GetSlot(liveTournament.slotsData, tournamentMaxScore);
            socialEdgeTournament.TournamentModel.Tournament.tournamentSlot = tournamentSlot;

            var pool = TournamentPoolSample(activeCollectionInfo.name, socialEdgePlayer, tournamentSlot, tournamentModel.Tournament.joinedTime, liveTournament.maxPlayers -1, null);
            socialEdgeTournament.TournamentModel.Tournament.entries = pool;
            string tournamentId = socialEdgeTournament.TournamentModel.Id;
            var playerActiveTournament = CreatePlayerActiveTournament(tournamentModel.Tournament, 1);
            var activeTournaments = socialEdgePlayer.PlayerModel.Tournament.activeTournaments;
            activeTournaments.Add(tournamentId, playerActiveTournament);
            socialEdgePlayer.PlayerModel.Tournament.activeTournaments = activeTournaments;

            return tournamentId;
        }

        private static TournamentDataModel SetupTournamentModel(SocialEdgeTournamentContext socialEdgeTournament, string tournamentShortCode, TournamentLiveData liveTourament, long currentStartTime, DateTime expiryTime, int tournamentCollectionIdx)
        {
            var tournament = socialEdgeTournament.TournamentModel.Tournament;
            tournament.shortCode = tournamentShortCode;
            tournament.name = liveTourament.name;
            tournament.type = liveTourament.type;
            tournament.startTime = currentStartTime;
            tournament.joinedTime = Utils.UTCNow();
            tournament.lastUpdateTimeSeconds = 0;
            tournament.secondsLeftNextUpdate = 0;
            tournament.duration = liveTourament.duration;
            tournament.rewards = liveTourament.rewards;
            tournament.concluded = false;
            tournament.entries = new List<TournamentPoolEntry>();
            tournament.score = 0;
            tournament.expireAt = expiryTime;
            tournament.tournamentCollectionIndex = tournamentCollectionIdx;
            tournament.tournamentSlot = 0;  

            return socialEdgeTournament.TournamentModel;  
        } 

        private static ActiveTournament CreatePlayerActiveTournament(TournamentData tournament, int rank)
        {
            return new ActiveTournament()
            {
                shortCode = tournament.shortCode,
                type = tournament.type,
                name = tournament.name,
                rank = rank,
                grandPrize = tournament.rewards["0"],
                startTime = tournament.startTime,
                duration = tournament.duration,
                score = 0,
                matchesPlayedCount = 0
            };
        }

        public static int GetPlayerRetentionDays(SocialEdgePlayerContext socialEdgePlayer)
        {
            // TODO: change combined info to a get Profile
            long creationTime = Utils.ToUTC(socialEdgePlayer.CombinedInfo.AccountInfo.Created);
            double retentionDays = (Utils.UTCNow() - creationTime) / (60*60*24*1000.0);
            return (int)Math.Floor(retentionDays);
        }
        
        public static long GetPlayerMaxScore(SocialEdgePlayerContext socialEdgePlayer) 
        {
            //long? tournamentMaxScore = socialEdgePlayer.CombinedInfo.UserReadOnlyData(Constants.PrivateData.TOURNAMENT_MAX_SCORE);
            //return tournamentMaxScore != null ? tournamentMaxScore.Value : 0;
            return 0;
        }

        private static long CalculateCurrentStartTime(long waitTimeSeconds, long durationSeconds, long firstStartTimeSeconds) 
        {
            long currentTimeSeconds = Utils.UTCNow() / 1000;
            long currentStartTimeInSeconds = 0;

            if (currentTimeSeconds > firstStartTimeSeconds)
            {
                var tournamentGap = durationSeconds + waitTimeSeconds;
                var currentTimeGap = (currentTimeSeconds - firstStartTimeSeconds) % tournamentGap;
                currentStartTimeInSeconds = currentTimeGap < durationSeconds ? currentTimeSeconds - currentTimeGap : 
                                                                                (currentTimeSeconds - currentTimeGap) + tournamentGap;
            }
            else
            {
                currentStartTimeInSeconds = firstStartTimeSeconds;
            }

            return currentStartTimeInSeconds;
        }

        private static int GetTournamentDaysLeftWhenJoined(SocialEdgePlayerContext socialEdgePlayer, long joinedTime)
        {
            int daysLeft = 7;
            var activeTournaments = socialEdgePlayer.GetPlayerData("activeTournaments", "hotData");
            var activeTournament = activeTournaments.AsBsonDocument.GetElement(0).Value.AsBsonDocument;
            var tournamentEndTime = activeTournament["startTime"].ToInt64() + (activeTournament["duration"].ToInt64() * 60 * 1000);
            if(tournamentEndTime > 0 && tournamentEndTime > joinedTime)
            {
                long diff = tournamentEndTime - joinedTime;
                daysLeft = (int)(diff / (60*60*24*1000));
                daysLeft = (int)Math.Floor((float)daysLeft);
            }

            return daysLeft;
        }

        private static List<TournamentPoolEntry> FilterFriends(SocialEdgePlayerContext socialEdgePlayer, List<TournamentPoolEntry> entries)
        {
            List<TournamentPoolEntry> filtered = new List<TournamentPoolEntry>();
            var friends = socialEdgePlayer.Friends;
            foreach (var entry in entries)
            {
                FriendInfo friend = friends.Find(x => x.FriendPlayFabId == socialEdgePlayer.PlayerIdFromObjectId(entry._id));
                if (friend == null)
                    filtered.Add(new TournamentPoolEntry() { _id = entry._id});
            }

            return filtered;
        }

        private static List<TournamentPoolEntry> TournamentPoolSample(string collectionName, SocialEdgePlayerContext socialEdgePlayer, int tournamentSlot, long joinedTime, int poolSize, List<ObjectId> a_alreadyIncludedInArray)
        {
            List<TournamentPoolEntry> pool;
            if(tournamentSlot <= 1) tournamentSlot = 1;
            if(poolSize <= 1) poolSize = 1;
            
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<TournamentEntryData>(collectionName);
            string playerId = socialEdgePlayer.PlayerId;
            List<ObjectId> alreadyIncludedArray = new List<ObjectId>(a_alreadyIncludedInArray != null ? a_alreadyIncludedInArray : new List<ObjectId>());
            alreadyIncludedArray.Add(ObjectId.Parse(socialEdgePlayer.PlayerDBId));
            
            // Check if user joined tournamnet on last day add the active players in leaderboards
            var daysLeft = GetTournamentDaysLeftWhenJoined(socialEdgePlayer, joinedTime);
  
            FilterDefinition<TournamentEntryData> filter = Builders<TournamentEntryData>.Filter.Eq("tournamentSlot", tournamentSlot);
            filter = filter & Builders<TournamentEntryData>.Filter.Nin<ObjectId>("_id", alreadyIncludedArray);
            var sortByJoinTime = Builders<TournamentEntryData>.Sort.Ascending("joinTime");
            var sortByLastActive = Builders<TournamentEntryData>.Sort.Ascending("lastActiveTime");
            var projection = Builders<TournamentEntryData>.Projection.Include("_id");

            if (daysLeft > 0)
                pool = collection.Find(filter).Sort(sortByJoinTime).Sort(sortByLastActive).Project<TournamentPoolEntry>(projection).Limit(poolSize).ToList<TournamentPoolEntry>();
            else
                pool = collection.Find(filter).Sort(sortByLastActive).Project<TournamentPoolEntry>(projection).Limit(poolSize).ToList<TournamentPoolEntry>();

            // Filter out self and blocked
            pool = FilterFriends(socialEdgePlayer, pool);

            return pool;
        }
    }
}