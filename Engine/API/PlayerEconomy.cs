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
            Int32 piggyBankBalance = socialEdgePlayer.PlayerModel.Economy.piggyBankGems;
            long piggyBankExpiryTimestamp = socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp;
            Int32 playerLeague = socialEdgePlayer.PlayerModel.Info.league;

            dynamic commonSettings = Settings.CommonSettings;

            if (piggyBankExpiryTimestamp != 0 && currentTime >= piggyBankExpiryTimestamp)
            {
                socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = 0;
                socialEdgePlayer.PlayerModel.Economy.piggyBankGems = 0;
                piggyBankBalance = 0;
            }

            if (playerLeague >= commonSettings["piggyBankUnlocksAtLeague"] && piggyBankBalance < commonSettings["piggyBankMaxCap"])
            {
                Int32 piggyBankRewardPerGameSetting = (Int32)commonSettings["piggyBankRewardPerGame"];
                Int32 piggyBankRewardPerGame = piggyBankExpiryTimestamp > currentTime ? piggyBankRewardPerGameSetting * 2 : piggyBankRewardPerGameSetting;

                Int32 piggyBankMaxCap = (Int32)commonSettings["piggyBankMaxCap"];
                Int32 piggyLimitAvailable = piggyBankMaxCap - piggyBankBalance;
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

        public dynamic GetDynamicGemSpotBundle()
        {
            SetupDynamicBundleTier();
            dynamic dynamicBundlePurchaseTier = Settings.DynamicGemSpotBundles[socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier];
            // Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(dynamicBundlePurchaseTier.ToString());
            // dictionary["pack1"] = Utils.GetShortCode(dictionary["pack1"].ToString());;
            // dictionary["pack2"] = Utils.GetShortCode(dictionary["pack2"].ToString());;
            // dictionary["bundle"] = Utils.GetShortCode(dictionary["bundle"].ToString());;
            return dynamicBundlePurchaseTier;
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

                socialEdgePlayer.MiniProfile.EventGlow = 0;
                socialEdgePlayer.PlayerModel.Events.dailyEventProgress = 0;
                socialEdgePlayer.PlayerModel.Events.dailyEventState = "running";
                socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp = expiryTimestamp;
                socialEdgePlayer.PlayerModel.Events.dailyEventRewards = CalculateDailyEventRewards(defaultBet);
            }
        }


        private int GetDefaultBet()
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
            long creationTime = Utils.ToUTC(socialEdgePlayer.CombinedInfo.AccountInfo.Created);
            long currentTime = Utils.UTCNow();
            double retentionDays = ((currentTime - creationTime) / (60*60*24*1000));
            retentionDays = Math.Floor(retentionDays);
            return (int)retentionDays;
        }

        public bool IsValidForRVReward()
        {
            int retentionDay = GetPlayerRetentionDays();
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
                bool added = Grant(item.Key, (int)item.Value);
                if (added)
                {
                    rewarded.Add(item.Key, (int)item.Value);
                }
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
                if (catalogItem != null && catalogItem.VirtualCurrencyPrices["GM"] > 0)
                {
                    double dollars = catalogItem.RealCurrencyPrices["RM"] / 100;
                    int iap_count = theGood.RemainingUses != null ? (int)theGood.RemainingUses : 0;
                    dCost += iap_count * dollars;
                }
            }

            return dCost;
        }

        public long ProcessDailyEventExipryTimestamp()
        {
            long currentTime = Utils.UTCNow();
            long hourToMilliseconds = 60 * 60 * 1000;
        
            if (socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp == 0 || 
                socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp  <= currentTime) 
            {
                long expiryTimestamp = Utils.ToUTC(Utils.EndOfDay(new DateTime())) - (socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot * hourToMilliseconds);
                int defaultBet = GetDefaultBet();

                if (expiryTimestamp < currentTime)
                {
                    expiryTimestamp = expiryTimestamp + 24 * hourToMilliseconds;
                }
                
                socialEdgePlayer.MiniProfile.EventGlow = 0;
                socialEdgePlayer.PlayerModel.Events.dailyEventProgress = 0;
                socialEdgePlayer.PlayerModel.Events.dailyEventRewards = CalculateDailyEventRewards(defaultBet);
                socialEdgePlayer.PlayerModel.Events.dailyEventState = "running";
                socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp = expiryTimestamp;
            }
        
            return socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp;
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
    }
}