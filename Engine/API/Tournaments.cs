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
using SocialEdgeSDK.Server.DataService;

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
            var playerEntry = socialEdgeTournament.TournamentEntryModel.Get(socialEdgePlayer.PlayerDBId, activeCollectionInfo.name);// TournamentCollectionManager.GetPlayerFromCurrentTournament(socialEdgePlayer, activeCollectionInfo.name);
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
            TournamentData tournamentModel = SetupTournamentModel(socialEdgeTournament, tournamentShortCode, liveTournament, currentStartTimeUTCSeconds, activeCollectionInfo.expiryTime, activeCollectionInfo.index);

            int tournamentMaxScore = socialEdgePlayer.PlayerModel.Tournament.tournamentMaxScore;
            var tournamentSlot = TournamentCollectionManager.GetSlot(liveTournament.slotsData, tournamentMaxScore);
            tournamentModel.tournamentSlot = tournamentSlot;

            var pool = TournamentPoolSample(tournamentShortCode, activeCollectionInfo.name, socialEdgePlayer, tournamentSlot, tournamentModel.joinedTime, liveTournament.maxPlayers - 1, null);
            tournamentModel.entryIds = pool;
            string tournamentId = socialEdgeTournament.TournamentModel.Id;
            var playerActiveTournament = socialEdgePlayer.PlayerModel.Tournament.CreatePlayerActiveTournament(tournamentId, tournamentModel, 1);

            return tournamentId;
        }

        private static TournamentData SetupTournamentModel(SocialEdgeTournamentContext socialEdgeTournament, string tournamentShortCode, TournamentLiveData liveTourament, long currentStartTime, long expiryTime, int tournamentCollectionIdx)
        {
            socialEdgeTournament.TournamentModel.Create();
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

            return socialEdgeTournament.TournamentModel.Tournament;  
        } 

        public static int GetPlayerRetentionDays(SocialEdgePlayerContext socialEdgePlayer)
        {
            // TODO: change combined info to a get Profile
            long creationTime = Utils.ToUTC(socialEdgePlayer.CombinedInfo.AccountInfo.Created);
            double retentionDays = (Utils.UTCNow() - creationTime) / (60*60*24*1000.0);
            return (int)Math.Floor(retentionDays);
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
            List<string> pool = new List<string>();
            // TODO Need to check algorithm to uncomment this
            //if (tournamentSlot <= 1) tournamentSlot = 1;
            if (poolSize <= 1) poolSize = 1;
            
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<TournamentEntryData>(collectionName);
            string playerId = socialEdgePlayer.PlayerId;
            List<ObjectId> alreadyIncludedArray = new List<ObjectId>(a_alreadyIncludedInArray != null ? a_alreadyIncludedInArray : new List<ObjectId>());
            alreadyIncludedArray.Add(ObjectId.Parse(socialEdgePlayer.PlayerDBId));
            
            // Check if user joined tournamnet on last day add the active players in leaderboards
            var daysLeft = GetTournamentDaysLeftWhenJoined(socialEdgePlayer, tournamentShortCode, joinedTime);
  
            FilterDefinition<TournamentEntryData> filter = Builders<TournamentEntryData>.Filter.Eq(typeof(TournamentEntryData).Name + ".tournamentSlot", tournamentSlot);
            filter = filter & Builders<TournamentEntryData>.Filter.Nin<ObjectId>("_id", alreadyIncludedArray);
            var sortByJoinTime = Builders<TournamentEntryData>.Sort.Ascending("joinTime");
            var sortByLastActive = Builders<TournamentEntryData>.Sort.Ascending("lastActiveTime");
            var projection = Builders<TournamentEntryData>.Projection.Include("_id");

            List<BsonDocument> poolEntries;
            if (daysLeft > 0)
                poolEntries = collection.Find(filter).Sort(sortByJoinTime).Sort(sortByLastActive).Project(projection).Limit(poolSize).ToList();
            else
                poolEntries = collection.Find(filter).Sort(sortByLastActive).Project(projection).Limit(poolSize).ToList();

            foreach(var poolEntry in poolEntries)
                pool.Add(poolEntry["_id"].ToString());

            // Filter out self and blocked
            pool = FilterFriends(socialEdgePlayer, pool);

            return pool;
        }

        public static void End(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, ActiveTournament activeTournament)
        {
            int tournamentNewScore = 0;
            TournamentData tournamentModel = socialEdgeTournament.TournamentModel.Tournament;

            if (activeTournament != null)
            {
                var tournamentLive = socialEdgeTournament.TournamentLiveModel.Get(tournamentModel.shortCode);
                string collectionName = tournamentLive.collectionPrefix + tournamentModel.tournamentCollectionIndex;
                List<string> entryIds = new List<string>(tournamentModel.entryIds);
                entryIds.Add(socialEdgePlayer.PlayerDBId);
                activeTournament.rank = TournamentCollectionManager.GetPlayerEntryRank(socialEdgePlayer, collectionName, entryIds);
                tournamentModel.score = activeTournament.score;
                tournamentNewScore = activeTournament.score;
            }

            tournamentModel.SetDirty();
            tournamentModel.concluded = true;

            List<TournamentReward> rewards = tournamentModel.rewards[socialEdgePlayer.MiniProfile.League.ToString()];
            TournamentReward reward = rewards.Find(x => activeTournament.rank >= x.minRank && activeTournament.rank <= x.maxRank);

            var i = socialEdgePlayer.Inbox.GetEnumerator();
            bool found = false;
            while(!found && i.MoveNext()) 
                found = i.Current.Value.type == "RewardTournamentEnd";

            if (!found)
            {
                InboxDataMessage msg = Inbox.CreateMessage();
                msg.type = "RewardTournamentEnd";
                msg.heading = "Tournament Results";
                msg.body = "Tournament";
                msg.trophies = reward.trophies;
                msg.rank = activeTournament.rank;
                msg.reward.Add("gems", reward.gems);
                msg.tournamentType = activeTournament.type;
                msg.tournamentId =  socialEdgeTournament.TournamentModel.Id;
                msg.chestType = reward.chestType;
                msg.expireAt = tournamentModel.expireAt;
                
                InboxModel.Add(msg, socialEdgePlayer);
            }

            if (tournamentNewScore > socialEdgePlayer.PlayerModel.Tournament.tournamentMaxScore)
                socialEdgePlayer.PlayerModel.Tournament.tournamentMaxScore = tournamentNewScore;
        }

        public static void UpdateTournaments(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament)
        {
            Dictionary<string, ActiveTournament> activeTournaments = socialEdgePlayer.PlayerModel.Tournament.activeTournaments;
            List<string> markedForDeletion = new List<string>();

            foreach(var item in activeTournaments)
            {
                string tournamentId = item.Key;
                ActiveTournament activeTournament = item.Value;

                TournamentData tournamentModel = socialEdgeTournament.TournamentModel.Get(tournamentId);
                if (tournamentModel != null)
                {
                    bool isEnded = (Utils.UTCNow() <  activeTournament.startTime) || // This case can occur if the tournament match ends after the tournament time is up
                                    (Utils.UTCNow() > (activeTournament.startTime + activeTournament.duration * 60 * 1000 ));
                    if (isEnded)
                    {
                        End(socialEdgePlayer, socialEdgeTournament, activeTournament);
                        markedForDeletion.Add(tournamentId);
                    }
                }
                else
                {
                    // Mark for deletion if tournament model was not found/deleted from db
                    markedForDeletion.Add(tournamentId);
                }
            }

            // Remove active tournaments marked for deletion
            foreach (string id in markedForDeletion)
                socialEdgePlayer.PlayerModel.Tournament.RemoveActiveTournament(id);

            // Automatically join the next tournament
            if (activeTournaments.Count == 0)
            {
                string tournamentShortCode = socialEdgeTournament.TournamentLiveModel.GetActiveShortCode(socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot);
                Join(socialEdgePlayer, socialEdgeTournament, tournamentShortCode, 0);
            }
        }

        public static List<TournamentLeaderboardEntry> GetLeaderboard(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, string tournamentId)
        {
            TournamentData tournamentModel = socialEdgeTournament.TournamentModel.Get(tournamentId);
            TournamentLiveData tournamentLive = socialEdgeTournament.TournamentLiveModel.Get(tournamentModel.shortCode);
            string collectionName = tournamentLive.collectionPrefix + tournamentModel.tournamentCollectionIndex.ToString();
            List<string> entryIds = new List<string>(tournamentModel.entryIds);
            entryIds.Add(socialEdgePlayer.PlayerDBId);
            List<TournamentLeaderboardEntry> leaderboardEntries = TournamentCollectionManager.GetSortedLeaderboardEntries(collectionName, entryIds);
            return leaderboardEntries;
        }

        public static void FillAvailablePoolEntries(string tournamentId, SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament)
        {
            TournamentData tournamentData = socialEdgeTournament.TournamentModel.Get(tournamentId);
            TournamentLiveData tournamentLiveData = socialEdgeTournament.TournamentLiveModel.Get(tournamentData.shortCode);
            if (tournamentData.entryIds.Count >= tournamentLiveData.maxPlayers)
                return;

            int remainPoolSize = tournamentLiveData.maxPlayers - tournamentData.entryIds.Count - 1;
            List<ObjectId> alreadyIncludedArray = new List<ObjectId>();

            foreach(var id in tournamentData.entryIds)
                alreadyIncludedArray.Add(ObjectId.Parse(id));

            List<string> pool = TournamentPoolSample(tournamentData.shortCode, tournamentLiveData.collectionPrefix + tournamentData.tournamentCollectionIndex.ToString(), 
                                        socialEdgePlayer, tournamentData.tournamentSlot, tournamentData.joinedTime, remainPoolSize, alreadyIncludedArray);
            if (pool.Count > 0)
                tournamentData.AppendEntryIds(pool);
        }

        public static bool HasTournamentEnded(TournamentData tournament)
        {
            // Calculate if a tournament has ended using start time and duration
            return (Utils.UTCNow() > (tournament.startTime + (tournament.duration * 60 * 1000)));
        }

        public static void HandleTournamentMatchEnd(ChallengePlayerModel challengePlayerModel, SocialEdgePlayerContext socialEdgePlayer, 
                                                    SocialEdgeTournamentContext socialEdgeTournament, bool isWin, bool isDraw)
        {
            if (string.IsNullOrEmpty(challengePlayerModel.tournamentId))
                return;

            int score = 0;
            LeagueSettingsData leagueSettings = SocialEdge.TitleContext.LeagueSettings.leagues[socialEdgePlayer.MiniProfile.League.ToString()];

            if (isWin) 
            {
                // Calc score and reward trophies
                float matchCoinsMultiplyer = float.Parse(Settings.CommonSettings["matchCoinsMultiplyer"].ToString());
                score = (int)(challengePlayerModel.betValue * matchCoinsMultiplyer);
                socialEdgePlayer.PlayerModel.Info.trophies = socialEdgePlayer.PlayerModel.Info.trophies + (challengePlayerModel.powerMode ? leagueSettings.trophies.win * 2 : leagueSettings.trophies.win);
                // Update Allstars leaderboard
                var taskT = Player.UpdatePlayerStatistics(socialEdgePlayer.PlayerId, "score", score);

                 socialEdgePlayer.PlayerModel.Economy.jackpotNotCollectedCounter = socialEdgePlayer.PlayerModel.Economy.jackpotNotCollectedCounter + 1;
                // jackpot probablity = jackpot not collected counter divided by 10
                // is jackpot = select a random number between 1 and 10, if random number is less or equal to the probability into 10
                int randNumber = Utils.GetRandomInteger(1, 10);
                double rewardJackpotProbability = socialEdgePlayer.PlayerModel.Economy.jackpotNotCollectedCounter >= 10 ? 1 : 
                                                        socialEdgePlayer.PlayerModel.Economy.jackpotNotCollectedCounter / 10;
                bool isJackpot = randNumber <= rewardJackpotProbability * 10;
                // e.g. total reward : reward 1 + reward 2 + reward 3 = 100
                // reward 1 = random or jackpot
                // reward 2 max = 100 minus reward 1 minus min of reward 3
                // reward 2 = random between reward 2 min and max
                // reward 3 = 100 minus reward 1 minus reward 2
                var bonusCoinsRewards = Settings.CommonSettings["bonusCoinsRewards"];
                int reward1Rand = Utils.GetRandomInteger((int)bonusCoinsRewards["reward1"][0], (int)bonusCoinsRewards["reward1"][1]);
                int reward1Round = (int)Utils.RoundToNearestMultiple(reward1Rand, 5);
                int reward1 = isJackpot ? (int)bonusCoinsRewards["reward1"][1] : reward1Round;
                double reward1Ratio = (double)reward1 / 100;
                isJackpot = (int)bonusCoinsRewards["reward1"][1] == reward1;
            
                int reward2Max = 100 - reward1 - (int)bonusCoinsRewards["reward3"][0];
                int reward2Rand = Utils.GetRandomInteger((int)bonusCoinsRewards["reward2"][0], reward2Max);
                int reward2 = (int)Utils.RoundToNearestMultiple((int)reward2Rand, 5);
                double reward2Ratio = (double)reward2 / 100;

                int reward3 = 100 - reward1 - reward2;
                double reward3Ratio = (double)reward3 / 100;
            
                double freeReward = challengePlayerModel.betValue * (double)Settings.CommonSettings["bonusCoinsFreeRatio"];
                double rvReward = challengePlayerModel.betValue * (double)Settings.CommonSettings["bonusCoinsRVRatio"];
            
                ChallengeWinnerBonusRewardsData rewards = new ChallengeWinnerBonusRewardsData();
                rewards.bonusCoinsFree1 = (int)Math.Round(freeReward * reward1Ratio);
                rewards.bonusCoinsFree2 = (int)Math.Round(freeReward * reward2Ratio);
                rewards.bonusCoinsFree3 = (int)Math.Round(freeReward * reward3Ratio);
                rewards.bonusCoinsRV1 = (int)Math.Round(rvReward * reward1Ratio);
                rewards.bonusCoinsRV2 = (int)Math.Round(rvReward * reward2Ratio);
                rewards.bonusCoinsRV3 = (int)Math.Round(rvReward * reward3Ratio);
                challengePlayerModel.winnerBonusRewards = rewards;

                if (isJackpot)
                    socialEdgePlayer.PlayerModel.Economy.jackpotNotCollectedCounter = 0;

                // Check for league promotion
                int nextLeague = socialEdgePlayer.MiniProfile.League + 1;
                if (nextLeague < SocialEdge.TitleContext.LeagueSettings.leagues.Count)
                {
                    LeagueSettingsData nextLeagueData = SocialEdge.TitleContext.LeagueSettings.leagues[nextLeague.ToString()];
                    if (socialEdgePlayer.PlayerModel.Info.trophies >= nextLeagueData.qualification.trophies)
                    {
                        socialEdgePlayer.MiniProfile.League = nextLeague;
                        socialEdgePlayer.PlayerModel.Info.trophies = socialEdgePlayer.PlayerModel.Info.trophies - nextLeagueData.qualification.trophies;
                        Inbox.SetupLeaguePromotion(nextLeague.ToString(), true, socialEdgePlayer);
                    }
                }
            }
            else if (!isDraw)
            {
                socialEdgePlayer.PlayerModel.Info.trophies = socialEdgePlayer.PlayerModel.Info.trophies - leagueSettings.trophies.loss;
                socialEdgePlayer.PlayerModel.Info.trophies = socialEdgePlayer.PlayerModel.Info.trophies < 0 ? 0 : socialEdgePlayer.PlayerModel.Info.trophies;
            }
        
            ActiveTournament activeTournament = socialEdgePlayer.PlayerModel.Tournament.activeTournaments.ContainsKey(challengePlayerModel.tournamentId) ? 
                                        socialEdgePlayer.PlayerModel.Tournament.activeTournaments[challengePlayerModel.tournamentId] : null;
            if (activeTournament != null) 
            {
                var tournament = socialEdgeTournament.TournamentModel.Get(challengePlayerModel.tournamentId);

                if (score > 0) 
                {
                    activeTournament.score = activeTournament.score + score;
                
                    // Report score to tournament collection entry
                    if (socialEdgePlayer.PlayerModel.Tournament.isReportingInChampionship) 
                    {
                        // TODO store prefix in tournament model!
                        string collectionName = socialEdgeTournament.TournamentLiveModel.Get(tournament.shortCode).collectionPrefix + tournament.tournamentCollectionIndex.ToString();
                        socialEdgeTournament.TournamentEntryModel.Get(socialEdgePlayer.PlayerDBId, collectionName).score = activeTournament.score;
                    }
                }
            
                activeTournament.matchesPlayedCount = activeTournament.matchesPlayedCount + 1;
            }
        }
    }
}
