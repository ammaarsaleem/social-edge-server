/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Models
{
    public class ChallengeWinnerBonusRewardsData
    {
        #pragma warning disable format
        [JsonIgnore][BsonIgnore]                                                            public DataModelBase _parent;
        [BsonElement("_bonusCoinsFree1")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]  public int _bonusCoinsFree1;
        [BsonElement("_bonusCoinsFree2")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]  public int _bonusCoinsFree2;
        [BsonElement("_bonusCoinsFree3")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]  public int _bonusCoinsFree3;
        [BsonElement("_bonusCoinsRV1")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]    public int _bonusCoinsRV1;
        [BsonElement("_bonusCoinsRV2")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]    public int _bonusCoinsRV2;
        [BsonElement("_bonusCoinsRV3")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]    public int _bonusCoinsRV3;
        #pragma warning restore format

        [BsonIgnore] public int bonusCoinsFree1 { get => _bonusCoinsFree1; set { _bonusCoinsFree1 = value; _parent.isDirty = true; } }
        [BsonIgnore] public int bonusCoinsFree2 { get => _bonusCoinsFree2; set { _bonusCoinsFree2 = value; _parent.isDirty = true; } }
        [BsonIgnore] public int bonusCoinsFree3 { get => _bonusCoinsFree3; set { _bonusCoinsFree3 = value; _parent.isDirty = true; } }
        [BsonIgnore] public int bonusCoinsRV1 { get => _bonusCoinsRV1; set { _bonusCoinsRV1 = value; _parent.isDirty = true; } }
        [BsonIgnore] public int bonusCoinsRV2 { get => _bonusCoinsRV2; set { _bonusCoinsRV2 = value; _parent.isDirty = true; } }
        [BsonIgnore] public int bonusCoinsRV3 { get => _bonusCoinsRV3; set { _bonusCoinsRV3 = value; _parent.isDirty = true; } }
    }

    public class ChallengePlayerModel
    {
        #pragma warning disable format
        [JsonIgnore][BsonIgnore]                                                            public DataModelBase _parent;
        [BsonElement("betValue")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]          public long _betValue;
        [BsonElement("isEventMatch")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]    public bool _isEventMatch;
        [BsonElement("powerMode")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]       public bool _powerMode;
        [BsonElement("tournamentId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]     public string _tournamentId;
        [BsonElement("isBot")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]           public bool _isBot;
        [BsonElement("playerColor")][BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string _playerColor;

        [BsonElement("eloScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _eloScore;
        [BsonElement("eloChange")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]         public int _eloChange;
        [BsonElement("coinsMultiplyer")][BsonRepresentation(MongoDB.Bson.BsonType.Double)]  public double _coinsMultiplyer;
        [BsonElement("tournamentScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int _tournamentScore;
        [BsonElement("promoted")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]        public bool _promoted;

        [BsonElement("piggyBankReward")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int _piggyBankReward;
                                                                                            public ChallengeWinnerBonusRewardsData _winnerBonusRewards;
        #pragma warning restore format

        [BsonIgnore] public long betValue { get => _betValue; set { _betValue = value; _parent.isDirty = true; } }
        [BsonIgnore] public bool isEventMatch { get => _isEventMatch; set { _isEventMatch = value; _parent.isDirty = true; } }
        [BsonIgnore] public bool powerMode { get => _powerMode; set { _powerMode = value; _parent.isDirty = true; } }
        [BsonIgnore] public string tournamentId { get => _tournamentId; set { _tournamentId = value; _parent.isDirty = true; } }
        [BsonIgnore] public bool isBot { get => _isBot; set { _isBot = value; _parent.isDirty = true; } }
        [BsonIgnore] public string playerColor { get => _playerColor; set { _playerColor = value; _parent.isDirty = true; } }
        [BsonIgnore] public int eloScore { get => _eloScore; set { _eloScore = value; _parent.isDirty = true; } }
        [BsonIgnore] public int eloChange { get => _eloChange; set { _eloChange = value; _parent.isDirty = true; } }
        [BsonIgnore] public double coinsMultiplyer { get => _coinsMultiplyer; set { _coinsMultiplyer = value; _parent.isDirty = true; } }
        [BsonIgnore] public int tournamentScore { get => _tournamentScore; set { _tournamentScore = value; _parent.isDirty = true; } }
        [BsonIgnore] public bool promoted { get => _promoted; set { _promoted = value; _parent.isDirty = true; } }
        [BsonIgnore] public int piggyBankReward { get => _piggyBankReward; set { _piggyBankReward = value; _parent.isDirty = true; } }
        [BsonIgnore] public ChallengeWinnerBonusRewardsData winnerBonusRewards { get => _winnerBonusRewards; set { _winnerBonusRewards = value; _parent.isDirty = true; } }

        public ChallengeWinnerBonusRewardsData CreateChallengeWinnerBonusReward()
        {
            ChallengeWinnerBonusRewardsData challengeWinnerBonusRewardsData = new ChallengeWinnerBonusRewardsData();
            challengeWinnerBonusRewardsData._parent = _parent;
            return challengeWinnerBonusRewardsData;
        }
    }

    public class ChallengeData : DataModelBase
    {
        #pragma warning disable format
        [BsonElement("activeTimeStamp")][BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]    public DateTime _activeTimeStamp;
        [BsonElement("matchDuration")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]         public long _matchDuration;
        [BsonElement("isRanked")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]            public bool _isRanked;
        [BsonElement("gameEndReason")][BsonRepresentation(MongoDB.Bson.BsonType.String)]        public string _gameEndReason; 
        [BsonElement("winnerId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]             public string _winnerId; 
        [BsonElement("playersData")]                                                            public Dictionary<string, ChallengePlayerModel> _playersData;
        #pragma warning restore format

        [BsonIgnore] public DateTime activeTimeStamp { get => _activeTimeStamp; set { _activeTimeStamp = value; isDirty = true; } }
        [BsonIgnore] public long matchDuration { get => _matchDuration; set { _matchDuration = value; isDirty = true; } }
        [BsonIgnore] public bool isRanked { get => _isRanked; set { _isRanked = value; isDirty = true; } }
        [BsonIgnore] public string gameEndReason { get => _gameEndReason; set { _gameEndReason = value; isDirty = true; } }
        [BsonIgnore] public string winnerId { get => _winnerId; set { _winnerId = value; isDirty = true; } }
        [BsonIgnore] public Dictionary<string, ChallengePlayerModel> playersData { get => _playersData; set { _playersData = value; isDirty = true; } }

        public ChallengeData()
        {
            _playersData = new Dictionary<string, ChallengePlayerModel>();
        }

        public ChallengePlayerModel CreateChallengePlayerModel()
        {
            ChallengePlayerModel challengePlayerModel = new ChallengePlayerModel();
            challengePlayerModel._parent = this;
            return challengePlayerModel; 
        }

        public new virtual void PrepareCache()
        {
            foreach (var data in _playersData)
            {
                data.Value._parent = this;

                if (data.Value._winnerBonusRewards != null)
                    data.Value._winnerBonusRewards._parent = this;
            }
        }
    }

    public class ChallengeModelDocument
    {
        #pragma warning disable format
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]    public string _id;
        [BsonElement("ChallengeDataModel")]                     public ChallengeData _model;
        #pragma warning restore format
    }

    public class ChallengeDataModel
    {
        [BsonIgnore] const string COLLECTION = "challenges";
        [BsonIgnore] private SocialEdgeChallengeContext _socialEdgeChallenge;
        [BsonIgnore] private bool _isCached;
        [BsonIgnore] private string _id;
        [BsonIgnore] Dictionary<string, ChallengeData> _cache;
        [BsonIgnore] Dictionary<string, bool> _readOnly;
        [BsonElement("challenge")][BsonIgnore] public ChallengeData _challenge;
        
        public ChallengeData Challenge { get => _cache.ContainsKey(_id) ? _cache[_id] : null; }
        public string Id { get => _id; }

        public void ReadOnly() { _isCached = false; }
        public void ReadOnly(string id) { _readOnly.Add(id, true); }
        
        public ChallengeDataModel(SocialEdgeChallengeContext socialEdgeContext)
        {
            _socialEdgeChallenge = socialEdgeContext; 
            socialEdgeContext.SetDirtyBit(CacheChallengeDataSegments.CHALLENGE_MODEL);
            _isCached = true;
            _cache = new Dictionary<string, ChallengeData>();
            _readOnly = new Dictionary<string, bool>();
        }

        public ChallengeData Get(string modelId = null)
        {
            string id = modelId != null ? modelId : _id;
            if (id != null && _cache.ContainsKey(id)) 
                return _cache[id];

            var collection = SocialEdge.DataService.GetCollection<ChallengeModelDocument>(COLLECTION);
            var projection = Builders<ChallengeModelDocument>.Projection.Include(typeof(ChallengeDataModel).Name);
            var taskT = collection.FindOneById<ChallengeModelDocument>(id, projection);
            taskT.Wait();
            if (taskT.Result != null) 
            {
                _cache.Add(id, taskT.Result._model);
                _id = id;
                taskT.Result._model.PrepareCache();
            }
            SocialEdge.Log.LogInformation("Task fetch CHALLENGE_MODEL:" + id + " " + (taskT.Result != null ? "(success)" : "(null)"));
            return taskT.Result != null ? taskT.Result._model : null;
        }

        public string Create(ChallengeData challengeData = null)
        {
            var collection = SocialEdge.DataService.GetCollection<ChallengeModelDocument>(COLLECTION);
            ChallengeModelDocument modelDocument = new ChallengeModelDocument();
            challengeData.PrepareCache();
            modelDocument._model = challengeData;
            modelDocument._model._activeTimeStamp = DateTime.UtcNow;
            var taskT = collection.InsertOne(modelDocument);
            taskT.Wait(); 
            SocialEdge.Log.LogInformation("Task Insert CHALLENGE_MODEL");
            if (taskT.Result == true) _cache.Add(modelDocument._id, modelDocument._model != null ? modelDocument._model : new ChallengeData());           
            return taskT.Result == true ? _id = modelDocument._id : null;
        } 

        internal bool CacheWrite()
        {
            if (_isCached == false)
                return false;

            var collection = SocialEdge.DataService.GetCollection<ChallengeModelDocument>(COLLECTION);

            var tasks = new List<Task>();
            var i = _cache.GetEnumerator();
            while (i.MoveNext())
            {
                string id = i.Current.Key;
                if (_readOnly.ContainsKey(id))
                    continue;

                ChallengeData data = i.Current.Value;
                var taskT = collection.UpdateOneById<ChallengeData>(id, typeof(ChallengeDataModel).Name, data, true);
                tasks.Add(taskT);
            }

            Task.WaitAll(tasks.ToArray());

            if (tasks.Count > 0)
                SocialEdge.Log.LogInformation("Task flush CHALLENGE_MODEL");
                
            return true;       
        }
    }
}

