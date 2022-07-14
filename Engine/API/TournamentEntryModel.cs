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
    public class TournamentEntryData : DataModelBase
    {
        [BsonElement("eloScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _eloScore;
        [BsonElement("rnd")][BsonRepresentation(MongoDB.Bson.BsonType.Double)]              public double _rnd;
        [BsonElement("expireAt")][BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]       public DateTime _expireAt;
        [BsonElement("score")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]             public int _score;
        [BsonElement("retentionDay")][BsonRepresentation(MongoDB.Bson.BsonType.String)]     public string _retentionDay;
        [BsonElement("league")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]            public int _league;
        [BsonElement("tournamentMaxScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]public int _tournamentMaxScore;
        [BsonElement("tournamentSlot")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]    public int _tournamentSlot;
        [BsonElement("lastActiveTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]    public long _lastActiveTime;
        [BsonElement("joinTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]          public long _joinTime;
        [BsonElement("playerTimeZoneSlot")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]public int _playerTimeZoneSlot;

        [BsonIgnore] public int eloScore {get => _eloScore; set {_eloScore = value; isDirty = true;}}
        [BsonIgnore] public double rnd {get => _rnd; set {_rnd = value; isDirty = true;}}
        [BsonIgnore] public DateTime expireAt {get => _expireAt; set {_expireAt = value; isDirty = true;}}
        [BsonIgnore] public int score {get => _eloScore; set {_eloScore = value; isDirty = true;}}
        [BsonIgnore] public string retentionDay {get => _retentionDay; set {_retentionDay = value; isDirty = true;}}
        [BsonIgnore] public int league {get => _league; set {_league = value; isDirty = true;}}
        [BsonIgnore] public int tournamentMaxScore {get => _eloScore; set {_eloScore = value; isDirty = true;}}
        [BsonIgnore] public int tournamentSlot {get => _tournamentSlot; set {_tournamentSlot = value; isDirty = true;}}
        [BsonIgnore] public long lastActiveTime {get => _lastActiveTime; set {_lastActiveTime = value; isDirty = true;}}
        [BsonIgnore] public long joinTime {get => _joinTime; set {_joinTime = value; isDirty = true;}}
        [BsonIgnore] public int playerTimeZoneSlot {get => _playerTimeZoneSlot; set {_playerTimeZoneSlot = value; isDirty = true;}}
    }

    public class TournamentEntryModelDocument 
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]   public string _id;
        [BsonElement("TournamentEntryData")]                   public TournamentEntryData _model;
    }

    public class TournamentEntryModel
    {
        [BsonIgnore] private string _collectionName;
        [BsonIgnore] private SocialEdgeTournamentContext _socialEdgeTournament;
        [BsonIgnore] private bool _isCached;
        [BsonIgnore] private string _id;

        [BsonElement("TournamentEntryData")][BsonIgnoreIfNull]  public TournamentEntryData _entry;

        [BsonIgnore] public string CollectionName { get => _collectionName; set => _collectionName = value; }
        [BsonIgnore] public string DBId { get => _id; set => _id = value; }
        public TournamentEntryData TournamentEntry { get => _entry != null && _entry.isCached ? _entry : _entry = Get(); }

        public void ReadOnly() { _isCached = false; }
        
        public TournamentEntryModel(SocialEdgeTournamentContext socialEdgeTournament)
        {
            _isCached = true;
            _socialEdgeTournament = socialEdgeTournament;
            _socialEdgeTournament.SetDirtyBit(CacheTournamentDataSegments.TOURNAMENT_ENTRY);
        } 

        public TournamentEntryData Get()
        {
            if (_collectionName == null || string.IsNullOrEmpty(_id))
                return null;

            var collection = SocialEdge.DataService.GetCollection<TournamentEntryModelDocument>(_collectionName);
            var projection = Builders<TournamentEntryModelDocument>.Projection.Exclude("_id").Include(typeof(TournamentEntryData).Name);
            var taskT = collection.FindOneById<TournamentEntryData>(_id, projection);
            taskT.Wait();
            if (taskT.Result != null) taskT.Result.isDirty = false;
            return taskT.Result != null ? taskT.Result : new TournamentEntryData();
        }        

        internal bool CacheWrite()
        {
            if (_isCached == false || _entry.isDirty == false)
                return false;

            var collection = SocialEdge.DataService.GetCollection<TournamentEntryModelDocument>(_collectionName);
            var taskT = collection.UpdateOneById<TournamentEntryData>(_id, typeof(TournamentEntryData).Name, _entry, true);
            taskT.Wait(); 
            SocialEdge.Log.LogInformation("Task flush TOURNAMENT_ENTRY");
            return taskT.Result.ModifiedCount != 0;       
        }        
    }
}

