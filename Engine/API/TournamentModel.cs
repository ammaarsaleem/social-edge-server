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
    public class TournamentData : DataModelBase
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string shortCode;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string name;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string type;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long startTime;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long joinedTime;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long lastUpdateTimeSeconds;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long secondsLeftNextUpdate;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long duration;
                                                                public Dictionary<string, List<TournamentReward>> rewards;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool concluded;
                                                                public List<string> entryIds;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int score;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long expireAt;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int tournamentCollectionIndex;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int tournamentSlot;
    }

    public class TournamentModelDocument
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]  public string _id;
        [BsonElement("TournamentDataModel")] public TournamentData _model;
    }

    public class TournamentDataModel
    {
        [BsonIgnore] const string TOURNAMENT_MODEL_COLLECTION = "tournamentModel";
        [BsonIgnore] private SocialEdgeTournamentContext _socialEdgeTournament;
        [BsonIgnore] private bool _isCached;
        [BsonIgnore] private string _id;
        [BsonIgnore] Dictionary<string, TournamentData> _cache;
        [BsonIgnore] Dictionary<string, bool> _readOnly;
        [BsonElement("tournament")][BsonIgnore] public TournamentData _tournament;
        
        public TournamentData Tournament { get => _cache.ContainsKey(_id) ? _cache[_id] : null; }
        public string Id { get => _id; }

        public void ReadOnly() { _isCached = false; }
        public void ReadOnly(string id) { _readOnly.Add(id, true); }
        
        public TournamentDataModel(SocialEdgeTournamentContext socialEdgeTournament)
        {
            _socialEdgeTournament = socialEdgeTournament; 
            socialEdgeTournament.SetDirtyBit(CacheTournamentDataSegments.TOURNAMENT_MODEL);
            _isCached = true;
            _cache = new Dictionary<string, TournamentData>();
            _readOnly = new Dictionary<string, bool>();
        }

        public TournamentData Get(string tournamentId = null)
        {
            string id = tournamentId != null ? tournamentId : _id;
            if (id != null && _cache.ContainsKey(id)) 
                return _cache[id];

            var collection = SocialEdge.DataService.GetCollection<TournamentModelDocument>(TOURNAMENT_MODEL_COLLECTION);
            var projection = Builders<TournamentModelDocument>.Projection.Include(typeof(TournamentDataModel).Name);
            var taskT = collection.FindOneById<TournamentModelDocument>(id, projection);
            taskT.Wait();
            if (taskT.Result != null) 
            {
                _cache.Add(id, taskT.Result._model);
                _id = id;
            }
            SocialEdge.Log.LogInformation("Task fetch TOURNAMENT_MODEL:" + id + " " + (taskT.Result != null ? "(success)" : "(null)"));
            return taskT.Result != null ? taskT.Result._model : null;
        }

        public string Create()
        {
            var collection = SocialEdge.DataService.GetCollection<TournamentModelDocument>(TOURNAMENT_MODEL_COLLECTION);
            TournamentModelDocument tournamentModelDocument = new TournamentModelDocument();
            var taskT = collection.InsertOne(tournamentModelDocument);
            taskT.Wait();  
            if (taskT.Result == true) _cache.Add(tournamentModelDocument._id, new TournamentData());           
            return taskT.Result == true ? _id = tournamentModelDocument._id : null;
        } 

        internal bool CacheWriteTournamentModel()
        {
            if (_isCached == false)
                return false;

            var collection = SocialEdge.DataService.GetCollection<TournamentModelDocument>(TOURNAMENT_MODEL_COLLECTION);

            var tasks = new List<Task>();
            var i = _cache.GetEnumerator();
            while (i.MoveNext())
            {
                string id = i.Current.Key;
                if (_readOnly.ContainsKey(id))
                    continue;

                TournamentData data = i.Current.Value;
                var taskT = collection.UpdateOneById<TournamentData>(id, typeof(TournamentDataModel).Name, data, true);
                tasks.Add(taskT);
            }

            Task.WaitAll(tasks.ToArray());

            SocialEdge.Log.LogInformation("Task flush TOURNAMENT_MODEL");
            return true;       
        }
    }
}

