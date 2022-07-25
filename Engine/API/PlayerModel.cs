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
    public class AdRewardsData
    {

    }

    public class DailyEventRewards
    {

    }

    public class ActiveTournament
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string shortCode;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string type;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string name;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int rank;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long startTime;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long duration;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int score;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int matchesPlayedCount;
                                                            public List<TournamentReward> grandPrize;
    }

    public class DataModelBase
    {
        [JsonIgnore][BsonIgnore] public bool isCached;
        [JsonIgnore][BsonIgnore] public bool isDirty;
        public DataModelBase() { isCached = true; }
    }

    public class PlayerDataMeta : DataModelBase
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool isInitialized;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string clientVersion;
    }

    public class PlayerInventoryItem
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string key;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string kind;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string json;
    }

    public class PlayerPublicProfileEx
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int eloScore;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int trophies;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int earnings;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int win;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int lose;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int draw;
                                                            public List<PlayerInventoryItem> activeInventory;
    }

    public class PlayerDataInfo : DataModelBase
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int eloScore;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int trophies;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int trophies2;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int earnings;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int win;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int lose;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int draw;
                                                            public List<PlayerInventoryItem> activeInventory;

        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int league;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int eloCompletedPlacementGames;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string editedName;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int totalGamesCount;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string tag;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool firstLongMatchCompleted;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool isSearchRegistered;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool isFBConnectRewardClaimed;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool careerLeagueSet;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string uploadedPicId;

        public PlayerDataInfo()
        {
            activeInventory = new List<PlayerInventoryItem>();
        }
    }

    public class PlayerDataEconomy : DataModelBase
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool isPremium;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long removeAdsTimeStamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long removeAdsTimePeriod;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long subscriptionExpiryTime;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string subscriptionType;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string dynamicBundlePurchaseTier;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string dynamicBundleDisplayTier;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string dynamicBundlePurchaseTierNew;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int lastBundleUpdatePlayDay;

        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int shopRvRewardClaimedCount;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int balloonRewardsClaimedCount;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int outOfGemsSessionCount;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int cpuPowerupUsedCount;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int totalPowerupUsageCount;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long chestUnlockTimestamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long rvUnlockTimestamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long piggyBankExpiryTimestamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long piggyBankDoublerExipryTimestamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long freePowerPlayExipryTimestamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long shopRvRewardCooldownTimestamp;

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string lastWatchedVideoId;
                                                            public AdRewardsData adsRewardData;

        public PlayerDataEconomy()
        {
            adsRewardData = new AdRewardsData();
        }
    }

    public class PlayerDataEvent : DataModelBase
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long eventTimeStamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long dailyEventExpiryTimestamp;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int dailyEventProgress;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int dailyEventState;
                                                            public DailyEventRewards dailyEventRewards;

        public PlayerDataEvent()
        {
            dailyEventRewards = new DailyEventRewards();
        }
    }

    public class PlayerDataTournament : DataModelBase
    { 
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool isReportingInChampionship;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int reportingChampionshipCollectionIndex;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int tournamentMaxScore;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int playerTimeZoneSlot;
                                                            public Dictionary<string, ActiveTournament> activeTournaments;

        public PlayerDataTournament()
        {
            reportingChampionshipCollectionIndex = -1;
            activeTournaments = new Dictionary<string, ActiveTournament>();
        }
    }

    public class PlayerDataChallenge : DataModelBase
    {                                                       
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string currentChallengeId;
                                                            public Dictionary<string, string> activeChallenges;
                                                            public Dictionary<string, string> pendingChallenges;
    }

    public class PlayerModelDocument
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] public string _id;
        [BsonElement("PlayerDataModel")] public PlayerDataModel _model;
    }

    public class PlayerDataModel
    {
        [BsonIgnore] private const string PLAYER_MODEL_COLLECTION = "playerModel";
        [BsonIgnore] private SocialEdgePlayerContext _socialEdgePlayer;
        [BsonIgnore] private bool _isCached;
    
        // IMPORTANT: Field name have format '_<fieldname>' and Element name must have format '<fieldname>
        [JsonIgnore][BsonElement("meta")][BsonIgnoreIfNull]         public PlayerDataMeta _meta;
        [JsonIgnore][BsonElement("info")][BsonIgnoreIfNull]         public PlayerDataInfo _info;
        [JsonIgnore][BsonElement("economy")][BsonIgnoreIfNull]      public PlayerDataEconomy _economy;
        [JsonIgnore][BsonElement("events")][BsonIgnoreIfNull]       public PlayerDataEvent _events;
        [JsonIgnore][BsonElement("tournament")][BsonIgnoreIfNull]   public PlayerDataTournament _tournament;
        [JsonIgnore][BsonElement("challenge")][BsonIgnoreIfNull]    public PlayerDataChallenge _challenge;

        public PlayerDataMeta Meta { get => _meta != null && _meta.isCached ? _meta : _meta = Get<PlayerDataMeta>(nameof(_meta)); }
        public PlayerDataInfo Info { get => _info != null && _info.isCached ? _info : _info = Get<PlayerDataInfo>(nameof(_info)); }
        public PlayerDataEconomy Economy { get => _economy != null && _economy.isCached ? _economy : _economy = Get<PlayerDataEconomy>(nameof(_economy)); }
        public PlayerDataEvent Events { get => _events != null && _events.isCached ? _events : _events = Get<PlayerDataEvent>(nameof(_events)); }
        public PlayerDataTournament Tournament { get => _tournament != null && _tournament.isCached ? _tournament : _tournament = Get<PlayerDataTournament>(nameof(_tournament)); }
        public PlayerDataChallenge Challenge { get => _challenge != null && _challenge.isCached ? _challenge : _challenge = Get<PlayerDataChallenge>(nameof(_challenge)); }

        public void ReadOnly() { _isCached = false; }
        
        public PlayerDataModel(SocialEdgePlayerContext socialEdgePlayer)
        {
            _socialEdgePlayer = socialEdgePlayer; 
            _socialEdgePlayer.SetDirtyBit(CachePlayerDataSegments.PLAYER_MODEL);
            _isCached = true;
        } 

        private T Get<T>(string fieldName)
        {
            string elemName = fieldName.Substring(1);
            var collection = SocialEdge.DataService.GetCollection<PlayerModelDocument>(PLAYER_MODEL_COLLECTION);
            var projection = Builders<PlayerModelDocument>.Projection.Include(typeof(PlayerDataModel).Name + "." + elemName).Exclude("_id");
            var taskT = collection.FindOneById<PlayerModelDocument>(_socialEdgePlayer.PlayerDBId, projection);
            taskT.Wait();

            SocialEdge.Log.LogInformation("Task fetch PLAYER_MODEL:" + elemName + " " + (taskT.Result != null ? "(success)" : "(default)"));

            return taskT.Result != null ? (T)taskT.Result._model.GetType().GetField(fieldName).GetValue(taskT.Result._model) : (T)Activator.CreateInstance(typeof(T));
        }

        public PlayerDataModel Fetch()
        {
            var collection = SocialEdge.DataService.GetCollection<PlayerModelDocument>(PLAYER_MODEL_COLLECTION);
            var taskT = collection.FindOneById(_socialEdgePlayer.PlayerDBId);
            taskT.Wait();
            if (taskT.Result != null)
                Fill(taskT.Result._model);

            return this;
        }       
        
        internal bool CacheWrite()
        {
            if (_isCached == false)
                return false;

            var collection = SocialEdge.DataService.GetCollection<PlayerDataModel>(PLAYER_MODEL_COLLECTION);
            var update = Builders<PlayerDataModel>.Update;
            var updates = new List<UpdateDefinition<PlayerDataModel>>();

            if (_meta != null)
                updates.Add(update.Set<PlayerDataMeta>(typeof(PlayerDataModel).Name + ".meta", _meta));
            if (_info != null)
                updates.Add(update.Set<PlayerDataInfo>(typeof(PlayerDataModel).Name + ".info", _info));
            if (_economy != null)
                updates.Add(update.Set<PlayerDataEconomy>(typeof(PlayerDataModel).Name + ".economy", _economy));
            if (_events != null)
                 updates.Add(update.Set<PlayerDataEvent>(typeof(PlayerDataModel).Name + ".events", _events));
            if (_tournament != null)
                updates.Add(update.Set<PlayerDataTournament>(typeof(PlayerDataModel).Name + ".tournament", _tournament));
            if (_challenge != null)
                updates.Add(update.Set<PlayerDataChallenge>(typeof(PlayerDataModel).Name + ".challenge", _challenge));

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
            _info = new PlayerDataInfo ();
            _economy = new PlayerDataEconomy();
            _events = new PlayerDataEvent();
            _tournament = new PlayerDataTournament();
            _challenge = new PlayerDataChallenge();
        }

        private void Fill(PlayerDataModel model)
        {
            _meta = _meta == null && model._meta != null ? model._meta : _meta;
            _info = _info == null && model._info != null ? model._info : _info;
            _economy = _economy == null && model._economy != null ? model._economy : _economy;
            _events = _events == null && model._events != null ? model._events : _events;
            _tournament = _tournament == null && model._tournament != null ? model._tournament : _tournament;
            _challenge = _challenge == null && model._challenge != null ? model._challenge : _challenge;
        }
/*
        public void Get(List<string> fields)
        {
            var collection = SocialEdge.DataService.GetCollection<PlayerModel>(PLAYER_MODEL_COLLECTION);
            var projection = Builders<PlayerModel>.Projection.Include(fields[0]).Exclude("_id");
            for(int i = 1; i < fields.Count; i++)
            {
                projection = projection.Include(fields[i]);
            }

            var taskT = collection.FindOneById<PlayerModel>(_socialEdgePlayer.PlayerDBId, projection);
            taskT.Wait();

            PlayerModel model = taskT.Result;
            _meta = model._meta != null ? model._meta : _meta;
            _info = model._info != null ? model._info : _info;
            _economy = model._economy != null ? model._economy : _economy;
            _events = model._events != null ? model._events : _events;
            _tournament = model._tournament != null ? model._tournament : _tournament;
            _challenge = model._challenge != null ? model._challenge : _challenge;
        }


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
