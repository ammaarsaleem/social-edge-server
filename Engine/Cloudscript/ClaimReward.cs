/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
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
        public Dictionary<string, long> rewardsLong;
        public long shopRvRewardCooldownTimestamp;
        public int shopRvMaxReward;
        public int ratingBoostTier1Reward;
        public int ratingBoosterReward;
        public long rvUnlockTimestamp;
        public int eloScore;
        public EconomyBalloonReward balloonReward;
        public string dailyEventState;
        public int dailyEventRing;
        public PlayerDataEvent playerDataEvent;
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
            string rewardType = data["rewardType"].Value.ToString();

            EconomySettingsModel economySettings = SocialEdge.TitleContext.EconomySettings;
            EconomyRewards rewardsData = economySettings.Rewards;
            var rewardsTable = new Dictionary<string, object>() 
            {
                {"ratingBoostTier1",  rewardsData.ratingBoostTier1Reward},
                {"chestCoinsReward", rewardsData.chestCoinsReward},
                {"coinPurchaseReward", rewardsData.coinPurchaseReward},
                {"dailyReward", rewardsData.dailyReward},
                {"personalisedAdsGemReward", rewardsData.personalisedAdsGemReward},
                {"powerPlayReward", rewardsData.powerPlayReward},
                {"freeFullGameAnalysis", rewardsData.freeFullGameAnalysis}, 
                {"ratingBoosterReward", rewardsData.rvBoosterReward},
                {"chestGemsReward", rewardsData.chestGemsReward},
                {"analysisReward", rewardsData.rvAnalysisReward},
                {"betCoinsReturn", rewardsData.betCoinsReturn}
            };

            ClaimRewardResult result = new ClaimRewardResult();

            // Coin chest and Gems chest
            if (rewardType == "chestCoinsReward" || rewardType == "chestGemsReward")
            { 
                Random rand = new Random();
                EconomyMinMax reward = (EconomyMinMax)rewardsTable[rewardType];
                int rewardPoints = rand.Next(reward.min, reward.max + 1);

                result = RewardChest(SocialEdgePlayer, rewardType, rewardPoints, data, SocialEdgePlayer);
            }
            else if (rewardType == "dailyReward")
            {
                int rewardPoints = int.Parse(rewardsTable[rewardType].ToString());
                result = RewardDaily("dailyReward", rewardPoints, data, SocialEdgePlayer);
            }
            else if (rewardType == "shopRvReward")
            {
                result = RewardShopRV(rewardType, SocialEdgePlayer);
            }
            else if (rewardType == "ratingBoostTier1")
            {   
                string challengeId = data["userData"]["BaseData"]["challengeId"].ToString();
                int rewardPoints = int.Parse(rewardsTable[rewardType].ToString());
                result = RewardRatingBoostTier1(rewardType, challengeId, rewardPoints, SocialEdgePlayer, SocialEdgeChallenge);
            }
            else if (rewardType == "coinPurchaseReward") 
            {
                int rewardPoints = int.Parse(rewardsTable[rewardType].ToString());
                result = CoinPurchaseReward(rewardType, rewardPoints, SocialEdgePlayer);
            }
            else if (rewardType == "powerPlayReward" || rewardType == "ratingBoosterReward" || rewardType == "analysisReward") 
            {
                int rewardPoints = int.Parse(rewardsTable[rewardType].ToString());
                result = ValidateFreeRVReward(rewardType, data, rewardPoints, SocialEdgePlayer, SocialEdgeChallenge);
            }
            else if (rewardType == "freeFullGameAnalysis") 
            {
                int rewardPoints = int.Parse(rewardsTable[rewardType].ToString());
                result = FreeFullGameAnalysis(rewardType, rewardPoints, SocialEdgePlayer);
            }
            else if (rewardType == "betCoinsReturn") 
            {
                string challengeId = data["userData"]["BaseData"]["challengeId"].ToString();
                result = BetCoinsReturn(rewardType, challengeId, SocialEdgePlayer, SocialEdgeChallenge);
            }
            else if(rewardType.Contains("bonusCoins")) 
            {
                string challengeId = data["userData"]["BaseData"]["challengeId"].ToString();
                result = BonusCoins(rewardType, challengeId, SocialEdgePlayer, SocialEdgeChallenge);
            }
            else if (rewardType == "balloonRVRewards") 
            {
                //int defaultBet = int.Parse(data["defaultBet"].ToString());
                result = BalloonRVRewards(rewardType, SocialEdgePlayer);
            }
            else if (rewardType == "balloonCoins") 
            {
                result = BalloonCoins(rewardType, SocialEdgePlayer);
            }
            else if (rewardType == "balloonGems") 
            {
                result = BalloonGems(rewardType, SocialEdgePlayer);
            }
            else if (rewardType == "balloonPowerPlayMins") 
            {
                result = BalloonPowerPlayMins(rewardType, SocialEdgePlayer);
            }
            else if (rewardType == "balloonPiggyBankMins") 
            {
                result = BalloonPiggyBankMins(rewardType, SocialEdgePlayer);
            }
            else if (rewardType == "dailyEventContinue") 
            {
                result = DailyEventContinue(rewardType, SocialEdgePlayer);
            }
            else if (rewardType == "dailyEventReward") 
            {
                result = DailyEventReward(rewardType, SocialEdgePlayer);
            }

            CacheFlush();
            return result;
        }

        private ClaimRewardResult RewardRatingBoostTier1(string rewardType, string challengeId, int rewardPoints, SocialEdgePlayerContext socialEdgePlayer, SocialEdgeChallengeContext socialEdgeChallenge)
        {
            ChallengeData challengeData = socialEdgeChallenge.ChallengeModel.Get(challengeId);
            socialEdgeChallenge.ChallengeModel.ReadOnly(challengeId);
            ChallengePlayerModel playerChallengeData = challengeData.playersData[socialEdgePlayer.PlayerId];
            int eloChange = playerChallengeData.eloChange;
            bool isRatingBoosterUsed = false;// playerChallengeData.ratingBoosterUsed;

            ClaimRewardResult result = new ClaimRewardResult();
            
            if (isRatingBoosterUsed == false) 
            {
                if (eloChange < 0 && Math.Abs(eloChange) < rewardPoints)
                {
                    rewardPoints = Math.Abs(eloChange);
                }
                
                CatalogItem ratingBoosterItem = SocialEdge.TitleContext.GetCatalogItem("SpecialItemRatingBooster");
                if (socialEdgePlayer.VirtualCurrency["GM"] < ratingBoosterItem.VirtualCurrencyPrices["GM"])
                {
                    result.error = "gemsInsufficient";
                    result.gems = socialEdgePlayer.VirtualCurrency["GM"];
                }
                else 
                {
                    Player.PurchaseItem(socialEdgePlayer.PlayerId, "SpecialItemRatingBooster", (int)ratingBoosterItem.VirtualCurrencyPrices["GM"], "GM");
                    socialEdgePlayer.PlayerModel.Info.eloScore = socialEdgePlayer.PlayerModel.Info.eloScore + rewardPoints;
                    //playerChallengeData.ratingBoosterUsed = true;
                    result.claimRewardType = rewardType;
                    result.ratingBoostTier1Reward = rewardPoints;
                    result.gems = (int)ratingBoosterItem.VirtualCurrencyPrices["GM"];
                }
            }

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
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency(rewardType == "chestCoinsReward" ? "CN" : "GM", amount);
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

        private ClaimRewardResult RewardDaily(string rewardType, int amount, object data, SocialEdgePlayerContext socialEdgePlayer)
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
                    if (Utils.UTCNow() >= startTime)
                    {
                        msg.startTime = Utils.ToUTC(Utils.EndOfDay(DateTime.Now));
                        msg.time = msg.startTime;
                        var taskInboxT = InboxModel.Set(socialEdgePlayer.InboxId, socialEdgePlayer.Inbox);
            
                        var reward = Leagues.GetDailyReward(socialEdgePlayer.MiniProfile.League.ToString());
                        int numCoins = (int)reward["coins"] * 2;
                        int numGems = (int)reward["gems"] * 2;
                        socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", numCoins);
                        socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("GM", numGems);
            
                        result.claimRewardType = rewardType;
                        result.rewards = new Dictionary<string, int>();
                        result.rewards.Add("coins", numCoins);
                        result.rewards.Add("gems", numGems);
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

        public ClaimRewardResult RewardShopRV(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            PlayerDataEconomy playerDataEconomy = socialEdgePlayer.PlayerModel.Economy;
            EconomySettingsModel economySettings = SocialEdge.TitleContext.EconomySettings;

            if (Utils.UTCNow() >= socialEdgePlayer.PlayerModel.Economy.shopRvRewardCooldownTimestamp)
            {
                int shopRvRewardNotClaimedDays = socialEdgePlayer.PlayerModel.Info.playDays - playerDataEconomy.shopRVRewardClaimedDay;
                var rewardsProbablity = economySettings.balloonRewardsProbability;
                //var rewardsProbablity = Settings.Economy["balloonRewardsProbability"];

                var selectedRewardType = playerDataEconomy.shopRvRewardClaimedCount == 0 || shopRvRewardNotClaimedDays >= 5 ? "A" :
                            rewardsProbablity[ Utils.GetRandomInteger(0, rewardsProbablity.Count - 1) ];
                EconomyBalloonReward selectedReward = economySettings.balloonRewards[selectedRewardType];
                var defaultBet = playerDataEconomy.shopRvDefaultBet;
                int rewardCoins = selectedReward.balloonCoins;
    
                if (defaultBet > 10000) 
                    rewardCoins = (int)(defaultBet * selectedReward.coinsRewardRatio);
                
                playerDataEconomy.shopRvMaxReward = 0;
                int shopRvMaxReward = socialEdgePlayer.PlayerEconomy.ProcessShopRvMaxReward();
                
                result.rewards = new Dictionary<string, int>();
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", rewardCoins);
                var coolDownTime = economySettings.ShopRVRewards.cooldownInMins * 60 * 1000;

                playerDataEconomy.shopRvRewardClaimedCount = playerDataEconomy.shopRvRewardClaimedCount + 1;
                playerDataEconomy.shopRvRewardCooldownTimestamp = Utils.UTCNow() + coolDownTime;
                playerDataEconomy.shopRVRewardClaimedDay = socialEdgePlayer.PlayerModel.Info.playDays;

                result.claimRewardType = rewardType;
                result.rewards.Add("coins", rewardCoins);
                result.shopRvRewardCooldownTimestamp = playerDataEconomy.shopRvRewardCooldownTimestamp;
                result.shopRvMaxReward = shopRvMaxReward;
            }
            else
            {
                result.error = "invalidShopRvReward";
                result.claimRewardType = rewardType;
                result.shopRvRewardCooldownTimestamp = playerDataEconomy.shopRvRewardCooldownTimestamp;
            }

            return result;
        }

        private ClaimRewardResult ValidateFreeRVReward(string rewardType, dynamic data, int rewardPoints, SocialEdgePlayerContext socialEdgePlayer, SocialEdgeChallengeContext socialEdgeChallenge)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            bool isValidForRVReward = socialEdgePlayer.PlayerEconomy.IsValidForRVReward();
            int rvCooldownTimeSec = SocialEdge.TitleContext.EconomySettings.Ads.freemiumTimerCooldownTimeInMin * 60 * 1000;

            if (Utils.UTCNow() >= socialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp && isValidForRVReward)
            {
                string rewardItemId = "SpecialItemPowerMode";
                
                if (rewardType == "ratingBoosterReward") 
                {
                    string challengeId = data["userData"]["BaseData"]["challengeId"].ToString();
                    ChallengeData challengeData = socialEdgeChallenge.ChallengeModel.Get(challengeId);
                    socialEdgeChallenge.ChallengeModel.ReadOnly(challengeId);
                    ChallengePlayerModel playerChallengeData = challengeData.playersData[socialEdgePlayer.PlayerId];
                    bool isRatingBoosterUsed = false;// playerChallengeData.ratingBoosterUsed;
                    //var challengeData = getPlayerChallenge(challengeId);
                    rewardItemId = "SpecialItemRatingBooster";
                    //if (isRatingBoosterUsed(playerId, challengeData) == true) {
                    //Spark.exit();
                    int eloChange = playerChallengeData.eloChange;
                    if (eloChange < 0 && Math.Abs(eloChange) < rewardPoints) 
                    {
                        rewardPoints = Math.Abs(eloChange);
                    }                
                    socialEdgePlayer.PlayerModel.Info.eloScore = socialEdgePlayer.PlayerModel.Info.eloScore + rewardPoints;
                    result.ratingBoosterReward = rewardPoints;
                    //Challenge.updateRatingBoosterUsed(Spark, challengeId, playerId);
                }
                else if (rewardType == "analysisReward") 
                {
                    rewardItemId = "FullGameAnalysis";
                }

                socialEdgePlayer.PlayerEconomy.Grant(rewardItemId, 1);
                socialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp = Utils.UTCNow() + rvCooldownTimeSec;

                result.rewards = new Dictionary<string, int>();
                result.claimRewardType = rewardType;
                result.rewards.Add(rewardItemId, rewardPoints);
                result.rvUnlockTimestamp = socialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp;
            }
            else 
            {
                result.error ="invalidRVReward";
                result.claimRewardType = rewardType;
                result.rvUnlockTimestamp = socialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp;
                result.gems = socialEdgePlayer.VirtualCurrency["GM"];
                result.eloScore =  socialEdgePlayer.PlayerModel.Info.eloScore;
            }

            return result;
        }

        public ClaimRewardResult FreeFullGameAnalysis(string rewardType, int rewardPoints, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            string itemId = "FullGameAnalysis";
            int gameAnalysisBought = socialEdgePlayer.PlayerEconomy.GetNumVGoods(itemId);

            if (gameAnalysisBought < rewardPoints) 
            {
                result.rewards = new Dictionary<string, int> ();
                socialEdgePlayer.PlayerEconomy.Grant(itemId, 1);
                result.claimRewardType = rewardType;
                result.rewards.Add("FullGameAnalysis", 1);
            } 
            else 
            {
                result.error = "Invalid Reward : " + rewardType;
            }

            return result;
        }

        public ClaimRewardResult BetCoinsReturn(string rewardType, string challengeId, SocialEdgePlayerContext socialEdgePlayer, SocialEdgeChallengeContext socialEdgeChallenge)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            ChallengeData challengeData = socialEdgeChallenge.ChallengeModel.Get(challengeId);
            ChallengePlayerModel playerChallengeData = challengeData.playersData[socialEdgePlayer.PlayerId];

            long betValue = playerChallengeData.betValue;
            int betReturn = (int)Math.Round(betValue * double.Parse(Settings.CommonSettings["betLossAversionRatio"].ToString()));
            socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", betReturn);

            result.rewards = new Dictionary<string, int> ();
            result.claimRewardType = rewardType;;
            result.rewards.Add("coins", betReturn);
            return result;
        }

        public ClaimRewardResult BonusCoins(string rewardType, string challengeId, SocialEdgePlayerContext socialEdgePlayer, SocialEdgeChallengeContext socialEdgeChallenge)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            ChallengeData challengeData = socialEdgeChallenge.ChallengeModel.Get(challengeId);
            ChallengePlayerModel playerChallengeData = challengeData.playersData[socialEdgePlayer.PlayerId];

            ChallengeWinnerBonusRewardsData bonusRewards = playerChallengeData.winnerBonusRewards;
        
            if (bonusRewards != null) 
            {
                int bonusCoins = (int)bonusRewards.GetType().GetField(rewardType).GetValue(bonusRewards);
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", bonusCoins);
            }

            return result;
        }

        public ClaimRewardResult BalloonRVRewards(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();

            int defaultBet = socialEdgePlayer.PlayerEconomy.GetDefaultBet();
            List<string> rewardsProbablity = SocialEdge.TitleContext.EconomySettings.balloonRewardsProbability;
            var selectedRewardType = socialEdgePlayer.PlayerModel.Economy.balloonRewardsClaimedCount == 0 ? "A" :
                        rewardsProbablity[ Utils.GetRandomInteger(0, rewardsProbablity.Count - 1) ];
            EconomyBalloonReward selectedRewardSetting = SocialEdge.TitleContext.EconomySettings.balloonRewards[selectedRewardType];
            EconomyBalloonReward selectedReward = new EconomyBalloonReward();
            selectedReward.balloonCoins = selectedRewardSetting.balloonCoins;
            selectedReward.balloonGems = selectedRewardSetting.balloonGems;
            selectedReward.balloonPiggyBankMins = selectedRewardSetting.balloonPiggyBankMins;
            selectedReward.balloonPowerPlayMins = selectedRewardSetting.balloonPowerPlayMins;
            selectedReward.coinsRewardRatio = selectedRewardSetting.coinsRewardRatio;
        
            if (defaultBet > 10000) 
            {
                selectedReward.balloonCoins = (int)(defaultBet * selectedReward.coinsRewardRatio);
            }
        
            selectedReward.coinsRewardRatio = -1;

            if ((socialEdgePlayer.PlayerEconomy.GetRealMoneyAmountSpent() <= 0) || 
                    socialEdgePlayer.MiniProfile.League < int.Parse(Settings.CommonSettings["piggyBankUnlocksAtLeague"].ToString()))   
            {
                selectedReward.balloonPiggyBankMins = -1;
            }
            else if (socialEdgePlayer.Inventory.Where(item => item.ItemId.Contains("PiggyBank")).FirstOrDefault() != null)
            {
                int rand = Utils.GetRandomInteger(0, 10);
                if (rand < 9)
                {
                    selectedReward.balloonPowerPlayMins = -1;
                }
                else
                {
                    selectedReward.balloonPiggyBankMins = -1;
                }
            }
            else 
            {
                int rand = Utils.GetRandomInteger(0, 10);
                if (rand > 5)
                {
                    selectedReward.balloonPowerPlayMins = -1;
                }
                else
                {
                    selectedReward.balloonPiggyBankMins = -1;
                }
            }
        
            socialEdgePlayer.PlayerModel.Economy.balloonRewardsClaimedCount = socialEdgePlayer.PlayerModel.Economy.balloonRewardsClaimedCount + 1;
            socialEdgePlayer.PlayerModel.Economy.balloonReward = new BalloonReward()
            {
                balloonCoins = selectedReward.balloonCoins,
                balloonGems = selectedReward.balloonGems,
                balloonPiggyBankMins = selectedReward.balloonPiggyBankMins,
                balloonPowerPlayMins = selectedReward.balloonPowerPlayMins,
                coinsRewardRatio = selectedReward.coinsRewardRatio
            };

            result.claimRewardType = rewardType;
            result.balloonReward = selectedReward;

            return result;
        }

        public ClaimRewardResult BalloonCoins(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            int coins = socialEdgePlayer.PlayerModel.Economy.balloonReward.balloonCoins;
            socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", coins);
            result.claimRewardType = rewardType;
            
            result.rewards = new Dictionary<string, int> ();
            result.rewards.Add("coins", coins);
            return result;
        }

        public ClaimRewardResult BalloonGems(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            int gems = socialEdgePlayer.PlayerModel.Economy.balloonReward.balloonGems;
            socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("GM", gems);
            result.claimRewardType = rewardType;

            result.rewards = new Dictionary<string, int> ();
            result.rewards.Add("gems", gems);
            return result;
        }

        public ClaimRewardResult BalloonPowerPlayMins(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            int reward = socialEdgePlayer.PlayerModel.Economy.balloonReward.balloonPowerPlayMins * 1000 * 60;
        
            if(socialEdgePlayer.PlayerModel.Economy.freePowerPlayExipryTimestamp > Utils.UTCNow()) 
            {
                socialEdgePlayer.PlayerModel.Economy.freePowerPlayExipryTimestamp = socialEdgePlayer.PlayerModel.Economy.freePowerPlayExipryTimestamp + reward;
            }
            else 
            {
                socialEdgePlayer.PlayerModel.Economy.freePowerPlayExipryTimestamp = Utils.UTCNow() + reward;
            }
        
            result.rewardsLong = new Dictionary<string, long> ();
            result.claimRewardType = rewardType;
            result.rewardsLong.Add("freePowerPlayExipryTimestamp", socialEdgePlayer.PlayerModel.Economy.freePowerPlayExipryTimestamp);
            return result;
        }

        public ClaimRewardResult BalloonPiggyBankMins(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();
            int reward = socialEdgePlayer.PlayerModel.Economy.balloonReward.balloonPiggyBankMins * 1000 * 60;
        
            if (socialEdgePlayer.PlayerModel.Economy.piggyBankDoublerExipryTimestamp > Utils.UTCNow()) 
            {
                socialEdgePlayer.PlayerModel.Economy.piggyBankDoublerExipryTimestamp = socialEdgePlayer.PlayerModel.Economy.piggyBankDoublerExipryTimestamp + reward;
            }
            else 
            {
                socialEdgePlayer.PlayerModel.Economy.piggyBankDoublerExipryTimestamp = Utils.UTCNow() + reward;
            }

            result.rewardsLong = new Dictionary<string, long>();
            result.claimRewardType = rewardType;
            result.rewardsLong.Add("piggyBankDoublerExipryTimestamp", socialEdgePlayer.PlayerModel.Economy.piggyBankDoublerExipryTimestamp);
            return result;
        }

        public ClaimRewardResult DailyEventContinue(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();

            if (socialEdgePlayer.PlayerModel.Events.dailyEventState == "lost") 
            {
                socialEdgePlayer.PlayerModel.Events.dailyEventState = "running";
            }
        
            result.claimRewardType = rewardType;
            result.dailyEventState = socialEdgePlayer.PlayerModel.Events.dailyEventState;
            return result;
        }

        public ClaimRewardResult DailyEventReward(string rewardType, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();

            if (Utils.UTCNow() <= socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp &&
                                        socialEdgePlayer.PlayerModel.Events.dailyEventProgress > 0 &&
                                        socialEdgePlayer.PlayerModel.Events.dailyEventState != "completed") 
            {
                int rewardCoins = 0;
                int rewardGems = 0;
            
                for (int i = 0; i < socialEdgePlayer.PlayerModel.Events.dailyEventProgress; i++) 
                {
                    rewardCoins += socialEdgePlayer.PlayerModel.Events.dailyEventRewards[i].coins;
                    rewardGems += socialEdgePlayer.PlayerModel.Events.dailyEventRewards[i].gems;
                }

                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", rewardCoins);
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("GM", rewardGems);

                socialEdgePlayer.PlayerModel.Events.dailyEventState = "completed";
                socialEdgePlayer.MiniProfile.EventGlow = socialEdgePlayer.PlayerModel.Events.dailyEventRewards.Count == socialEdgePlayer.PlayerModel.Events.dailyEventProgress ? 1 : 0;
            
                result.rewards = new Dictionary<string, int>();
                result.claimRewardType = rewardType;
                result.rewards.Add("coins", rewardCoins);
                result.rewards.Add("gems", rewardGems);
                result.dailyEventState = socialEdgePlayer.PlayerModel.Events.dailyEventState;
                result.dailyEventRing = socialEdgePlayer.MiniProfile.EventGlow;
            }
            else 
            {
                socialEdgePlayer.PlayerEconomy.ProcessDailyEvent();

                result.error = "invalidDailyEventReward";
                result.claimRewardType = rewardType;
                result.playerDataEvent = socialEdgePlayer.PlayerModel.Events;
            }

            return result;
        }

        public ClaimRewardResult CoinPurchaseReward(string rewardType, int rewardPoints, SocialEdgePlayerContext socialEdgePlayer)
        {
            ClaimRewardResult result = new ClaimRewardResult();

            int currentCoins = socialEdgePlayer.VirtualCurrency["CN"];
            int minCoins = SocialEdge.TitleContext.EconomySettings.BettingIncrements[0];
            
            if (currentCoins < minCoins) 
            {
                result.rewards = new Dictionary<string, int>();
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", rewardPoints);
                result.claimRewardType = rewardType;
                result.rewards.Add("coins", rewardPoints);
            }
            else 
            {
                result.error = "invalidCoinPurchaseReward";
                result.coins = currentCoins;
            }

            return result;
        }
    }
}
