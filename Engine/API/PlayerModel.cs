/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using SocialEdgeSDK.Server.Context;
using Newtonsoft.Json;

namespace SocialEdgeSDK.Server.Models
{
    public static class PlayerModelFields
    {
        public static string META = typeof(PlayerDataModel).Name + ".meta";
        public static string INFO = typeof(PlayerDataModel).Name + ".info";
        public static string ECONOMY = typeof(PlayerDataModel).Name + ".economy";
        public static string EVENTS = typeof(PlayerDataModel).Name + ".events";
        public static string TOURNAMENT = typeof(PlayerDataModel).Name + ".tournament";
        public static string CHALLENGE = typeof(PlayerDataModel).Name + ".challenge";
        public static string FRIENDS = typeof(PlayerDataModel).Name + ".friends";
        public static string BLOCKED = typeof(PlayerDataModel).Name + ".blocked";

        public static string[] ALL = new string[]
        {
            META, INFO, ECONOMY, EVENTS, TOURNAMENT, CHALLENGE, FRIENDS, BLOCKED
        };
    }

    public class GSPlayerModelDocument
    {
        #pragma warning disable format                                                        
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]    public string _id;
        [BsonElement("PlayerDataModel")]                        public BsonDocument document;
        #pragma warning restore format
    }
    public class PublicProfileEx
    {
        public bool isOnline;
        public DateTime created;
        public int eloScore;
        public int trophies;
        public int earnings;
        public int gamesWon;
        public int gamesLost;
        public int gamesDrawn; 

        public PublicProfileEx(bool isOnline, DateTime created, int eloScore, int trophies, int earnings, int gamesWon, int gamesLost, int gamesDrawn)
        {
            this.isOnline = isOnline;
            this.created = created;
            this.eloScore = eloScore;
            this.trophies = trophies;
            this.earnings = earnings;
            this.gamesWon = gamesWon;
            this.gamesLost = gamesLost;
            this.gamesDrawn = gamesDrawn; 
        }
    }

    public class AdRewardsData
    {

    }

    public class DailyEventRewards
    {
        #pragma warning disable format
        [BsonElement("gems")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]              public int gems;
        [BsonElement("coins")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]             public int coins;
        #pragma warning restore format
    }

    public class GameResults
    {
        #pragma warning disable format
        [BsonElement("won")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]              public int won;
        [BsonElement("lost")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]             public int lost;
        [BsonElement("drawn")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]            public int drawn;
        #pragma warning restore format
    }

    public class FriendData
    {
        #pragma warning disable format
        [JsonIgnore][BsonIgnore]                                                            public DataModelBase _parent;
        [BsonElement("friendType")][BsonRepresentation(MongoDB.Bson.BsonType.String)]       public string _friendType;
        [BsonElement("lastMatchTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]public long _lastMatchTimestamp;
        [BsonElement("flagMask")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _flagMask;
        [BsonElement("gamesWon")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _gamesWon;
        [BsonElement("gamesLost")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]         public int _gamesLost;
        [BsonElement("gamesDrawn")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]        public int _gamesDrawn;
        #pragma warning restore format

        [BsonIgnore] public string friendType { get => _friendType; set { _friendType = value; _parent.isDirty = true; } }
        [BsonIgnore] public long lastMatchTimestamp { get => _lastMatchTimestamp; set { _lastMatchTimestamp = value; _parent.isDirty = true; } }
        [BsonIgnore] public int flagMask { get => _flagMask; set { _flagMask = value; _parent.isDirty = true; } }
        [BsonIgnore] public int gamesWon { get => _gamesWon; set { _gamesWon = value; _parent.isDirty = true; } }
        [BsonIgnore] public int gamesLost { get => _gamesLost; set { _gamesLost = value; _parent.isDirty = true; } }
        [BsonIgnore] public int gamesDrawn { get => _gamesDrawn; set { _gamesDrawn = value; _parent.isDirty = true; } }

        // Note: invokes setters so do not use operators (+=, ++ etc)
        public int GamesWonInc() { return gamesWon = gamesWon + 1; }
        public int GamesLostInc() { return gamesLost = gamesLost + 1; }
        public int GamesDrawnInc() { return gamesDrawn = gamesDrawn + 1; }
        public int GamesWonDec() { return  gamesWon = gamesWon - 1; }
        public int GamesLostDec() { return gamesLost = gamesLost - 1; }
        public int GamesDrawnDec() { return gamesDrawn = gamesDrawn - 1; }
    }

    public class ActiveTournament
    {
        #pragma warning disable format
        [JsonIgnore][BsonIgnore]                                                                public PlayerDataTournament _parent;
        [BsonElement("shortCode")][BsonRepresentation(MongoDB.Bson.BsonType.String)]            public string _shortCode;
        [BsonElement("type")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                 public string _type;
        [BsonElement("name")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                 public string _name;
        [BsonElement("rank")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                  public int _rank;
        [BsonElement("startTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]             public long _startTime;
        [BsonElement("duration")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]              public long _duration;
        [BsonElement("score")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                 public int _score;
        [BsonElement("matchesPlayedCount")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]    public int _matchesPlayedCount;
        [BsonElement("grandPrize")]                                                             public List<TournamentReward> _grandPrize;
        #pragma warning restore format

        [BsonIgnore] public string shortCode { get => _shortCode; set { _shortCode = value; _parent.isDirty = true; } }
        [BsonIgnore] public string type { get => _type; set { _type = value; _parent.isDirty = true; } }
        [BsonIgnore] public string name { get => _name; set { _name = value; _parent.isDirty = true; } }
        [BsonIgnore] public int rank { get => _rank; set { _rank = value; _parent.isDirty = true; } }
        [BsonIgnore] public long startTime { get => _startTime; set { _startTime = value; _parent.isDirty = true; } }
        [BsonIgnore] public long duration { get => _duration; set { _duration = value; _parent.isDirty = true; } }
        [BsonIgnore] public int score { get => _score; set { _score = value; _parent.isDirty = true; } }
        [BsonIgnore] public int matchesPlayedCount { get => _matchesPlayedCount; set { _matchesPlayedCount = value; _parent.isDirty = true; } }
        [BsonIgnore] public List<TournamentReward>  grandPrize { get => _grandPrize; set { _grandPrize = value; _parent.isDirty = true; } }
    }

    public interface IDataModelBase
    {
        bool isCached { get; set; }
        public void PrepareCache();
    }

    public class DataModelBase
    {
        [JsonIgnore][BsonIgnore] public bool isCached { get; set; }
        [JsonIgnore][BsonIgnore] public bool isDirty;

        public DataModelBase() { isCached = true; }
        public void SetDirty() { isDirty = true; }
        public virtual void PrepareCache() { }
    }

    public class PlayerDataMeta : DataModelBase, IDataModelBase
    {
        #pragma warning disable format
        [BsonElement("isInitialized")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool _isInitialized;
        [BsonElement("clientVersion")][BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string _clientVersion;
        #pragma warning restore format

        [BsonIgnore] public bool isInitialized { get => _isInitialized; set { _isInitialized = value; isDirty = true; } }
        [BsonIgnore] public string clientVersion { get => _clientVersion; set { _clientVersion = value; isDirty = true; } }
    }

    public class PlayerInventoryItem
    {
        #pragma warning disable format
        [JsonIgnore][BsonIgnore]                                                public DataModelBase _parent;
        [BsonElement("key")][BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string _key;
        [BsonElement("kind")][BsonRepresentation(MongoDB.Bson.BsonType.String)] public string _kind;
        [BsonElement("json")][BsonRepresentation(MongoDB.Bson.BsonType.String)] public string _json;
        #pragma warning restore format

        [BsonIgnore] public string key { get => _key; set { _key = value; _parent.isDirty = true; } }
        [BsonIgnore] public string kind { get => _kind; set { _kind = value; _parent.isDirty = true; } }
        [BsonIgnore] public string json { get => _json; set { _json = value; _parent.isDirty = true; } }
    }

    public class PlayerDataBlocked : DataModelBase, IDataModelBase
    {
        public Dictionary<string, string> blocked;
        
        // Note: Default constructor must be defined when there is another constructor defined. 
        // Otherwiese mongo driver does not call the base constructor
        public PlayerDataBlocked() 
        {
            blocked = new Dictionary<string, string>();
        }

        public PlayerDataBlocked(bool isDirty)
        {
            blocked = new Dictionary<string, string>();
            this.isDirty = isDirty;
        }

        // public new virtual void PrepareCache()
        // {
        //     foreach (var block in blocked)
        //         block.Value._parent = this;
        // }
    }

    public class PlayerDataFriends : DataModelBase, IDataModelBase
    {
        public Dictionary<string, FriendData> friends;

        // Note: Default constructor must be defined when there is another constructor defined. 
        // Otherwiese mongo driver does not call the base constructor
        public PlayerDataFriends() 
        {
            friends = new Dictionary<string, FriendData> ();
        }

        public PlayerDataFriends(bool isDirty)
        {
            friends = new Dictionary<string, FriendData> ();
            this.isDirty = isDirty;
        }

        public void Add(string friendId, FriendData friendData)
        {
            friends.Add(friendId, friendData);
            isDirty = true;
        }

        public void Remove(string friendId)
        {
            friends.Remove(friendId);
            isDirty = true;
        }

        public FriendData CreateFriendData()
        {
            FriendData friendData = new FriendData();
            friendData._parent = this;
            return friendData;
        }

        public new virtual void PrepareCache()
        {
            foreach (var friend in friends)
                friend.Value._parent = this;
        }
    }

    public class PlayerDataInfo : DataModelBase, IDataModelBase
    {
        #pragma warning disable format
        [BsonElement("fbId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                         public string _fbId;
        [BsonElement("created")][BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]                    public DateTime _created;
        [BsonElement("isOnline")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]                    public bool _isOnline;
        [BsonElement("eloScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                      public int _eloScore;
        [BsonElement("trophies")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                      public int _trophies;
        [BsonElement("trophies2")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                     public int _trophies2;
        [BsonElement("earnings")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                      public int _earnings;
        [BsonElement("gamesWon")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                      public int _gamesWon;
        [BsonElement("gamesLost")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                     public int _gamesLost;
        [BsonElement("gamesDrawn")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                    public int _gamesDrawn;
        [BsonElement("activeInventory")]                                                                public List<PlayerInventoryItem> _activeInventory;
        [BsonElement("videosProgress")]                                                                 public Dictionary<string, float> _videosProgress;
        [BsonElement("eloCompletedPlacementGames")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]    public int _eloCompletedPlacementGames;
        [BsonElement("editedName")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                   public string _editedName;
        [BsonElement("totalGamesCount")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]               public int _totalGamesCount;
        [BsonElement("tag")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                          public string _tag;
        [BsonElement("firstLongMatchCompleted")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool _firstLongMatchCompleted;
        [BsonElement("isSearchRegistered")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]          public bool _isSearchRegistered;
        [BsonElement("isFBConnectRewardClaimed")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]    public bool _isFBConnectRewardClaimed;
        [BsonElement("careerLeagueSet")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]             public bool _careerLeagueSet;
        [BsonElement("uploadedPicId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                public string _uploadedPicId;
        [BsonElement("playDays")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                      public int _playDays;
        [BsonElement("lastPlayDay")][BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]                public DateTime _lastPlayDay;
        [BsonElement("gamesPlayedPerDay")]                                                              public Dictionary<string, GameResults> _gamesPlayedPerDay;
        
        #pragma warning restore format

        [BsonIgnore] public string fbId { get => _fbId; set { _fbId = value; isDirty = true; } }
        [BsonIgnore] public DateTime created { get => _created; set { _created = value; isDirty = true; } }
        [BsonIgnore] public bool isOnline { get => _isOnline; set { _isOnline = value; isDirty = true; } }
        [BsonIgnore] public int eloScore { get => _eloScore; set { _eloScore = value; isDirty = true; } }
        [BsonIgnore] public int trophies { get => _trophies; set { _trophies = value; isDirty = true; } }
        [BsonIgnore] public int trophies2 { get => _trophies2; set { _trophies2 = value; isDirty = true; } }
        [BsonIgnore] public int earnings { get => _earnings; set { _earnings = value; isDirty = true; } }
        [BsonIgnore] public int gamesWon { get => _gamesWon; set { _gamesWon = value; isDirty = true; } }
        [BsonIgnore] public int gamesLost { get => _gamesLost; set { _gamesLost = value; isDirty = true; } }
        [BsonIgnore] public int gamesDrawn { get => _gamesDrawn; set { _gamesDrawn = value; isDirty = true; } }
        [BsonIgnore] public List<PlayerInventoryItem> activeInventory { get => _activeInventory; set { _activeInventory = value; isDirty = true; } }
        [BsonIgnore] public Dictionary<string, float> videosProgress { get => _videosProgress; set { _videosProgress = value; isDirty = true; } }
        [BsonIgnore] public int eloCompletedPlacementGames { get => _eloCompletedPlacementGames; set { _eloCompletedPlacementGames = value; isDirty = true; } }
        [BsonIgnore] public string editedName { get => _editedName; set { _editedName = value; isDirty = true; } }
        [BsonIgnore] public int totalGamesCount { get => _totalGamesCount; set { _totalGamesCount = value; isDirty = true; } }
        [BsonIgnore] public string tag { get => _tag; set { _tag = value; isDirty = true; } }
        [BsonIgnore] public bool firstLongMatchCompleted { get => _firstLongMatchCompleted; set { _firstLongMatchCompleted = value; isDirty = true; } }
        [BsonIgnore] public bool isSearchRegistered { get => _isSearchRegistered; set { _isSearchRegistered = value; isDirty = true; } }
        [BsonIgnore] public bool isFBConnectRewardClaimed { get => _isFBConnectRewardClaimed; set { _isFBConnectRewardClaimed = value; isDirty = true; } }
        [BsonIgnore] public bool careerLeagueSet { get => _careerLeagueSet; set { _careerLeagueSet = value; isDirty = true; } }
        [BsonIgnore] public string uploadedPicId { get => _uploadedPicId; set { _uploadedPicId = value; isDirty = true; } }
        [BsonIgnore] public int playDays { get => _playDays; set { _playDays = value; isDirty = true; } }
        [BsonIgnore] public DateTime lastPlayDay { get => _lastPlayDay; set { _lastPlayDay = value; isDirty = true; } }
        [BsonIgnore] public Dictionary<string, GameResults> gamesPlayedPerDay { get => _gamesPlayedPerDay; set {_gamesPlayedPerDay = value; isDirty = true; } }

        public PlayerDataInfo()
        {
            activeInventory = new List<PlayerInventoryItem>();
            gamesPlayedPerDay = new Dictionary<string, GameResults>();
            videosProgress = new Dictionary<string, float>();
        }

        public PlayerInventoryItem CreatePlayerInventoryItem()
        {
            PlayerInventoryItem item = new PlayerInventoryItem();
            item._parent = this;
            return item;
        }

        public new virtual void PrepareCache()
        {
            foreach (var item in _activeInventory)
                item._parent = this;
        } 
    }

    public class BalloonReward : DataModelBase, IDataModelBase
    {
        #pragma warning disable format        
        [BsonElement("balloonCoins")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                      public int _balloonCoins;
        [BsonElement("balloonGems")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                       public int _balloonGems;
        [BsonElement("balloonPowerPlayMins")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]              public int _balloonPowerPlayMins;
        [BsonElement("balloonPiggyBankMins")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]              public int _balloonPiggyBankMins;
        [BsonElement("coinsRewardRatio")][BsonRepresentation(MongoDB.Bson.BsonType.Double)]                 public double _coinsRewardRatio;
        #pragma warning restore format

        [BsonIgnore] public int balloonCoins { get => _balloonCoins; set { _balloonCoins = value; isDirty = true; } }
        [BsonIgnore] public int balloonGems { get => _balloonGems; set { _balloonGems = value; isDirty = true; } }
        [BsonIgnore] public int balloonPowerPlayMins { get => _balloonPowerPlayMins; set { _balloonPowerPlayMins = value; isDirty = true; } }
        [BsonIgnore] public int balloonPiggyBankMins { get => _balloonPiggyBankMins; set { _balloonPiggyBankMins = value; isDirty = true; } }
        [BsonIgnore] public double coinsRewardRatio { get => _coinsRewardRatio; set { _coinsRewardRatio = value; isDirty = true; } }
    }

    public class PlayerDataEconomy : DataModelBase, IDataModelBase
    {
        #pragma warning disable format        
        [BsonElement("isPremium")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]                       public bool _isPremium;
        [BsonElement("removeAdsTimeStamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]                public long _removeAdsTimeStamp;
        [BsonElement("removeAdsTimePeriod")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]               public long _removeAdsTimePeriod;
        [BsonElement("subscriptionExpiryTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]            public long _subscriptionExpiryTime;
        [BsonElement("subscriptionType")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                 public string _subscriptionType;
        [BsonElement("dynamicBundlePurchaseTier")][BsonRepresentation(MongoDB.Bson.BsonType.String)]        public string _dynamicBundlePurchaseTier;
        [BsonElement("dynamicBundleDisplayTier")][BsonRepresentation(MongoDB.Bson.BsonType.String)]         public string _dynamicBundleDisplayTier;
        [BsonElement("dynamicBundlePurchaseTierNew")][BsonRepresentation(MongoDB.Bson.BsonType.String)]     public string _dynamicBundlePurchaseTierNew;
        [BsonElement("lastBundleUpdatePlayDay")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]           public int _lastBundleUpdatePlayDay;

        [BsonElement("shopRvRewardClaimedCount")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _shopRvRewardClaimedCount;
        [BsonElement("shopRVRewardClaimedDay")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]            public int _shopRVRewardClaimedDay;
        [BsonElement("shopRvDefaultBet")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                  public int _shopRvDefaultBet;
        [BsonElement("shopRvMaxReward")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                   public int _shopRvMaxReward;
        [BsonElement("balloonRewardsClaimedCount")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]        public int _balloonRewardsClaimedCount;
        [BsonElement("outOfGemsSessionCount")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]             public int _outOfGemsSessionCount;
        [BsonElement("cpuPowerupUsedCount")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]               public int _cpuPowerupUsedCount;
        [BsonElement("totalPowerupUsageCount")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]            public int _totalPowerupUsageCount;
        [BsonElement("chestUnlockTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]              public long _chestUnlockTimestamp;
        [BsonElement("rvUnlockTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]                 public long _rvUnlockTimestamp;
        [BsonElement("piggyBankGems")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                     public int _piggyBankGems;
        [BsonElement("piggyBankExpiryTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]          public long _piggyBankExpiryTimestamp;
        [BsonElement("piggyBankDoublerExipryTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long _piggyBankDoublerExipryTimestamp;
        [BsonElement("freePowerPlayExipryTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]      public long _freePowerPlayExipryTimestamp;
        [BsonElement("shopRvRewardCooldownTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]     public long _shopRvRewardCooldownTimestamp;
        [BsonElement("lastWatchedVideoId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]               public string _lastWatchedVideoId;
        [BsonElement("adsRewardData")]                                                                      public AdRewardsData _adsRewardData;
        [BsonElement("jackpotNotCollectedCounter")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]        public int _jackpotNotCollectedCounter;
        [BsonElement("balloonReward")]                                                                      public BalloonReward _balloonReward;

        #pragma warning restore format

        [BsonIgnore] public bool isPremium { get => _isPremium; set { _isPremium = value; isDirty = true; } }
        [BsonIgnore] public long removeAdsTimeStamp { get => _removeAdsTimeStamp; set { _removeAdsTimeStamp = value; isDirty = true; } }
        [BsonIgnore] public long removeAdsTimePeriod { get => _removeAdsTimePeriod; set { _removeAdsTimePeriod = value; isDirty = true; } }
        [BsonIgnore] public long subscriptionExpiryTime { get => _subscriptionExpiryTime; set { _subscriptionExpiryTime = value; isDirty = true; } }
        [BsonIgnore] public string subscriptionType { get => _subscriptionType; set { _subscriptionType = value; isDirty = true; } }
        [BsonIgnore] public string dynamicBundlePurchaseTier { get => _dynamicBundlePurchaseTier; set { _dynamicBundlePurchaseTier = value; isDirty = true; } }
        [BsonIgnore] public string dynamicBundleDisplayTier { get => _dynamicBundleDisplayTier; set { _dynamicBundleDisplayTier = value; isDirty = true; } }
        [BsonIgnore] public string dynamicBundlePurchaseTierNew { get => _dynamicBundlePurchaseTierNew; set { _dynamicBundlePurchaseTierNew = value; isDirty = true; } }
        [BsonIgnore] public int lastBundleUpdatePlayDay { get => _lastBundleUpdatePlayDay; set { _lastBundleUpdatePlayDay = value; isDirty = true; } }
        [BsonIgnore] public int shopRvRewardClaimedCount { get => _shopRvRewardClaimedCount; set { _shopRvRewardClaimedCount = value; isDirty = true; } }
        [BsonIgnore] public int shopRVRewardClaimedDay { get => _shopRVRewardClaimedDay; set { _shopRVRewardClaimedDay = value; isDirty = true; } }
        [BsonIgnore] public int shopRvDefaultBet { get => _shopRvDefaultBet; set { _shopRvDefaultBet = value; isDirty = true; } }
        [BsonIgnore] public int shopRvMaxReward { get => _shopRvMaxReward; set { _shopRvMaxReward = value; isDirty = true; } }
        [BsonIgnore] public int balloonRewardsClaimedCount { get => _balloonRewardsClaimedCount; set { _balloonRewardsClaimedCount = value; isDirty = true; } }
        [BsonIgnore] public int outOfGemsSessionCount { get => _outOfGemsSessionCount; set { _outOfGemsSessionCount = value; isDirty = true; } }
        [BsonIgnore] public int cpuPowerupUsedCount { get => _cpuPowerupUsedCount; set { _cpuPowerupUsedCount = value; isDirty = true; } }
        [BsonIgnore] public int totalPowerupUsageCount { get => _totalPowerupUsageCount; set { _totalPowerupUsageCount = value; isDirty = true; } }
        [BsonIgnore] public long chestUnlockTimestamp { get => _chestUnlockTimestamp; set { _chestUnlockTimestamp = value; isDirty = true; } }
        [BsonIgnore] public long rvUnlockTimestamp { get => _rvUnlockTimestamp; set { _rvUnlockTimestamp = value; isDirty = true; } }
        [BsonIgnore] public int piggyBankGems { get => _piggyBankGems; set { _piggyBankGems = value; isDirty = true; } }
        [BsonIgnore] public long piggyBankExpiryTimestamp { get => _piggyBankExpiryTimestamp; set { _piggyBankExpiryTimestamp = value; isDirty = true; } }
        [BsonIgnore] public long piggyBankDoublerExipryTimestamp { get => _piggyBankDoublerExipryTimestamp; set { _piggyBankDoublerExipryTimestamp = value; isDirty = true; } }
        [BsonIgnore] public long freePowerPlayExipryTimestamp { get => _freePowerPlayExipryTimestamp; set { _freePowerPlayExipryTimestamp = value; isDirty = true; } }
        [BsonIgnore] public long shopRvRewardCooldownTimestamp { get => _shopRvRewardCooldownTimestamp; set { _shopRvRewardCooldownTimestamp = value; isDirty = true; } }
        [BsonIgnore] public string lastWatchedVideoId { get => _lastWatchedVideoId; set { _lastWatchedVideoId = value; isDirty = true; } }
        [BsonIgnore] public AdRewardsData adsRewardData { get => _adsRewardData; set { _adsRewardData = value; isDirty = true; } }
        [BsonIgnore] public int jackpotNotCollectedCounter { get => _jackpotNotCollectedCounter; set { _jackpotNotCollectedCounter = value; isDirty = true; } }
        [BsonIgnore] public BalloonReward balloonReward { get => _balloonReward; set { _balloonReward = value; isDirty = true; } }

        public PlayerDataEconomy()
        {
            adsRewardData = new AdRewardsData();
        }
    }

    public class PlayerDataEvent : DataModelBase, IDataModelBase
    {
        #pragma warning disable format         
        [BsonElement("eventTimeStamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]            public long _eventTimeStamp;
        [BsonElement("dailyEventExpiryTimestamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)] public long _dailyEventExpiryTimestamp;
        [BsonElement("dailyEventProgress")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]        public int _dailyEventProgress;
        [BsonElement("dailyEventState")][BsonRepresentation(MongoDB.Bson.BsonType.String)]          public string _dailyEventState;
        [BsonElement("dailyEventRewards")]                                                          public List<DailyEventRewards> _dailyEventRewards;
        #pragma warning restore format

        [BsonIgnore] public long eventTimeStamp { get => _eventTimeStamp; set { _eventTimeStamp = value; isDirty = true; } }
        [BsonIgnore] public long dailyEventExpiryTimestamp { get => _dailyEventExpiryTimestamp; set { _dailyEventExpiryTimestamp = value; isDirty = true; } }
        [BsonIgnore] public int dailyEventProgress { get => _dailyEventProgress; set { _dailyEventProgress = value; isDirty = true; } }
        [BsonIgnore] public string dailyEventState { get => _dailyEventState; set { _dailyEventState = value; isDirty = true; } }
        [BsonIgnore] public List<DailyEventRewards> dailyEventRewards { get => _dailyEventRewards; set { _dailyEventRewards = value; isDirty = true; } }

        public PlayerDataEvent()
        {
            dailyEventRewards = new List<DailyEventRewards>();
        }
    }

    public class PlayerDataTournament : DataModelBase, IDataModelBase
    { 
        #pragma warning disable format 
        [BsonElement("isReportingInChampionship")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]           public bool _isReportingInChampionship;
        [BsonElement("reportingChampionshipCollectionIndex")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]  public int _reportingChampionshipCollectionIndex;
        [BsonElement("tournamentMaxScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                    public int _tournamentMaxScore;
        [BsonElement("playerTimeZoneSlot")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                    public int _playerTimeZoneSlot;
        [BsonElement("activeTournaments")]                                                                      public Dictionary<string, ActiveTournament> _activeTournaments;
        #pragma warning restore format

        [BsonIgnore] public bool isReportingInChampionship { get => _isReportingInChampionship; set { _isReportingInChampionship = value; isDirty = true; } }
        [BsonIgnore] public int reportingChampionshipCollectionIndex { get => _reportingChampionshipCollectionIndex; set { _reportingChampionshipCollectionIndex = value; isDirty = true; } }
        [BsonIgnore] public int tournamentMaxScore { get => _tournamentMaxScore; set { _tournamentMaxScore = value; isDirty = true; } }
        [BsonIgnore] public int playerTimeZoneSlot { get => _playerTimeZoneSlot; set { _playerTimeZoneSlot = value; isDirty = true; } }
        [BsonIgnore] public Dictionary<string, ActiveTournament> activeTournaments { get => _activeTournaments; set { _activeTournaments = value; isDirty = true; } }

        public PlayerDataTournament()
        {
            reportingChampionshipCollectionIndex = -1;
            activeTournaments = new Dictionary<string, ActiveTournament>();
        }

        public void RemoveActiveTournament(string id)
        {
            _activeTournaments.Remove(id);
            isDirty = true;
        }

        public new virtual void PrepareCache()
        {
            foreach (var activeTournament in _activeTournaments)
                activeTournament.Value._parent = this;
        }

        public ActiveTournament CreatePlayerActiveTournament(string tournamentId, TournamentData tournament, int playerRank, int defaultScore)
        {
            ActiveTournament activeTournament = new ActiveTournament()
            {
                _parent = this,
                shortCode = tournament.shortCode,
                type = tournament.type,
                name = tournament.name,
                rank = playerRank,
                grandPrize = tournament.rewards["0"],
                startTime = tournament.startTime,
                duration = tournament.duration,
                score = defaultScore,
                matchesPlayedCount = 0
            };

            _activeTournaments.Add(tournamentId, activeTournament);
            isDirty = true;

            return activeTournament;
        }
    }

    public class PlayerDataChallenge : DataModelBase, IDataModelBase
    {        
        #pragma warning disable format                                                        
        [BsonElement("currentChallengeId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]   public string _currentChallengeId;
        [BsonElement("activeChallenges")]                                                       public Dictionary<string, string> _activeChallenges;
        [BsonElement("pendingChallenges")]                                                      public Dictionary<string, string> _pendingChallenges;
        #pragma warning restore format

        [BsonIgnore] public string currentChallengeId { get => _currentChallengeId; set { _currentChallengeId = value; isDirty = true; } }
        [BsonIgnore] public Dictionary<string, string> activeChallenges { get => _activeChallenges; set { _activeChallenges = value; isDirty = true; } }
        [BsonIgnore] public Dictionary<string, string> pendingChallenges { get => _pendingChallenges; set { _pendingChallenges = value; isDirty = true; } }

        // Note: Default constructor must be defined when there is another constructor defined. 
        // Otherwiese mongo driver does not call the base constructor
        public PlayerDataChallenge() 
        {
            _activeChallenges = new Dictionary<string, string>();
            _pendingChallenges = new Dictionary<string, string>();
        }

        public PlayerDataChallenge(bool isDirty)
        {
            _activeChallenges = new Dictionary<string, string>();
            _pendingChallenges = new Dictionary<string, string>();
            this.isDirty = isDirty;
        }
    }

    public class PlayerModelDocument
    {
        #pragma warning disable format                                                        
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]    public string _id;
        [BsonElement("PlayerDataModel")]                        public PlayerDataModel _model;
        #pragma warning restore format
    }

    public class PlayerDataModel
    {
        [BsonIgnore] private const string PLAYER_MODEL_COLLECTION = "playerModel";
        [BsonIgnore] private SocialEdgePlayerContext _socialEdgePlayer;
        [BsonIgnore] private bool _isCached;

        [BsonIgnore] private UpdateDefinitionBuilder<PlayerDataModel> update = Builders<PlayerDataModel>.Update;
        [BsonIgnore] private List<UpdateDefinition<PlayerDataModel>> updates = new List<UpdateDefinition<PlayerDataModel>>();
    
        // IMPORTANT: Field name have format '_<fieldname>' and Element name must have format '<fieldname>
        #pragma warning disable format                                                        
        [JsonIgnore][BsonElement("meta")][BsonIgnoreIfNull]         public PlayerDataMeta _meta;
        [JsonIgnore][BsonElement("info")][BsonIgnoreIfNull]         public PlayerDataInfo _info;
        [JsonIgnore][BsonElement("economy")][BsonIgnoreIfNull]      public PlayerDataEconomy _economy;
        [JsonIgnore][BsonElement("events")][BsonIgnoreIfNull]       public PlayerDataEvent _events;
        [JsonIgnore][BsonElement("tournament")][BsonIgnoreIfNull]   public PlayerDataTournament _tournament;
        [JsonIgnore][BsonElement("challenge")][BsonIgnoreIfNull]    public PlayerDataChallenge _challenge;
        [JsonIgnore][BsonElement("friends")][BsonIgnoreIfNull]      public PlayerDataFriends _friends;
        [JsonIgnore][BsonElement("blocked")][BsonIgnoreIfNull]      public PlayerDataBlocked _blocked;
        #pragma warning restore format

        public PlayerDataMeta Meta { get => _meta != null && _meta.isCached ? _meta : _meta = Get<PlayerDataMeta>(nameof(_meta)); }
        public PlayerDataInfo Info { get => _info != null && _info.isCached ? _info : _info = Get<PlayerDataInfo>(nameof(_info)); }
        public PlayerDataEconomy Economy { get => _economy != null && _economy.isCached ? _economy : _economy = Get<PlayerDataEconomy>(nameof(_economy)); }
        public PlayerDataEvent Events { get => _events != null && _events.isCached ? _events : _events = Get<PlayerDataEvent>(nameof(_events)); }
        public PlayerDataTournament Tournament { get => _tournament != null && _tournament.isCached ? _tournament : _tournament = Get<PlayerDataTournament>(nameof(_tournament)); }
        public PlayerDataChallenge Challenge { get => _challenge != null && _challenge.isCached ? _challenge : _challenge = Get<PlayerDataChallenge>(nameof(_challenge)); }
        public PlayerDataFriends Friends { get => _friends != null && _friends.isCached ? _friends : _friends = Get<PlayerDataFriends>(nameof(_friends)); }
        public PlayerDataBlocked Blocked { get => _blocked != null && _blocked.isCached ? _blocked : _blocked = Get<PlayerDataBlocked>(nameof(_blocked)); }

        public void DBOpAddFriend(string friendId, FriendData friendData)
        {
            updates.Add(update.Set<FriendData>(typeof(PlayerDataModel).Name + ".friends.friends."+friendId, friendData));
        }

        public void DBOpRemoveFriend(string friendId)
        {
            updates.Add(update.Unset(typeof(PlayerDataModel).Name + ".friends.friends."+friendId));
        }

        public FriendData CreateFriendData()
        {
            FriendData friendData = new FriendData();
            friendData._parent = _friends;
            return friendData;
        }

        public void DBOpUnblockFriend(string friendId)
        {
            updates.Add(update.Unset(typeof(PlayerDataModel).Name + ".blocked.blocked."+friendId));
        }

        public void DBOpBlockFriend(string friendId, string displayName)
        {
            updates.Add(update.Set<string>(typeof(PlayerDataModel).Name + ".blocked.blocked."+friendId, displayName));
        }

        public void ReadOnly() { _isCached = false; }

        public PlayerDataModel(SocialEdgePlayerContext socialEdgePlayer)
        {
            _socialEdgePlayer = socialEdgePlayer;
            _socialEdgePlayer.SetDirtyBit(CachePlayerDataSegments.PLAYER_MODEL);
            _isCached = true;
        }

        private T Get<T>(string fieldName) where T : IDataModelBase
        {
            string elemName = fieldName.Substring(1);
            var collection = SocialEdge.DataService.GetCollection<PlayerModelDocument>(PLAYER_MODEL_COLLECTION);
            var projection = Builders<PlayerModelDocument>.Projection.Include(typeof(PlayerDataModel).Name + "." + elemName).Exclude("_id");
            var taskT = collection.FindOneById<PlayerModelDocument>(_socialEdgePlayer.PlayerDBId, projection);
            taskT.Wait();

            var field = taskT.Result != null ? (T)taskT.Result._model.GetType().GetField(fieldName).GetValue(taskT.Result._model) : (T)Activator.CreateInstance(typeof(T));
            field.isCached = true;
            field.PrepareCache();

            SocialEdge.Log.LogInformation("Task fetch PLAYER_MODEL:" + elemName + " " + (taskT.Result != null ? "(success)" : "(default)"));
            return field;
        }

        internal bool CacheWrite()
        {
            if (_isCached == false)
                return false;

            var collection = SocialEdge.DataService.GetCollection<PlayerDataModel>(PLAYER_MODEL_COLLECTION);
            //var update = Builders<PlayerDataModel>.Update;
            //var updates = new List<UpdateDefinition<PlayerDataModel>>();

            if (_meta != null && _meta.isDirty)
                updates.Add(update.Set<PlayerDataMeta>(typeof(PlayerDataModel).Name + ".meta", _meta));
            if (_info != null && _info.isDirty)
                updates.Add(update.Set<PlayerDataInfo>(typeof(PlayerDataModel).Name + ".info", _info));
            if (_economy != null && _economy.isDirty)
                updates.Add(update.Set<PlayerDataEconomy>(typeof(PlayerDataModel).Name + ".economy", _economy));
            if (_events != null && _events.isDirty)
                updates.Add(update.Set<PlayerDataEvent>(typeof(PlayerDataModel).Name + ".events", _events));
            if (_tournament != null && _tournament.isDirty)
                updates.Add(update.Set<PlayerDataTournament>(typeof(PlayerDataModel).Name + ".tournament", _tournament));
            if (_challenge != null && _challenge.isDirty)
                updates.Add(update.Set<PlayerDataChallenge>(typeof(PlayerDataModel).Name + ".challenge", _challenge));
            if (_friends != null && _friends.isDirty)
                updates.Add(update.Set<PlayerDataFriends>(typeof(PlayerDataModel).Name + ".friends", _friends));
            if (_blocked != null && _blocked.isDirty)
                updates.Add(update.Set<PlayerDataBlocked>(typeof(PlayerDataModel).Name + ".blocked", _blocked));

            if (updates.Count == 0)
                return false;

            var taskT = collection.UpdateOneById(_socialEdgePlayer.PlayerDBId, update.Combine(updates), true);
            taskT.Wait();
            SocialEdge.Log.LogInformation("Task flush PLAYER_MODEL");
            return taskT.Result.ModifiedCount != 0;
        }

        public void CreateDefaults()
        {
            _meta = new PlayerDataMeta();
            _info = new PlayerDataInfo();
            _economy = new PlayerDataEconomy();
            _events = new PlayerDataEvent();
            _tournament = new PlayerDataTournament();
            _challenge = new PlayerDataChallenge(isDirty:true);
            _friends = new PlayerDataFriends(isDirty:true);
            _blocked = new PlayerDataBlocked(isDirty:true);
        }

        public void Prefetch(params string[] fields)
        {
            var collection = SocialEdge.DataService.GetCollection<PlayerModelDocument>(PLAYER_MODEL_COLLECTION);
            var projection = Builders<PlayerModelDocument>.Projection.Include(fields[0]).Exclude("_id");
            for (int i = 1; i < fields.Length; i++)
            {
                projection = projection.Include(fields[i]);
            }

            var taskT = collection.FindOneById<PlayerModelDocument>(_socialEdgePlayer.PlayerDBId, projection);
            taskT.Wait();

            SocialEdge.Log.LogInformation("Task fetch PLAYER_MODEL fields:" + " " + (taskT.Result != null ? "(success)" : "(null)"));

            PlayerDataModel model = taskT.Result._model;
            _meta = model._meta != null ? model._meta : _meta;
            _info = model._info != null ? model._info : _info;
            _economy = model._economy != null ? model._economy : _economy;
            _events = model._events != null ? model._events : _events;
            _tournament = model._tournament != null ? model._tournament : _tournament;
            _challenge = model._challenge != null ? model._challenge : _challenge;
            _friends = _friends == null && model._friends != null ? model._friends : _friends;
            _blocked = _blocked == null && model._blocked != null ? model._blocked : _blocked;

            if (_meta != null)
                _meta.PrepareCache();

            if (_info != null)
                _info.PrepareCache();

            if (_economy != null)
                _economy.PrepareCache();

            if (_events != null)
                _events.PrepareCache();

            if (_tournament != null)
                _tournament.PrepareCache();

            if (_challenge != null)
                _challenge.PrepareCache();

            if (_friends != null)
                _friends.PrepareCache();

            if (_blocked != null)
                _blocked.PrepareCache();
        }
        public GSPlayerModelDocument GetGSPlayerData(SocialEdgePlayerContext socialEdgePlayer, string deviceId, string fbId, string appleId)
        {
            SocialEdge.Log.LogInformation("PLAYER IDs deviceId: " + deviceId + " fbId: "+fbId + " appleId: "+appleId);
            GSPlayerModelDocument gsPlayerData  = null;
            string query = null;
            string findId = null;

            if(!string.IsNullOrEmpty(fbId)){
                query = "PlayerDataModel.facebookId";
                findId = fbId;
            }
            else if(!string.IsNullOrEmpty(appleId)){
                query = "PlayerDataModel.appleId";
                findId = appleId;
            }
            else{
                query = "PlayerDataModel.deviceId";
                findId = deviceId;
            }
            
            SocialEdge.Log.LogInformation("FIND QUERY: " + query + " findId: "+findId);

            if(!string.IsNullOrEmpty(findId)){

                var collection =  SocialEdge.DataService.GetCollection<GSPlayerModelDocument>("gsDataCollection");
                var taskT = collection.FindOne(query, findId);
                taskT.Wait(); 

                if(taskT.Result != null){
                    gsPlayerData = taskT.Result;
                }
            }
            
            return gsPlayerData;
        }
       
        
       
        /*
                private void Set<T>(string id, T val)
                {
                    const string PLAYER_MODEL_COLLECTION = "playerModel";
                    var collection = SocialEdge.DataService.GetCollection<PlayerModel>(PLAYER_MODEL_COLLECTION);
                    var update = Builders<PlayerModel>.Update.Set<T>(typeof(T).Name, val);

                    var findT = collection.UpdateOneById<T>(id, typeof(T).Name, val);
                    findT.Wait();
                } 

                public void Get(string id, List<string> fields)
                {
                    const string PLAYER_MODEL_COLLECTION = "playerModel";
                    var collection = SocialEdge.DataService.GetCollection<PlayerModel>(PLAYER_MODEL_COLLECTION);
                    var projection = Builders<PlayerModel>.Projection.Include(fields[0]);
                    for(int i = 1; i < fields.Count; i++)
                    {
                        projection = projection.Include(fields[i]);
                    }

                    var findT = collection.FindOneById<PlayerModel>(id, projection);
                    findT.Wait();
                }


                public static void Set(string id, List<string> fields)
                {
                    const string PLAYER_MODEL_COLLECTION = "playerModel";
                    var collection = SocialEdge.DataService.GetCollection<PlayerModelDocument>(PLAYER_MODEL_COLLECTION);
                    var update = Builders<PlayerModelDocument>.Update.Set<Type.GetType(fields[0])>(Type.GetType(fields[0].ToString(), val);

                    var replaceT = collection.UpdateOneById(model._id, model, true);
                    replaceT.Wait();
                }
                */
    }
}
