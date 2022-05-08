/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using MongoDB.Bson;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Api;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class ClaimReward : FunctionContext
    {
        public ClaimReward(ITitleContext titleContext) { Base(titleContext); }

        [FunctionName("ClaimReward")]
        public async Task<object> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext(req, log);
            var data = Args["data"];
            var rewardType = data["rewardType"].Value;
            var economyData = SocialEdge.TitleContext.GetTitleDataProperty("Economy");
            var rewardsData = economyData["Rewards"];
            var rewardsTable = new Dictionary<string, object>() 
            {
                {"ratingBoostTier1",  rewardsData["ratingBoostTier1Reward"]},
                {"chestCoinsReward", rewardsData["chestCoinsReward"]},
                {"coinPurchaseReward", rewardsData["coinPurchaseReward"]},
                {"dailyReward", rewardsData["dailyReward"]},
                {"personalisedAdsGemReward", rewardsData["personalisedAdsGemReward"]},
                {"powerPlayReward", rewardsData["powerPlayReward"]},
               // TODO {"freeFullGameAnalysis", rewardsData[Settings.Rewards.freeFullGameAnalysis]}, 
                {"ratingBoosterReward", rewardsData["rvBoosterReward"]},
                {"chestGemsReward", rewardsData["chestGemsReward"]},
                {"analysisReward", rewardsData["rvAnalysisReward"]},
                {"betCoinsReturn", rewardsData["betCoinsReturn"]}
            };

            var rewardPoints = rewardsTable[rewardType];

            // Coin chest and Gems chest
            if (rewardType == "chestCoinsReward" || rewardType == "chestGemsReward")
            { 
                Random rand = new Random();
                rewardPoints = rand.Next((int)rewardPoints["min"].Value, (int)rewardPoints["max"].Value + 1);

                return await RewardChest(rewardType, rewardPoints, data, SocialEdgePlayer);
            }

            // Daily reward
            if (rewardType == "dailyReward")
            {
                return await RewardDaily("dailyReward", (int)rewardPoints, data, SocialEdgePlayer);
            }
            
            return null;
        }

        private async Task<object> RewardChest(string rewardType, int amount, dynamic data, SocialEdgePlayerContext playerContext)
        {
            var userData = data["userData"];
            var hotData = userData["hotData"];
            var economy = SocialEdge.TitleContext.GetTitleDataProperty("Economy");
            var ads = economy["Ads"];
            long chestUnlockTimestamp = hotData["chestUnlockTimestamp"];
            var chestCooldownTimeInMin = Convert.ToInt64(ads["chestCooldownTimeInMin"]);

            Dictionary<string, object> result = new Dictionary<string, object>();

            var currentTime = Utils.UTCNow();
            if (currentTime >= chestUnlockTimestamp)
            {
                var chestCooldownTimeSec = chestCooldownTimeInMin * 60 * 1000;
                
                AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
                request.Amount = amount;
                request.PlayFabId = playerContext.PlayerId;
                request.VirtualCurrency = rewardType == "chestCoinsReward" ? "CN" : "GM";

                var resultT = await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
                var chestUnlockTimestampUpdated = currentTime + chestCooldownTimeSec;
                hotData["chestUnlockTimestamp"] = chestUnlockTimestampUpdated;

                BsonDocument obj = new BsonDocument() { ["hotData"] = hotData.ToString() };
                var resultUpdateT = await Player.UpdatePlayerData(playerContext.PlayerId, obj);

                result.Add("claimRewardType", rewardType);
                result.Add("reward", amount);
                result.Add("chestUnlockTimestamp", chestUnlockTimestampUpdated);
            }
            else
            {
                // TODO avoid unnecessary requests
                var coins = playerContext.VirtualCurrency["CN"];
                var gems = playerContext.VirtualCurrency["GM"];
                result.Add("error", "invalidChestReward");
                result.Add("claimRewardType", rewardType);
                result.Add("coins", coins);
                result.Add("gems", gems);
                result.Add("chestUnlockTimestamp", chestUnlockTimestamp);
            }
                
            return result;
        }

        private async Task<object> RewardDaily(string rewardType, int amount, object data, SocialEdgePlayerContext playerContext)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            string leagueDailyRewardMsgId = InboxModel.FindOne(playerContext.Inbox, "RewardDailyLeague");

            if (leagueDailyRewardMsgId != null)
            {
                var inbox = playerContext.Inbox;
                if (inbox["messages"].AsBsonDocument.Contains(leagueDailyRewardMsgId))
                {
                    var msg = inbox["messages"][leagueDailyRewardMsgId];
                    var startTime = Convert.ToInt64(msg["startTime"]);
                    if (false && Utils.UTCNow() >= startTime)
                    {
                        msg["tartTime"] = Utils.EndOfDay(DateTime.Now);
                        msg["time"] = msg["startTime"];
                        await InboxModel.Set(playerContext.InboxId, playerContext.Inbox);
            
                        var reward = Leagues.GetDailyReward(playerContext.PublicData["leag"].ToString());
                        var doubleReward = new BsonDocument() { ["gems"] = (int)reward["gems"] * 2, ["coins"] = (int)reward["coins"] * 2 };
                        var granted = await Transactions.Grant(doubleReward, playerContext);
            
                        result.Add("claimRewardType", rewardType);
                        result.Add("reward", granted);
                    }
                    else
                    {
                        // TODO avoid unnecessary requests
                        
                        var playerinventoryResult = await Player.GetPlayerInventory(playerContext.PlayerId);
                        var coins = playerContext.VirtualCurrency["CN"];
                        var gems = playerContext.VirtualCurrency["GM"];
                        result.Add("error", "invalidDailyReward");
                        result.Add("coins", coins);
                        result.Add("gems", gems);
                        result.Add("msgStartTime", msg["startTime"]);
                    }                                   
                }
            }
            return result;
        }
    }
}
