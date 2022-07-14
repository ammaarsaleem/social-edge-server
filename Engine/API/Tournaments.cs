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

            var pool = TournamentPoolSample(tournamentShortCode, activeCollectionInfo.name, socialEdgePlayer, tournamentSlot, tournamentModel.Tournament.joinedTime, liveTournament.maxPlayers -1, null);
            socialEdgeTournament.TournamentModel.Tournament.entryIds = pool;
            string tournamentId = socialEdgeTournament.TournamentModel.Id;
            var playerActiveTournament = CreatePlayerActiveTournament(tournamentModel.Tournament, 1);
            socialEdgePlayer.PlayerModel.Tournament.activeTournaments.Add(tournamentId, playerActiveTournament);

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
            tournament.entryIds = new List<string>();
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

        private static int GetTournamentDaysLeftWhenJoined(SocialEdgePlayerContext socialEdgePlayer, string tournamentShortCode, long joinedTime)
        {
            int daysLeft = 7;
            long tournamentEndTime = 0;
            var activeTournaments = socialEdgePlayer.PlayerModel.Tournament.activeTournaments;
            if (activeTournaments.ContainsKey(tournamentShortCode))
            {
                ActiveTournament activeTournament = activeTournaments[tournamentShortCode];
                tournamentEndTime = activeTournament.startTime + (activeTournament.duration * 60 * 1000);
            }
                
            if (tournamentEndTime > 0 && tournamentEndTime > joinedTime)
            {
                long diff = tournamentEndTime - joinedTime;
                daysLeft = (int)(diff / (60*60*24*1000));
                daysLeft = (int)Math.Floor((float)daysLeft);
            }

            return daysLeft;
        }

        private static List<string> FilterFriends(SocialEdgePlayerContext socialEdgePlayer, List<string> entries)
        {
            List<string> filtered = new List<string>();
            var friends = socialEdgePlayer.Friends;
            foreach (var entry in entries)
            {
                FriendInfo friend = friends.Find(x => x.FriendPlayFabId == socialEdgePlayer.PlayerIdFromObjectId(entry));
                if (friend == null)
                    filtered.Add(entry);
            }

            return filtered;
        }

        private static List<string> TournamentPoolSample(string tournamentShortCode, string collectionName, SocialEdgePlayerContext socialEdgePlayer, int tournamentSlot, long joinedTime, int poolSize, List<ObjectId> a_alreadyIncludedInArray)
        {
            List<string> pool;
            if(tournamentSlot <= 1) tournamentSlot = 1;
            if(poolSize <= 1) poolSize = 1;
            
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<TournamentEntryData>(collectionName);
            string playerId = socialEdgePlayer.PlayerId;
            List<ObjectId> alreadyIncludedArray = new List<ObjectId>(a_alreadyIncludedInArray != null ? a_alreadyIncludedInArray : new List<ObjectId>());
            alreadyIncludedArray.Add(ObjectId.Parse(socialEdgePlayer.PlayerDBId));
            
            // Check if user joined tournamnet on last day add the active players in leaderboards
            var daysLeft = GetTournamentDaysLeftWhenJoined(socialEdgePlayer, tournamentShortCode, joinedTime);
  
            FilterDefinition<TournamentEntryData> filter = Builders<TournamentEntryData>.Filter.Eq("tournamentSlot", tournamentSlot);
            filter = filter & Builders<TournamentEntryData>.Filter.Nin<ObjectId>("_id", alreadyIncludedArray);
            var sortByJoinTime = Builders<TournamentEntryData>.Sort.Ascending("joinTime");
            var sortByLastActive = Builders<TournamentEntryData>.Sort.Ascending("lastActiveTime");
            var projection = Builders<TournamentEntryData>.Projection.Include("_id");

            if (daysLeft > 0)
                pool = collection.Find(filter).Sort(sortByJoinTime).Sort(sortByLastActive).Project<string>(projection).Limit(poolSize).ToList<string>();
            else
                pool = collection.Find(filter).Sort(sortByLastActive).Project<string>(projection).Limit(poolSize).ToList<string>();

            // Filter out self and blocked
            pool = FilterFriends(socialEdgePlayer, pool);

            return pool;
        }

        public static void End(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, TournamentData activeTournament, TournamentDataModel tournament,  string tournamentId)
        {
            int tournamentNewScore = 0;

            if (activeTournament != null)
            {
                var tournamentLive = socialEdgeTournament.TournamentLiveModel.Get(activeTournament.shortCode);
                string collectionName = tournamentLive.collectionPrefix + activeTournament.tournamentCollectionIndex;
                List<TournamentEntryData> playerList = TournamentCollectionManager.GetEntries(collectionName, tournament.Tournament.entryIds);

                if (playerList != null)
                {

                }

            }
        }
    }
}

/*
    var end = function(sparkPlayer, activeTournament, tournament, tournamentId) {
        //-- Calculate reward here and reward player
        var playerData = PlayerModel.get(sparkPlayer);
        var tournamentNewScore = 0;

        if (activeTournament) {
            var tournamentConfig = TournamentConfig.get(tournament.shortCode);
            var playersList = ChampionshipsCollectionManager.getPlayerLeaderboardFromCollection(sparkPlayer, tournamentConfig, tournament.championshipCollectionIndex, tournament.entries);
            if (playersList) {
                var entries = [];
                for (var i = 0; i < playersList.length; i++) {
                    var playerEntry = createTournamentPlayerModel(playersList[i]._id.$oid, playersList[i].publicProfile, playersList[i].score, true);
                    entries.push(playerEntry);
                }

                sortEntries(entries);
                
                //// Adding player in entries list
                var playerId = sparkPlayer.getPlayerId();
                if (activeTournament) {
                    var publicProfile = PlayerModel.getPublicProfile(playerId);
                    var tournamentPlayer = createTournamentPlayerModel(playerId, publicProfile, activeTournament.score, false);
                    var playerAdded = false;
                    for (var i = 0; i < entries.length; i++) {
                        if (entries[i].score <= tournamentPlayer.score) {
                            entries.splice(i, 0, tournamentPlayer);
                            playerAdded = true;
                            break;
                        }
                    }
                    
                    if (!playerAdded) {
                        entries.push(tournamentPlayer);
                    }
                }
                
                updateRanks(entries);

                // tournament.entries = entries;
                
                updatePlayerActiveTournament(playerId, activeTournament, entries);
                
                tournament.score = activeTournament.score;
                tournamentNewScore = tournament.score;
            }

            tournament.concluded = true;
            
            var reward = TournamentModel.getRewardForRank(tournament, activeTournament.rank, playerData.pub.league);
            var heading = Constants.TournamentResultMessage.HEADING;
            var body = Constants.TournamentResultMessage.BODY;
            
            var trophies1 = 0;
            var gems1 = 0;
            var chestType1 = "";
            
            if(reward != undefined)
            {
                trophies1 = reward.trophies;
                gems1 = reward.gems;
                chestType1 = reward.chestType;
            }
            
            var msgInfo = Inbox._getMessageByType(sparkPlayer, 'RewardTournamentEnd');
            if (msgInfo == null) {
                // Tournament reward
                var msgInfo = {
                    type: "RewardTournamentEnd",
                    heading: heading,
                    body: body,
                    trophies: trophies1,
                    rank: activeTournament.rank,
                    reward: {
                        gems: gems1
                    },
                    tournamentType: activeTournament.type,
                    tournamentId: tournamentId,
                    chest: chestType1,
                    expireAt: tournament.expireAt
                }
                var message = Inbox.create(sparkPlayer, msgInfo);
                InboxModel.add(sparkPlayer, message);
            }
                
            // Award trophies
            // playerData.pub.trophies2 += reward.trophies;
            // _checkForLeaguePromotion(playerData);
            PlayerModel.set(sparkPlayer);
            
            //Save new Max score in sparkPlayer
            PlayerModel.getAndSetTournamentMaxScore(sparkPlayer, tournamentNewScore);

            //-- If we decide to use a scheduler then we can put this code there.
            // else {
            //     // Convert durationMinutes and waitTimeMinutes to milliseconds
            //     var duration = tournamentConfig.durationMinutes * 60 * 1000;
            //     var waitTime = tournamentConfig.waitTimeMinutes * 60 * 1000;
            //     liveTournament.startTime += duration + waitTime;
            //     TournamentLive.set(tournamentShortCode, liveTournament);
            // }

            return tournament;
        }

        return errors.TOURNAMENT_NOT_FOUND;
    };


*/