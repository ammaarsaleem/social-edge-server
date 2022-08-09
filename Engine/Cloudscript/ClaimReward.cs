/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using PlayFab.Samples;
using MongoDB.Bson;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class ClaimRewardResult
    {
        public string error;
        public string claimRewardType;
        public long chestUnlockTimestamp;
        public int gems;
        public int coins;
        public long msgStartTime;
        public Dictionary<string, int> rewards;
    }

    public class ClaimReward : FunctionContext
    {
        public ClaimReward(ITitleContext titleContext) { Base(titleContext); }

        [FunctionName("ClaimReward")]
        public ClaimRewardResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
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
            ClaimRewardResult result = new ClaimRewardResult();

            // Coin chest and Gems chest
            if (rewardType == "chestCoinsReward" || rewardType == "chestGemsReward")
            { 
                Random rand = new Random();
                rewardPoints = rand.Next((int)rewardPoints["min"].Value, (int)rewardPoints["max"].Value + 1);

                result = RewardChest(SocialEdgePlayer, rewardType, rewardPoints, data, SocialEdgePlayer);
            }

            // Daily reward
            if (rewardType == "dailyReward")
            {
                result = RewardDaily("dailyReward", (int)rewardPoints, data, SocialEdgePlayer);
            }
            
            CacheFlush();
            return result;
        }

        private object RewardChest(SocialEdgePlayerContext socialEdgePlayer, string rewardType, int amount, dynamic data, SocialEdgePlayerContext playerContext)
        {
            var economy = SocialEdge.TitleContext.GetTitleDataProperty("Economy");
            var ads = economy["Ads"];
            long chestUnlockTimestamp = socialEdgePlayer.PlayerModel.Economy.chestUnlockTimestamp;
            var chestCooldownTimeInMin = Convert.ToInt64(ads["chestCooldownTimeInMin"]);

            ClaimRewardResult result = new ClaimRewardResult();

            var currentTime = Utils.UTCNow();
            if (currentTime >= chestUnlockTimestamp)
            {
                var chestCooldownTimeSec = chestCooldownTimeInMin * 60 * 1000;
                SocialEdgePlayer.PlayerEconomy.AddVirtualCurrency(rewardType == "chestCoinsReward" ? "CN" : "GM", amount);
                socialEdgePlayer.PlayerModel.Economy.chestUnlockTimestamp = currentTime + chestCooldownTimeSec;
                result.claimRewardType = rewardType;
                result.rewards = new Dictionary<string, int>();
                result.rewards.Add(rewardType == "chestCoinsReward" ? "coins" : "gems", amount);
                result.chestUnlockTimestamp = socialEdgePlayer.PlayerModel.Economy.chestUnlockTimestamp;
            }
            else
            {
                // TODO avoid unnecessary requests
                var coins = playerContext.VirtualCurrency["CN"];
                var gems = playerContext.VirtualCurrency["GM"];
                result.error = "invalidChestReward";
                result.claimRewardType = rewardType;
                result.coins = coins;
                result.gems = gems;
                result.chestUnlockTimestamp = chestUnlockTimestamp;
            }
                
            return result;
        }

        private object RewardDaily(string rewardType, int amount, object data, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            string leagueDailyRewardMsgId = InboxModel.FindOne("RewardDailyLeague", socialEdgePlayer);

            if (leagueDailyRewardMsgId != null)
            {
                Dictionary<string, InboxDataMessage> inbox = socialEdgePlayer.Inbox;
                if (inbox.ContainsKey(leagueDailyRewardMsgId))
                {
                    InboxDataMessage msg = inbox[leagueDailyRewardMsgId];
                    long startTime = msg.startTime;
                    if (false && Utils.UTCNow() >= startTime)
                    {
                        msg.startTime = Utils.ToUTC(Utils.EndOfDay(DateTime.Now));
                        msg.time = msg.startTime;
                        var taskInboxT = InboxModel.Set(socialEdgePlayer.InboxId, socialEdgePlayer.Inbox);
            
                        var reward = Leagues.GetDailyReward(socialEdgePlayer.PublicData["leag"].ToString());
                        var doubleReward = new BsonDocument() { ["gems"] = (int)reward["gems"] * 2, ["coins"] = (int)reward["coins"] * 2 };
                        var taskT = Transactions.Grant(doubleReward, socialEdgePlayer);
                        taskT.Wait();
                        var granted = taskT.Result;
            
                        result.claimRewardType = rewardType;
                        result.rewards = new Dictionary<string, int>();
                        if (granted.ContainsKey("coins"))
                            result.rewards.Add("coins", (int)granted["coins"]);

                        if (granted.ContainsKey("gems"))
                            result.rewards.Add("gems", (int)granted["gems"]);
                    }
                    else
                    {
                        // TODO avoid unnecessary requests
                        var coins = socialEdgePlayer.VirtualCurrency["CN"];
                        var gems = socialEdgePlayer.VirtualCurrency["GM"];
                        result.error = "invalidDailyReward";
                        result.coins = coins;
                        result.gems = gems;
                        result.msgStartTime = msg.startTime;
                    }                                   
                }
            }
            return result;
        }
    }
}
