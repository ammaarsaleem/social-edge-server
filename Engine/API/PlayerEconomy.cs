/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PlayFab.ProfilesModels;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace SocialEdgeSDK.Server.Context
{
    public class PlayerEconomy
    {
        [BsonIgnore] private SocialEdgePlayerContext socialEdgePlayer;

        public PlayerEconomy(SocialEdgePlayerContext _socialEdgePlayer)
        {
            socialEdgePlayer = _socialEdgePlayer;
        }

        public void ProcessPiggyBankExpiry()
        {
            if(socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp != 0 && Utils.UTCNow() >= socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp)
            {
                socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = 0;
                socialEdgePlayer.PlayerModel.Economy.piggyBankGems = 0;
            }
        }

        public int ProcessPiggyBankReward(ChallengePlayerModel matchData)
        {
            int piggyBankReward = 0;

            if (string.IsNullOrEmpty(matchData.tournamentId) && !matchData.isEventMatch) {
                return piggyBankReward;
            }

            string playerId = socialEdgePlayer.PlayerId;
            long currentTime = Utils.UTCNow();
            int piggyBankBalance = socialEdgePlayer.PlayerModel.Economy.piggyBankGems;
            long piggyBankExpiryTimestamp = socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp;
            int playerLeague = socialEdgePlayer.MiniProfile.League;

            dynamic commonSettings = Settings.CommonSettings;

            if (piggyBankExpiryTimestamp != 0 && currentTime >= piggyBankExpiryTimestamp)
            {
                socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = 0;
                socialEdgePlayer.PlayerModel.Economy.piggyBankGems = 0;
                piggyBankBalance = 0;
            }

            if (playerLeague >= commonSettings["piggyBankUnlocksAtLeague"] && piggyBankBalance < commonSettings["piggyBankMaxCap"])
            {
                int piggyBankRewardPerGameSetting = (int)commonSettings["piggyBankRewardPerGame"];
                int piggyBankRewardPerGame = socialEdgePlayer.PlayerModel.Economy.piggyBankDoublerExipryTimestamp > currentTime ? piggyBankRewardPerGameSetting * 2 : piggyBankRewardPerGameSetting;

                int piggyBankMaxCap = (int)commonSettings["piggyBankMaxCap"];
                int piggyLimitAvailable = piggyBankMaxCap - piggyBankBalance;
                piggyBankReward = piggyLimitAvailable >= piggyBankRewardPerGame ? piggyBankRewardPerGame : piggyLimitAvailable;
                socialEdgePlayer.PlayerModel.Economy.piggyBankGems = socialEdgePlayer.PlayerModel.Economy.piggyBankGems + piggyBankReward;

                if (socialEdgePlayer.PlayerModel.Economy.piggyBankGems >= piggyBankMaxCap)
                {
                    long piggyBankExpiry = (int)commonSettings["piggyBankExpirationInDays"] * 24 * 60 * 60 * 1000;
                    socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = currentTime + piggyBankExpiry;
                }
            }

            return piggyBankReward;
        }
        public string ProcessDynamicDisplayBundle()
        {
            SetupDynamicBundleTier();

            if (string.IsNullOrEmpty(socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier))
            {
                socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier = "A";
            }

            if (socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount > 0)
            {
                socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount = socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount + 1;

                if (socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount > (int)Settings.CommonSettings["dynamicBundleSwitchAfterSessions"])
                {
                    socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier = "B";
                }
            }

            string tempItemId = Settings.DynamicDisplayBundles[socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier][socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier].ToString();
            string shortCode = Utils.GetShortCode(tempItemId);
            return shortCode;
        }

        private void SetupDynamicBundleTier()
        {
            if (string.IsNullOrEmpty(socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier))
            {
                socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier = "T1";

                foreach (var dataItem in Settings.DynamicPurchaseTiers)
                {
                    string tierValue = dataItem.Value.ToString();
                    string dynamicBundlePurchaseTier = socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier;
                    string itemId = Utils.AppendItemId(dataItem.Name.ToString());
                    if (HasItemIdInInventory(itemId) && Utils.compareTier(tierValue, dynamicBundlePurchaseTier) == 1)
                    {
                        socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier = tierValue;
                    }
                }
            }
        }

        public Dictionary<string, string> GetDynamicGemSpotBundle()
        {
            SetupDynamicBundleTier();
            dynamic gemSpotBundleBson = Settings.DynamicGemSpotBundles[socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier];
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(gemSpotBundleBson.ToString());
        }

        public bool HasItemIdInInventory(string itemId)
        {
            ItemInstance object1 = socialEdgePlayer.Inventory.FirstOrDefault(i => i.ItemId == itemId);
            return object1 != null && object1.RemainingUses > 0;
        }

        public ItemInstance GetInventoryItem(SocialEdgePlayerContext socialEdgePlayer, string intanceId)
        {
            return socialEdgePlayer.Inventory.FirstOrDefault(i => i.ItemInstanceId == intanceId);
        }

        public void AddVirtualCurrency(string currencyType, int amount)
        {
            var taskT = Player.AddVirtualCurrency(socialEdgePlayer.PlayerId, amount, currencyType);
        }
        
        public void SubtractVirtualCurrency(string currencyType, int amount)
        {
            var taskT = Player.SubtractVirtualCurrency(socialEdgePlayer.PlayerId, amount, currencyType);
        }

        public void ProcessDailyEvent()
        {
            var currentTime = Utils.UTCNow();

            if(socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp == 0 || socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp <= currentTime)
            {
                var hourToMilliseconds = 60 * 60 * 1000;
                var expiryTimestamp = Utils.ToUTC(Utils.EndOfDay(DateTime.Now)) - (socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot * hourToMilliseconds);
                var defaultBet = GetDefaultBet();

                if(expiryTimestamp < currentTime)
                {
                    expiryTimestamp = expiryTimestamp + 24 * hourToMilliseconds;
                }

                if (socialEdgePlayer.MiniProfile.EventGlow != 0)
                    socialEdgePlayer.MiniProfile.EventGlow = 0;
                    
                socialEdgePlayer.PlayerModel.Events.dailyEventProgress = 0;
                socialEdgePlayer.PlayerModel.Events.dailyEventState = "running";
                socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp = expiryTimestamp;
                socialEdgePlayer.PlayerModel.Events.dailyEventRewards = CalculateDailyEventRewards(defaultBet);
            }
        }


        public int GetDefaultBet()
        {
            var defaultBetIncrementSettings = SocialEdge.TitleContext.EconomySettings.DefaultBetIncrementByGamesPlayed; 
            var bettingIncrementSettings = SocialEdge.TitleContext.EconomySettings.BettingIncrements; 
            var lastIndex = defaultBetIncrementSettings.Count - 1;
            var gamesPlayedIndex = GetGamesPlayedToday();
            gamesPlayedIndex = gamesPlayedIndex >= lastIndex ? lastIndex : gamesPlayedIndex;
            var coinsToBet = (int)(socialEdgePlayer.VirtualCurrency["CN"] * defaultBetIncrementSettings[gamesPlayedIndex]);
            var betIndex = GetBetIndex(coinsToBet, bettingIncrementSettings);
            return bettingIncrementSettings[betIndex];
        }

        private int GetBetIndex(int coinsToBet, List<int> bettingIncrementSettings)
        {
            var betIndex = 0;

            for(int i = 0; i < bettingIncrementSettings.Count; i++)
            {
                if(coinsToBet <= bettingIncrementSettings[i])
                {
                    if(i == 0)
                    {
                        betIndex = i;
                        break;
                    }

                    var diff1 = Math.Abs(coinsToBet - bettingIncrementSettings[i - 1]);
                    var diff2 = Math.Abs(coinsToBet - bettingIncrementSettings[i]);
                    betIndex = diff1 < diff2 ? i - 1 : i;
                    break;
                }

                betIndex = i;
            }

            return betIndex;
        }

        private int GetGamesPlayedToday()
        {
            var dateKey = DateTime.Now.ToShortDateString();
            var todayGamesData = socialEdgePlayer.PlayerModel.Info.gamesPlayedPerDay.Where(g => g.Key.Equals(dateKey)).Select(g => g.Value).FirstOrDefault();
            return todayGamesData != null ? todayGamesData.won + todayGamesData.lost + todayGamesData.drawn : 0;
        }

        public int ProcessShopRvMaxReward()
        {
            EconomySettingsModel economySettings = SocialEdge.TitleContext.EconomySettings;

            if (socialEdgePlayer.PlayerModel.Economy.shopRvMaxReward == 0) 
            {
                string selectedRewardType =  "A" ;
                var selectedReward = economySettings.balloonRewards[selectedRewardType];
                int defaultBet = GetDefaultBet();
                socialEdgePlayer.PlayerModel.Economy.shopRvDefaultBet = defaultBet;
                int rewardCoins = selectedReward.balloonCoins;
                if (defaultBet > rewardCoins) 
                {
                    rewardCoins = (int)(defaultBet * selectedReward.coinsRewardRatio);
                }
                socialEdgePlayer.PlayerModel.Economy.shopRvMaxReward = rewardCoins;
            }
            return socialEdgePlayer.PlayerModel.Economy.shopRvMaxReward;
        }

        public int GetPlayerRetentionDays()
        {
            return (int)(DateTime.UtcNow - socialEdgePlayer.CombinedInfo.AccountInfo.Created).TotalDays;
        }

        public bool IsValidForRVReward()
        {
            int retentionDay = socialEdgePlayer.PlayerModel.Info.playDays;
            int currentGems = socialEdgePlayer.VirtualCurrency["GM"];
            int minRequiredGems = SocialEdge.TitleContext.EconomySettings.Ads.minGemsRequiredforRV;
            int minPlayDays = SocialEdge.TitleContext.EconomySettings.Ads.minPlayDaysRequired;
            return retentionDay >= minPlayDays && currentGems <= minRequiredGems;
        }

        public int GetNumVGoods(string itemId)
        {
            ItemInstance vGood = socialEdgePlayer.Inventory.Where(item => item.ItemId == itemId).FirstOrDefault();
            return vGood != null ? (vGood.RemainingUses != null ? (int)vGood.RemainingUses : 0) : 0;
        }

        public bool Consume(string itemId, int qty)
        {
            var taskT = Player.ConsumeItem(socialEdgePlayer.PlayerId, itemId);
            taskT.Wait();
            return taskT.Result.Result.ItemInstanceId != null; 
        }

        public bool Grant(string itemId, int qty)
        {
            var taskT = Player.GrantItem(socialEdgePlayer.PlayerId, itemId);
            taskT.Wait();
            return taskT.Result.Result.ItemGrantResults.Count != 0;
        }

        public Dictionary<string, int> Grant(Dictionary<string, int> rewards)
        {
            Dictionary<string, int> rewarded = new Dictionary<string, int>();
        
            foreach(var item in rewards)
            {
                if(item.Key.Equals("coins"))
                {
                    AddVirtualCurrency("CN", item.Value);
                }
                else if(item.Key.Equals("gems"))
                {
                    AddVirtualCurrency("GM", item.Value);
                }
                else
                {
                    Grant(item.Key, item.Value);
                }
                
                rewarded.Add(item.Key, item.Value);
            }
        
            return rewarded;
        }

        public double GetRealMoneyAmountSpent()
        {
            List<ItemInstance> vGoods = socialEdgePlayer.Inventory;

            if (vGoods.Count == 0)
                return 0;
        
            double dCost = 0;
            foreach (ItemInstance theGood in vGoods) 
            {
                CatalogItem catalogItem = SocialEdge.TitleContext.GetCatalogItem(theGood.ItemId);
                if (catalogItem != null 
                    && catalogItem.RealCurrencyPrices != null 
                    && catalogItem.RealCurrencyPrices.ContainsKey("RM") 
                    && catalogItem.RealCurrencyPrices["RM"] > 0)
                {
                    double dollars = catalogItem.RealCurrencyPrices["RM"] / 100;
                    int iap_count = theGood.RemainingUses != null ? theGood.RemainingUses.Value : 1;
                    dCost += iap_count * dollars;
                }
            }

            return dCost;
        }

        public List<DailyEventRewards> CalculateDailyEventRewards(int betValue) 
        {
            List<DailyEventRewards> rewards = new List<DailyEventRewards>();
            for (int i = 0; i < SocialEdge.TitleContext.EconomySettings.DailyEventRewards.Count; i++) 
            {
                rewards.Add(new DailyEventRewards() 
                { 
                    gems = SocialEdge.TitleContext.EconomySettings.DailyEventRewards[i].gems,
                    coins = (int)(SocialEdge.TitleContext.EconomySettings.DailyEventRewards[i].coinsRatio * betValue)
                });
            }
        
            return rewards;
        }

        public void ProcessRvUnlockTimeStamp()
        {
            if (socialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp > 0)
                return;

            int playDays = socialEdgePlayer.PlayerModel.Info.playDays;
            int minPlayDays = SocialEdge.TitleContext.EconomySettings.Ads.minPlayDaysRequired;
        
            if (playDays >= minPlayDays)
            {
                socialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp = Utils.UTCNow();
            }
        }

        public void ProcessLobbyChestTimestamp()
        {
            if (socialEdgePlayer.PlayerModel.Economy.chestUnlockTimestamp <= 0)
            {
                long chestCooldownTimeSec = SocialEdge.TitleContext.EconomySettings.Ads.chestCooldownTimeInMin * 60 * 1000;
                socialEdgePlayer.PlayerModel.Economy.chestUnlockTimestamp = Utils.UTCNow() + chestCooldownTimeSec;
            }
        }

        private void ProcessPlayDays()
        {
            if((DateTime.UtcNow - socialEdgePlayer.PlayerModel.Info.lastPlayDay).TotalDays > 1)
            {
                socialEdgePlayer.PlayerModel.Info.playDays = socialEdgePlayer.PlayerModel.Info.playDays + 1;
                socialEdgePlayer.PlayerModel.Info.lastPlayDay = DateTime.UtcNow;
            }
        }

        public void ProcessEconomyInit()
        {
            ProcessPlayDays();
            ProcessLobbyChestTimestamp();
            ProcessDailyEvent();
            ProcessPiggyBankExpiry();
            ProcessShopRvMaxReward();
            ProcessRvUnlockTimeStamp();
        }
    }
}