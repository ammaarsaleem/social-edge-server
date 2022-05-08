/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab;
using System.Net;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Playfab
{
    public class PlayerHotDataSegment
    {
        public string tag;
        public int eloCompletedPlacementGames;
        public int eloScore;
        public int gamesWon;
        public int gamesLost;
        public int gamesDrawn;
        public string countryFlag;
        public int league;
        public int trophies;
        public int trophies2;
        public string editedName;
        public int totalGamesCount;

        public int shopRvRewardClaimedCount;
        public int balloonRewardsClaimedCount;
        public int outOfGemsSessionCount;
        public int cpuPowerupUsedCount;
        public int totalPowerupUsageCount;
        public int chestUnlockTimestamp;
        public int rvUnlockTimestamp;
        public int piggyBankExpiryTimestamp;
        public int piggyBankDoublerExipryTimestamp;
        public int freePowerPlayExipryTimestamp;
        public int shopRvRewardCooldownTimestamp;
        public int lastBundleUpdatePlayDay;

        public string adsRewardData;
        public int eventTimeStamp;
        public string lastWatchedVideoId;

        public int dailyEventExpiryTimestamp;
        public string dailyEventRewards;
        public int dailyEventProgress;
        public int dailyEventState;

        public bool isReportingInChampionship;
        public int reportingChampionshipCollectionIndex;
        public string activeTournaments;
        public string currentChallengeId;
        public string activeChallenges;
        public string pendingChallenges;

/*
        // Private Data
        X {"playerActiveInventory", null},
        X {"isBot", "false"},
        X {"botDifficulty", "0"},
        X {"friends", null},
        X {"community", null},
        X {"blocked", null},
        X {"removed", null},
*/
        public PlayerHotDataSegment()
        {
            tag = null;
            eloCompletedPlacementGames = 0;
            eloScore = 0;
            gamesWon = 0;
            gamesLost = 0;
            gamesDrawn = 0;
            countryFlag = null;
            league = 0;
            trophies = 0 ;
            trophies2 = 0;
            editedName = null;
            totalGamesCount = 0;;

            shopRvRewardClaimedCount = 0;
            balloonRewardsClaimedCount = 0;
            outOfGemsSessionCount = 0;
            cpuPowerupUsedCount = 0;
            totalPowerupUsageCount = 0;
            chestUnlockTimestamp = 0;
            rvUnlockTimestamp = 0;
            piggyBankExpiryTimestamp = 0;
            piggyBankDoublerExipryTimestamp = 0;
            freePowerPlayExipryTimestamp = 0;
            shopRvRewardCooldownTimestamp = 0;
            lastBundleUpdatePlayDay = 0;

            adsRewardData = null;
            eventTimeStamp = 0;
            lastWatchedVideoId = null;

            dailyEventExpiryTimestamp = 0;
            dailyEventRewards = null;
            dailyEventProgress = 0;
            dailyEventState = 0;

            isReportingInChampionship = false;
            reportingChampionshipCollectionIndex = -1;
            activeTournaments = null;
            currentChallengeId = null;
            activeChallenges = null;
            pendingChallenges = null;
        }
    }

    public class PlayerColdDataSegment
    {
        public bool isInitialized;
        public string clientVersion;
        public bool firstLongMatchCompleted;
        public bool isSearchRegistered;
        public bool isFBConnectRewardClaimed;
        public int playerTimeZoneSlot;
        public int removeAdsTimeStamp;
        public int removeAdsTimePeriod;
        public bool isPremium;
        public int subscriptionExpiryTime;
        public string subscriptionType;
        public string dynamicBundlePurchaseTier;
        public string dynamicBundleDisplayTier;
        public string dynamicBundlePurchaseTierNew;
        public int eloCompletedPlacementGames;
        public bool careerLeagueSet;
        public string uploadedPicId;

        public PlayerColdDataSegment()
        {
            isInitialized = false;
            clientVersion = "0.0.0";
            firstLongMatchCompleted = false;
            isSearchRegistered = false;
            isFBConnectRewardClaimed = false;
            playerTimeZoneSlot = 0;
            removeAdsTimeStamp = 0;
            removeAdsTimePeriod = 0;
            isPremium = false;
            subscriptionExpiryTime = 0;
            subscriptionType = null;
            dynamicBundlePurchaseTier = null;
            dynamicBundleDisplayTier = null;
            dynamicBundlePurchaseTierNew = null;
            eloCompletedPlacementGames = 0;
            careerLeagueSet = false;
            uploadedPicId = null;         
        }
    }

    public class Playstream_OnPlayerCreated
    {
        [FunctionName("Playstream_OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdge.Init(req);
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;

            string hashCode = context.PlayerProfile.PlayerId.GetHashCode().ToString();
            string newDisplayName = "NewGuest" + hashCode;
            var request = new PlayFab.AdminModels.UpdateUserTitleDisplayNameRequest
            {
                PlayFabId = context.PlayerProfile.PlayerId,
                DisplayName = newDisplayName
            };

            log.LogInformation("Noor sent a log");
            var result = await PlayFabAdminAPI.UpdateUserTitleDisplayNameAsync(request);
            log.LogDebug(JsonConvert.SerializeObject(result));

            PlayerHotDataSegment playerHotDataSegment = new PlayerHotDataSegment();
            string playerHotDataSegmentJSon = JsonConvert.SerializeObject(playerHotDataSegment);
            log.LogInformation("Serialized playerHotDataSegmentJSon : " + playerHotDataSegmentJSon);

            PlayerColdDataSegment playerColdDataSegment = new PlayerColdDataSegment();
            string playerColdDataSegmentJSon = JsonConvert.SerializeObject(playerColdDataSegment);
            log.LogInformation("Serialized playerColdDataSegmentJSon : " + playerColdDataSegmentJSon);

            PlayFabResult<UpdateUserDataResult> updatePlayerDataResult = null;
            var updateReadOnlyDataReq = new UpdateUserDataRequest()
            {
                 Data = new Dictionary<string, string>
                 {
                    {"hotData", playerHotDataSegmentJSon},
                    {"coldData", playerColdDataSegmentJSon}
                 },

                 PlayFabId = context.PlayerProfile.PlayerId,
             };
             updatePlayerDataResult = await PlayFabServerAPI.UpdateUserReadOnlyDataAsync(updateReadOnlyDataReq);

              log.LogDebug(JsonConvert.SerializeObject(updatePlayerDataResult));
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

