using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using PlayFab.Samples;
using SocialEdge.Server.Common.Utils;
using System.Collections.Generic;
using PlayFab.ServerModels;
using SocialEdge.Server.Api;
using SocialEdge.Server.DataService;
using Newtonsoft.Json.Linq;

namespace SocialEdge.Server.Requests
{
    public class ClaimReward
    {
        ITitleContext _titleContext;
        string _playerId;

        public ClaimReward(ITitleContext titleContext)
        {
            _titleContext = titleContext;
        }

        [FunctionName("ClaimReward")]
        public async Task<object> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            _playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            var data = args["data"];
            var rewardType = data["rewardType"].Value;
            var economyData = _titleContext.GetTitleDataProperty("Economy");
            var rewardsData = economyData["Rewards"];
            var rewardsTable = new Dictionary<string, object>() 
            {
                {"ratingBoostTier1",  rewardsData["ratingBoostTier1Reward"]},
                {"chestCoinsReward", rewardsData["chestCoinsReward"]},
                {"coinPurchaseReward", rewardsData["coinPurchaseReward"]},
                {"dailyReward", rewardsData["dailyReward"]},
                {"personalisedAdsGemReward", rewardsData["personalisedAdsGemReward"]},
                {"powerPlayReward", rewardsData["powerPlayReward"]},
                {"freeFullGameAnalysis", rewardsData["Settings.Rewards.freeFullGameAnalysis"]},
                {"ratingBoosterReward", rewardsData["rvBoosterReward"]},
                {"chestGemsReward", rewardsData["chestGemsReward"]},
                {"analysisReward", rewardsData["rvAnalysisReward"]},
                {"betCoinsReturn", rewardsData["betCoinsReturn"]}
            };

            var rewardPoints = rewardsTable[rewardType];
            if (rewardType == "chestCoinsReward" || rewardType == "chestGemsReward")
            { 
                Random rand = new Random();
                rewardPoints = rand.Next((int)rewardPoints["min"].Value, (int)rewardPoints["max"].Value + 1);

                var rewardChestT = await RewardChest(rewardType, rewardPoints, data);
                return rewardChestT;
            }

            return null;
        }
        private async Task<object> RewardChest(string rewardType, int amount, dynamic data)
        {
            var userData = data["userData"];
            var hotData = userData["hotData"];
            var economy = _titleContext.GetTitleDataProperty("Economy");
            var ads = economy["Ads"];
            long chestUnlockTimestamp = hotData["chestUnlockTimestamp"];
            long chestCooldownTimeInMin = ads["chestCooldownTimeInMin"];

            Dictionary<string, object> result = new Dictionary<string, object>();

            var currentTime = DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
            if (currentTime >= chestUnlockTimestamp)
            {
                var chestCooldownTimeSec = chestCooldownTimeInMin * 60 * 1000;
                
                AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
                request.Amount = amount;
                request.PlayFabId = _playerId;
                request.VirtualCurrency = rewardType == "chestCoinsReward" ? "CN" : "GM";

                var resultT = await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
                var chestUnlockTimestampUpdated = currentTime + chestCooldownTimeSec;
                hotData["chestUnlockTimestamp"] = chestUnlockTimestampUpdated;

                JObject jObj = new JObject() {["hotData"] = hotData };
                var resultUpdateT = await Player.UpdatePlayerData(_playerId, jObj);

                result.Add("claimRewardType", rewardType);
                result.Add("reward", amount);
                result.Add("chestUnlockTimestamp", chestUnlockTimestampUpdated);
            }
            else
            {
                result.Add("error", "invalidChestReward");
                result.Add("claimRewardType", rewardType);
                result.Add("coins", 0);// TODO
                result.Add("gems", 0);//TODO
                result.Add("chestUnlockTimestamp", chestUnlockTimestamp);
            }
                
            return result;
        }

        /*
               else if (rewardType ===  'dailyReward') {
            var leagueDailyRewardMsgId = Inbox.find(sparkPlayer, "RewardDailyLeague");
            
            if(leagueDailyRewardMsgId != null) {
                var inbox = InboxModel.get(sparkPlayer);
                var msg = inbox.messages[leagueDailyRewardMsgId];
        
                if (msg != undefined && currentTime >= msg.startTime) {
                    msg.startTime = moment(moment().endOf('day').toString()).valueOf();
                    msg.time = msg.startTime;
                    InboxModel.set(sparkPlayer);
            
                    var reward = Leagues.getDailyReward(playerData.pub.league);
                    var doubleReward = { gems: reward.gems*2, coins: reward.coins*2 };
                    var granted = Transactions.grant(sparkPlayer, doubleReward);
            
                    Spark.setScriptData('claimRewardType', rewardType);
                    Spark.setScriptData('reward', granted);
                }
                else {
                    Spark.setScriptError("error", "invalidDailyReward");
                    Spark.setScriptError("coins", sparkPlayer.getBalance4());
                    Spark.setScriptError("gems", sparkPlayer.getBalance3());
                    Spark.setScriptError("msgStartTime", msg.startTime);
                }
            }
        }
        */

    }

}
