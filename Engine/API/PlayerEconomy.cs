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

        private void ProcessSaleBundles()
        {
            SetupDynamicBundleTier();
            var economyData = socialEdgePlayer.PlayerModel.Economy;
            var dayInMs = 24 * 60 * 60 * 1000;
            var currentTime = Utils.UTCNow();

            if(economyData.bundleSaleEndTimestamp <= currentTime)
            {
                economyData.activeBundleSales.Clear();
            }

            if((economyData.lastPurchasedGems == 0 
                || socialEdgePlayer.VirtualCurrency["GM"] <= economyData.lastPurchasedGems * 0.3)
                && economyData.nextBundleSaleStartTimestamp <= currentTime)
            {
                var availableSales = Settings.DynamicSaleBundles[economyData.dynamicBundlePurchaseTier];
                economyData.activeBundleSales.Add(availableSales[0].ToString());
                economyData.activeBundleSales.Add(availableSales[1].ToString());
                economyData.bundleSaleEndTimestamp = currentTime + (3 * dayInMs);
                economyData.nextBundleSaleStartTimestamp = currentTime + (6 * dayInMs);

                if(availableSales.Count > 2)
                {
                    economyData.activeBundleSales.Add(availableSales[Utils.GetRandomInteger(2, availableSales.Count)].ToString());
                }
            }
        }

        private void ProcessRemoveAdsSale()
        {
            var economyData = socialEdgePlayer.PlayerModel.Economy;
            var dayInMs = 24 * 60 * 60 * 1000;
            var currentTime = Utils.UTCNow();
            var daysSinceCreation = (DateTime.UtcNow - socialEdgePlayer.PlayerModel.Info.created).TotalDays;

            if(daysSinceCreation > 7 && economyData.nextRemoveAdsSaleStartTimestamp <= currentTime)
            {
                economyData.removeAdsSaleEndTimestamp = currentTime + (2 * dayInMs);
                economyData.nextRemoveAdsSaleStartTimestamp = currentTime + (5 * dayInMs);
            }
        }

        private void ProcessSelectedEmoji()
        {
            if(socialEdgePlayer.PlayerModel.Info.selectedEmojiExpiryTimestamp <= Utils.UTCNow())
            {
                socialEdgePlayer.PlayerModel.Info.selectedEmojiId = 0;
            }
        }

        public Dictionary<string, string> GetDynamicGemSpotBundle()
        {
            SetupDynamicBundleTier();
            dynamic gemSpotBundleBson = Settings.DynamicGemSpotBundles[socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier];
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(gemSpotBundleBson.ToString());
        }

        public bool HasItemIdInInventory(string itemId, bool checkRemainingUses = true)
        {
            ItemInstance object1 = socialEdgePlayer.Inventory.FirstOrDefault(i => i.ItemId == itemId);
            return object1 != null && (!checkRemainingUses || object1.RemainingUses > 0);
        }

        public ItemInstance GetInventoryItem(SocialEdgePlayerContext socialEdgePlayer, string intanceId)
        {
            return socialEdgePlayer.Inventory.FirstOrDefault(i => i.ItemInstanceId == intanceId);
        }

        public ItemInstance GetInventoryItemWithItemID(SocialEdgePlayerContext socialEdgePlayer, string itemId)
        {
            ItemInstance obj = socialEdgePlayer.Inventory.FirstOrDefault(i => i.ItemId == itemId);
            return obj != null ? obj : null;
        }
        public void AddVirtualCurrency(string currencyType, int amount)
        {
            var taskT = Player.AddVirtualCurrency(socialEdgePlayer.PlayerId, amount, currencyType);
        }
        
        public void SubtractVirtualCurrency(string currencyType, int amount)
        {
            var taskT = Player.SubtractVirtualCurrency(socialEdgePlayer.PlayerId, amount, currencyType);
        }

        private bool HasAllThemesExceptSkinWood => socialEdgePlayer.Inventory.Count(s => s.ItemClass.Equals("skinShopItems")) == SocialEdge.TitleContext.CatalogItems.Catalog.Count(s => s.ItemClass.Equals("skinShopItems")) - 1 && !HasItemIdInInventory("SkinWood");

        private bool HasAllThemes => SocialEdge.TitleContext.CatalogItems.Catalog.Count(s => s.ItemClass.Equals("skinShopItems")) == socialEdgePlayer.Inventory.Count(s => s.ItemClass.Equals("skinShopItems"));

        private bool OwnsAllThemes => IsSubscriber || HasItemIdInInventory("com.turbolabz.instantchess.allthemespack", false) || HasAllThemes || HasAllThemesExceptSkinWood;

        private bool IsSubscriber => socialEdgePlayer.PlayerModel.Economy.subscriptionExpiryTime > Utils.UTCNow();

        public void ProcessDailyEvent()
        {
            var currentTime = Utils.UTCNow();

            if(socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp == 0 || socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp <= currentTime)
            {
                var hourToMilliseconds = 60 * 60 * 1000;
                var expiryTimestamp = Utils.ToUTC(DateTime.UtcNow.Date.AddDays(1)) - (socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot * hourToMilliseconds);
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

        public int GetMaxBet()
        {
            var bettingIncrementSettings = SocialEdge.TitleContext.EconomySettings.BettingIncrements; 
            var totalCoins = (int)socialEdgePlayer.VirtualCurrency["CN"];
            var betIndex = 0;

            for(int i = 0; i < bettingIncrementSettings.Count; i++)
            {
                if(totalCoins <= bettingIncrementSettings[i])
                {
                    betIndex = i > 0 ? i - 1 : i;
                    break;
                }

                betIndex = i;
            }

            return bettingIncrementSettings[betIndex];
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
            return (int)(DateTime.UtcNow - socialEdgePlayer.PlayerModel.Info.created).TotalDays;
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

        public void ProcessFreeSpinTimestamp()
        {
            var currentTime = Utils.UTCNow();
            var hourToMilliseconds = 60 * 60 * 1000;
            var availableTimestamp = Utils.ToUTC(DateTime.UtcNow.Date.AddDays(1)) - (socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot * hourToMilliseconds);
            
            if(availableTimestamp < currentTime)
            {
                availableTimestamp = availableTimestamp + 24 * hourToMilliseconds;
            }
            
            socialEdgePlayer.PlayerModel.Economy.freeSpinTimestamp = availableTimestamp;
        }

        public void ProcessSpinWheelRewards()
        {
            var maxBet = GetMaxBet();
            var ownAllThemes = OwnsAllThemes;
            ProcessSpinWheelRewards(SocialEdge.TitleContext.EconomySettings.spinWheel.freeRewards, socialEdgePlayer.PlayerModel.Economy.freeSpinRewards, maxBet, ownAllThemes, socialEdgePlayer.PlayerModel.Economy.freeSpinCounter, true);
            ProcessSpinWheelRewards(SocialEdge.TitleContext.EconomySettings.spinWheel.fortuneRewards, socialEdgePlayer.PlayerModel.Economy.fortuneSpinRewards, maxBet, ownAllThemes, socialEdgePlayer.PlayerModel.Economy.fortuneSpinCounter, false);
        }

        private void ProcessSpinWheelRewards(List<SpinWheelReward> serverSettings, List<SpinWheelReward> playerData, int maxBet, bool ownAllThemes, int counter, bool isFree)
        {
            var extraRewardIndex = 0;

            if(playerData == null)
            {
                playerData = new List<SpinWheelReward>();
            }
            else
            {
                playerData.Clear();
            }

            for(int i = 0; i < serverSettings.Count; i++)
            {
                var reward = serverSettings[i];

                if(reward.type.Equals("theme") && ownAllThemes)
                {
                    reward = SocialEdge.TitleContext.EconomySettings.spinWheel.extraRewards[extraRewardIndex];
                    extraRewardIndex++;
                }

                var newReward = new SpinWheelReward();
                newReward.type = reward.type;
                newReward.color = reward.color;
                newReward.label = reward.label;
                newReward.value = reward.type.Equals("coins") ? maxBet * reward.value : reward.value;

                if(counter == 0)
                {
                    if(isFree)
                    {
                        newReward.probability = i == 2 ? 100 : 0;
                    }
                    else 
                    {
                        newReward.probability = i == 4 ? 100 : 0;
                    }
                }
                else if(counter == 1 && !isFree)
                {
                    if(socialEdgePlayer.MiniProfile.League < int.Parse(Settings.CommonSettings["piggyBankUnlocksAtLeague"].ToString()) 
                        && socialEdgePlayer.Inventory.Where(item => item.ItemId.Contains("PiggyBank")).FirstOrDefault() != null)
                    {
                        newReward.probability = i == 7 ? 100 : 0;
                    }
                    else if(socialEdgePlayer.PlayerModel.Economy.freePowerPlayExipryTimestamp > 0)
                    {
                        newReward.probability = i == 2 ? 100 : 0;
                    }
                    else
                    {
                        newReward.probability = i == 6 ? 100 : 0;
                    }
                }
                else
                {
                    newReward.probability = reward.probability;
                }

                playerData.Add(newReward);
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
            ProcessReceivedSocialStars();
            ProcessSpinWheelRewards();
            ProcessSaleBundles();
            ProcessRemoveAdsSale();
            ProcessSelectedEmoji();
        }

        public void ProcessWinnerBonusRewards(ChallengePlayerModel challengePlayerModel)
        {
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
        
            ChallengeWinnerBonusRewardsData rewards = challengePlayerModel.CreateChallengeWinnerBonusReward();
            rewards.bonusCoinsFree1 = (int)Math.Round(freeReward * reward1Ratio);
            rewards.bonusCoinsFree2 = (int)Math.Round(freeReward * reward2Ratio);
            rewards.bonusCoinsFree3 = (int)Math.Round(freeReward * reward3Ratio);
            rewards.bonusCoinsRV1 = (int)Math.Round(rvReward * reward1Ratio);
            rewards.bonusCoinsRV2 = (int)Math.Round(rvReward * reward2Ratio);
            rewards.bonusCoinsRV3 = (int)Math.Round(rvReward * reward3Ratio);
            challengePlayerModel.winnerBonusRewards = rewards;

            if (isJackpot)
                socialEdgePlayer.PlayerModel.Economy.jackpotNotCollectedCounter = 0;
        }

        public void ProcessReceivedSocialStars()
        {
            var currentTime = Utils.UTCNow();

            if(socialEdgePlayer.PlayerModel.Info.dailyStarsExipryTimestamp == 0 || socialEdgePlayer.PlayerModel.Info.dailyStarsExipryTimestamp <= currentTime)
            {
                var hourToMilliseconds = 60 * 60 * 1000;
                var expiryTimestamp = Utils.ToUTC(DateTime.UtcNow.Date.AddDays(1)) - (socialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot * hourToMilliseconds);

                if(expiryTimestamp < currentTime)
                {
                    expiryTimestamp = expiryTimestamp + 24 * hourToMilliseconds;
                }
                    
                socialEdgePlayer.PlayerModel.Info.dailyStarsReceived = 0;
                socialEdgePlayer.PlayerModel.Info.dailyStarsExipryTimestamp = expiryTimestamp;
            }
        }

        public void IncreamentReceivedSocialStars()
        {
            ProcessReceivedSocialStars();
            socialEdgePlayer.PlayerModel.Info.dailyStarsReceived = socialEdgePlayer.PlayerModel.Info.dailyStarsReceived + 1;
            socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceived = socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceived + 1;
            int lifeTimeStarsReceivedLevel = 0;

            if(socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceived > 0 && socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceived < 10)
            {
                lifeTimeStarsReceivedLevel = 1;
            }
            else if(socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceived >= 10 && socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceived < 30)
            {
                lifeTimeStarsReceivedLevel = 2;
            }
            else
            {
                lifeTimeStarsReceivedLevel = ((socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceived - 30) / 50) + 3;
            }

            if(lifeTimeStarsReceivedLevel > socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceivedLevel)
            {
                socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceivedLevel = lifeTimeStarsReceivedLevel;

                if(lifeTimeStarsReceivedLevel > 1)
                {
                    var message = Inbox.CreateMessage();
                    message.type = "RewardSocialStarsLevelPromotion";
                    message.reward = new Dictionary<string, int>() { ["gems"] = 10};
                    InboxModel.Add(message, socialEdgePlayer);
                }
            }
        }
    }
}