/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using PlayFab.ServerModels;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialEdgeSDK.Server.Context
{
    // LEAGUE SETTINGS
    public class LeagueQualificationRewardData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int gems;
    }

    public class LeagueQualificationData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int trophies;
                                                                public LeagueQualificationRewardData reward;
    }

    public class LeagueTrophiesData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int win;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int loss;
    }

    public class LeagueDailyRewardData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int coins;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int gems;
    }

    public class LeagueSettingsData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string name;
                                                                public LeagueQualificationData qualification;
                                                                public LeagueTrophiesData trophies;
                                                                public LeagueDailyRewardData dailyReward;
    }

    public class LeagueSettingModel
    {
        public Dictionary<string, LeagueSettingsData> leagues;
    }

    // ECONOMY SETTINGS
    public class EconomyPlayerDefaults
    {
        public int CURRENCY2;
    }

    public class EconomyOwnedItem
    {
        public string shopItemKey;
        public int quantity;
    }

    public class EconomyMinMax
    {
        public int min;
        public int max;
    }

    public class EconomyRewards
    {
        public double matchWinReward;
        public int matchWinAdReward;
        public double matchRunnerUpReward;
        public int matchRunnerUpAdReward;
        public double rewardMatchPromotional;
        public int failSafe;
        public int facebookConnectReward;
        public int ratingBoostTier1Reward;
        public EconomyMinMax chestCoinsReward;
        public EconomyMinMax chestGemsReward;
        public int coinPurchaseReward;
        public int dailyReward;
        public int personalisedAdsGemReward;
        public int powerPlayReward;
        public int rvBoosterReward;
        public int freeFullGameAnalysis;
        public int rvAnalysisReward;
        public int betCoinsReturn;
        public LeagueDailyRewardData onboardingReward;
    }

    public class EconomyAds
    {
        public int adsGlobalCap;
        public int adsInterstitialCap;
        public int adsRewardedVideoCap;
        public int resignCap;
        public int ADS_SLOT_HOUR;
        public int ADS_FREE_NO_ADS_PERIOD;
        public int minutesForVictoryInteralAd;
        public int autoSubscriptionDlgThreshold;
        public int daysPerAutoSubscriptionDlgThreshold;
        public int sessionsBeforePregameAd;
        public int maxPregameAdsPerDay;
        public int intervalsBetweenPregameAds;
        public int waitForPregameAdLoadSeconds;
        public bool showPregameOneMinute;
        public bool showPregameTournament;
        public bool showInGameCPU;
        public bool showInGame30Min;
        public bool showInGameClassic;
        public int minutesBetweenIngameAds;
        public int minutesLeftDisableTournamentPregame;
        public int minutesElapsedDisable30MinInGame;
        public int chestCooldownTimeInMin;
        public bool enableBannerAds;
        public int minPlayDaysRequired;
        public int minGemsRequiredforRV;
        public int premiumTimerCooldownTimeInMin;
        public int freemiumTimerCooldownTimeInMin;
        public bool removeInterAdsOnPurchase;
        public bool turnOnAdinmoAds;
        public bool removeRVOnPurchase;
        public int waitForAdAvailibility;
        public List<string> adPlacements;
    }

    public class EconomyInventoryRewardedVideoCost
    {
        public int SpecialItemGemsBooster;
        public int SpecialItemHint;
        public int SpecialItemRatingBooster;
        public int SpecialItemKey;
        public int SpecialItemTicket;    
    }

    public class EconomyFreeHintThresholds
    {
        public int advantage;
        public int hintsPurchased;
    }

    public class EconomyShopRVRewards
    {
        public int min;
        public int max;
        public int cooldownInMins;
    }

    public class EcomonyDailyEventReward
    {
        public int gems;
        public double coinsRatio;
    }

    public class EconomyBalloonReward
    {
        public int balloonCoins;
        public int balloonGems;
        public int balloonPowerPlayMins;
        public int balloonPiggyBankMins;
        public double coinsRewardRatio;
    }

    public class EconomySettingsModel
    {
        public string CatalogId;
        public string StoreId;
        public EconomyPlayerDefaults PlayerDefaults;
        public List<EconomyOwnedItem> PlayerDefaultOwnedItems;
        public EconomyRewards Rewards;
        public EconomyAds Ads;
        public EconomyInventoryRewardedVideoCost InventoryRewardedVideoCost;
        public EconomyFreeHintThresholds FreeHintThresholds;
        public List<int> BettingIncrements;
        public List<double> DefaultBetIncrementByGamesPlayed;
        public EconomyShopRVRewards ShopRVRewards;
        public List<EcomonyDailyEventReward> DailyEventRewards;
        public Dictionary<string, EconomyBalloonReward> balloonRewards;
        public List<string> balloonRewardsProbability;
    }
}