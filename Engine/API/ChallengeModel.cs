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
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Models
{
    public class ChallengeWinnerBonusRewardsData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int bonusCoinsFree1;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int bonusCoinsFree2;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int bonusCoinsFree3;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int bonusCoinsRV1;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int bonusCoinsRV2;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int bonusCoinsRV3;
    }

    public class ChallengePlayerModel
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long betValue;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool isEventMatch;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool powerMode;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string tournamentId;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool isBot;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string playerId;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string playerColor;

        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int eloScore;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int eloChange;
        [BsonRepresentation(MongoDB.Bson.BsonType.Double)]      public double coinsMultiplyer;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int tournamentScore;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool promoted;

        [BsonIgnore]                                            public ChallengeWinnerBonusRewardsData winnerBonusRewards;
        [BsonIgnore]                                            public long piggyBankReward;
    }

    public class ChallengeData : DataModelBase
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long matchDuration;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool isRanked;
        [BsonElement("player1Data")]                            public ChallengePlayerModel player1Data;
        [BsonElement("player2Data")]                            public ChallengePlayerModel player2Data;

         [BsonRepresentation(MongoDB.Bson.BsonType.String)]     public string gameEndReason; 
         [BsonRepresentation(MongoDB.Bson.BsonType.String)]     public string winnerId; 

    }

    public class ChallengeModelDocument
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]  public string _id;
        [BsonElement("ChallengeDataModel")] public ChallengeData _model;
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
            var projection = Builders<ChallengeModelDocument>.Projection.Include(typeof(ChallengeModelDocument).Name);
            var taskT = collection.FindOneById<ChallengeModelDocument>(id, projection);
            taskT.Wait();
            if (taskT.Result != null) 
            {
                _cache.Add(id, taskT.Result._model);
                _id = id;
            }
            SocialEdge.Log.LogInformation("Task fetch CHALLENGE_MODEL:" + id + " " + (taskT.Result != null ? "(success)" : "(null)"));
            return taskT.Result != null ? taskT.Result._model : null;
        }

        public string Create(ChallengeData challengeData = null)
        {
            var collection = SocialEdge.DataService.GetCollection<ChallengeModelDocument>(COLLECTION);
            ChallengeModelDocument modelDocument = new ChallengeModelDocument();
            modelDocument._model = challengeData;
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

            SocialEdge.Log.LogInformation("Task flush CHALLENGE_MODEL");
            return true;       
        }
    }
}

