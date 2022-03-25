using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using PlayFab.Json;
using System.Collections.Generic;
using PlayFab.DataModels;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
// using PlayFab.Plugins.CloudScript;
using PlayFab.Samples;
using SocialEdge.Server.Constants;
using PlayFab;
using System.Net;
using SocialEdge.Server.Common.Utils;
namespace SocialEdge.Playfab
{
    public class Playstream_OnPlayerCreated
    {
        [FunctionName("Playstream_OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
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

            PlayFabResult<UpdateUserDataResult> updatePlayerDataResult = null;
            var updateReadOnlyDataReq = new UpdateUserDataRequest()
            {
                 Data = new Dictionary<string, string>
                 {
                    // Public Profile
                    {"tag", null},
                    {"eloCompletedPlacementGames", "0"},
                    {"eloScore", "0"},
                    {"gamesWon", "0"},
                    {"gamesLost", "0"},
                    {"gamesDrawn", "0"},
                    {"countryFlag", null},
                    {"league", "0"},
                    {"trophies", "0"},
                    {"trophies2", "0"},

                    // Private Data
                    {"adLifetimeImpressions", "0"},
                    {"playerActiveInventory", null},
                    {"isInitialized", "false"},
                    {"isBot", "false"},
                    {"botDifficulty", "0"},
                    {"currentChallengeId", null},
                    {"friends", null},
                    {"community", null},
                    {"blocked", null},
				    {"removed", null},
                    {"activeChallenges", null},
                    {"pendingChallenges", null},
                    {"firstLongMatchCompleted", "false"},
                    {"removeAdsTimeStamp", "0"},
                    {"removeAdsTimePeriod", "0"},
                    {"isPremium", "false"},
                    {"isSearchRegistered", "false"},
                    {"clientVersion", "0.0.0"},
                    {"editedName", ""},
                    {"isFBConnectRewardClaimed", "false"},
                    {"cpuPowerupUsedCount", "0"},
                    {"totalGamesCount", "0"},
                    {"totalPowerupUsageCount", "0"},
                    {"eventTimeStamp", "0"},
                    {"subscriptionExpiryTime", "0"},
                    {"subscriptionType", ""},
                    {"adsRewardData", null},
                    {"lastWatchedVideoId", ""},
                    {"uploadedPicId", ""},
                    {"activeTournaments", null},
                    {"careerLeagueSet", "false"},
                    {"isReportingInChampionship", "false"},
                    {"reportingChampionshipCollectionIndex", "-1"},
                    {"chestUnlockTimestamp", "0"},
                    {"rvUnlockTimestamp", "0"},
                    {"dynamicBundlePurchaseTier", ""},
                    {"dynamicBundleDisplayTier", ""},
                    {"outOfGemsSessionCount", "0"},
                    {"dynamicBundlePurchaseTierNew", ""},
                    {"lastBundleUpdatePlayDay", "0"},
                    {"playerTimeZoneSlot", "0"},
                    {"piggyBankExpiryTimestamp", "0"},
                    {"balloonRewardsClaimedCount", "0"},
                    {"freePowerPlayExipryTimestamp", "0"},
                    {"piggyBankDoublerExipryTimestamp", "0"},
                    {"shopRvRewardClaimedCount", "0"},
                    {"shopRvRewardCooldownTimestamp", "0"},
                    {"dailyEventExpiryTimestamp", "0"},
                    {"dailyEventRewards", null},
                    {"dailyEventProgress", "0"},
                    {"dailyEventState", ""}
                 },

                 PlayFabId = context.PlayerProfile.PlayerId,
             };
             updatePlayerDataResult = await PlayFabServerAPI.UpdateUserReadOnlyDataAsync(updateReadOnlyDataReq);

              log.LogDebug(JsonConvert.SerializeObject(updatePlayerDataResult));
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

