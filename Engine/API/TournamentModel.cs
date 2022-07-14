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
        [BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]    public DateTime expireAt;
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

        [BsonElement("tournament")][BsonIgnoreIfNull] public TournamentData _tournament;
        
        public TournamentData Tournament { get => _tournament != null && _tournament.isCached ? _tournament : _tournament = Get<TournamentData>(nameof(_tournament)); }
        public string Id { get => String.IsNullOrEmpty(_id) ? Insert() : _id; }

        public void ReadOnly() { _isCached = false; }
        
        public TournamentDataModel(SocialEdgeTournamentContext socialEdgeTournament)
        {
            _socialEdgeTournament = socialEdgeTournament; 
            socialEdgeTournament.SetDirtyBit(CacheTournamentDataSegments.TOURNAMENT_MODEL);
            _isCached = true;
        } 

        private T Get<T>(string fieldName)
        {
            if (string.IsNullOrEmpty(_id))
                return (T)Activator.CreateInstance(typeof(T));

            string elemName = fieldName.Substring(1);
            var collection = SocialEdge.DataService.GetCollection<TournamentModelDocument>(TOURNAMENT_MODEL_COLLECTION);
            var projection = Builders<TournamentModelDocument>.Projection.Include(typeof(TournamentDataModel).Name + "." + elemName);
            var taskT = collection.FindOneById<TournamentModelDocument>(_id, projection);
            taskT.Wait();
            _id = taskT.Result != null ? taskT.Result._id : null;
            return taskT.Result != null ? (T)taskT.Result._model.GetType().GetField(fieldName).GetValue(taskT.Result._model) : (T)Activator.CreateInstance(typeof(T));
        }

        private string Insert()
        {
            var collection = SocialEdge.DataService.GetCollection<TournamentModelDocument>(TOURNAMENT_MODEL_COLLECTION);

            if (String.IsNullOrEmpty(_id))
            {
                TournamentModelDocument tournamentModelDocument = new TournamentModelDocument() { _model = _tournament };
                var taskT = collection.InsertOne(tournamentModelDocument);
                taskT.Wait();  
                _id = tournamentModelDocument._id;           
            }

            return _id;
        } 

        internal bool CacheWriteTournamentModel()
        {
            if (_isCached == false || string.IsNullOrEmpty(_id))
                return false;

            var collection = SocialEdge.DataService.GetCollection<TournamentModelDocument>(TOURNAMENT_MODEL_COLLECTION);
            var taskT = collection.UpdateOneById<TournamentData>(_id, typeof(TournamentDataModel).Name, _tournament, true);
            taskT.Wait(); 
            SocialEdge.Log.LogInformation("Task flush TOURNAMENT_MODEL");
            return taskT.Result.ModifiedCount != 0;       
        }
    }
}

