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
        #pragma warning disable format
        [BsonElement("shortCode")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                public string _shortCode;
        [BsonElement("name")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                     public string _name;
        [BsonElement("type")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                     public string _type;
        [BsonElement("startTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]                 public long _startTime;
        [BsonElement("joinedTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]                public long _joinedTime;
        [BsonElement("lastUpdateTimeSeconds")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]     public long _lastUpdateTimeSeconds;
        [BsonElement("secondsLeftNextUpdate")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]     public long _secondsLeftNextUpdate;
        [BsonElement("duration")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]                  public long _duration;
        [BsonElement("rewards")]                                                                    public Dictionary<string, List<TournamentReward>> _rewards;
        [BsonElement("concluded")][BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]               public bool _concluded;
        [BsonElement("entryIds")]                                                                   public List<string> _entryIds;
        [BsonElement("score")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]                     public int _score;
        [BsonElement("expireAt")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]                  public long _expireAt;
        [BsonElement("tournamentCollectionIndex")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)] public int _tournamentCollectionIndex;
        [BsonElement("tournamentSlot")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]            public int _tournamentSlot;
        #pragma warning restore format
        
        [BsonIgnore] public string shortCode { get => _shortCode; set { _shortCode = value; isDirty = true; } }
        [BsonIgnore] public string name { get => _name; set { _name = value; isDirty = true; } }
        [BsonIgnore] public string type { get => _type; set { _type = value; isDirty = true; } }
        [BsonIgnore] public long startTime { get => _startTime; set { _startTime = value; isDirty = true; } }
        [BsonIgnore] public long joinedTime { get => _joinedTime; set { _joinedTime = value; isDirty = true; } }
        [BsonIgnore] public long lastUpdateTimeSeconds { get => _lastUpdateTimeSeconds; set { _lastUpdateTimeSeconds = value; isDirty = true; } }
        [BsonIgnore] public long secondsLeftNextUpdate { get => _secondsLeftNextUpdate; set { _secondsLeftNextUpdate = value; isDirty = true; } }
        [BsonIgnore] public long duration { get => _duration; set { _duration = value; isDirty = true; } }
        [BsonIgnore] public Dictionary<string, List<TournamentReward>> rewards { get => _rewards; set { _rewards = value; isDirty = true; } }
        [BsonIgnore] public bool concluded { get => _concluded; set { _concluded = value; isDirty = true; } }
        [BsonIgnore] public List<string> entryIds { get => _entryIds; set { _entryIds = value; isDirty = true; } }
        [BsonIgnore] public int score { get => _score; set { _score = value; isDirty = true; } }
        [BsonIgnore] public long expireAt { get => _expireAt; set { _expireAt = value; isDirty = true; } }
        [BsonIgnore] public int tournamentCollectionIndex { get => _tournamentCollectionIndex; set { _tournamentCollectionIndex = value; isDirty = true; } }
        [BsonIgnore] public int tournamentSlot { get => _tournamentSlot; set { _tournamentSlot = value; isDirty = true; } }

        public void AppendEntryIds(List<string> pool)
        {
            _entryIds.AddRange(pool);
            isDirty = true;
        }
    }

    public class TournamentModelDocument
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] public string _id;
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

        public TournamentData Tournament { get => Get(_id); }
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

            if (id == null)
                return null;

            var collection = SocialEdge.DataService.GetCollection<TournamentModelDocument>(TOURNAMENT_MODEL_COLLECTION);
            var projection = Builders<TournamentModelDocument>.Projection.Include(typeof(TournamentDataModel).Name);
            var taskT = collection.FindOneById<TournamentModelDocument>(id, projection);
            taskT.Wait();
            if (taskT.Result != null)
            {
                _cache.Add(id, taskT.Result._model);
                _id = id;
            }
            SocialEdge.Log.LogInformation("Task fetch TOURNAMENT_MODEL: " + id + " " + (taskT.Result != null ? "(success)" : "(null)"));
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
                if (data.isDirty == true)
                {
                    var taskT = collection.UpdateOneById<TournamentData>(id, typeof(TournamentDataModel).Name, data, true);
                    tasks.Add(taskT);
                    SocialEdge.Log.LogInformation("Task flush TOURNAMENT_MODEL: " + id);
                }
            }

            if (tasks.Count > 0)
                Task.WaitAll(tasks.ToArray());

            return true;
        }
    }
}

