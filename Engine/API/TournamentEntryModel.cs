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
    public class PlayerMiniProfileData : DataModelBase
    {
        [BsonElement("0")][BsonRepresentation(MongoDB.Bson.BsonType.String)]    public string _avatarId;
        [BsonElement("1")][BsonRepresentation(MongoDB.Bson.BsonType.String)]    public string _avatarBgColor;
        [BsonElement("2")][BsonRepresentation(MongoDB.Bson.BsonType.String)]    public string _uploadPicId;
        [BsonElement("3")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]     public int _eventGlow;
        [BsonElement("4")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]     public int _league;

        [BsonIgnore] public string AvatarId { get => _avatarId; set { _avatarId = value; isDirty = true; }}
        [BsonIgnore] public string AvatarBgColor { get => _avatarBgColor; set { _avatarBgColor = value;  isDirty = true; }}
        [BsonIgnore] public string UploadPicId { get => _uploadPicId; set { _uploadPicId = value;  isDirty = true; }}
        [BsonIgnore] public int EventGlow { get => _eventGlow; set { _eventGlow = value;  isDirty = true; }}
        [BsonIgnore] public int League { get => _league; set { _league = value;  isDirty = true; }}
    }

    public class TournamentEntryData : DataModelBase
    {
        [BsonElement("playerId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]         public string _playerId;
        [BsonElement("displayName")][BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string _displayName;
        [BsonElement("country")][BsonRepresentation(MongoDB.Bson.BsonType.String)]          public string _country;

        [BsonElement("eloScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _eloScore;
        [BsonElement("rnd")][BsonRepresentation(MongoDB.Bson.BsonType.Double)]              public double _rnd;
        [BsonElement("expireAt")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]          public long _expireAt;
        [BsonElement("score")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]             public int _score;
        [BsonElement("retentionDay")][BsonRepresentation(MongoDB.Bson.BsonType.String)]     public string _retentionDay;
        [BsonElement("tournamentMaxScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]public int _tournamentMaxScore;
        [BsonElement("tournamentSlot")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]    public int _tournamentSlot;
        [BsonElement("lastActiveTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]    public long _lastActiveTime;
        [BsonElement("joinTime")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]          public long _joinTime;
        [BsonElement("playerTimeZoneSlot")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]public int _playerTimeZoneSlot;
        [BsonElement("playerMiniProfile")]                                                  public PlayerMiniProfileData _playerMiniProfile;

        [BsonIgnore] public string playerId {get => _playerId; set {_playerId = value; isDirty = true;}}
        [BsonIgnore] public string displayName {get => _displayName; set {_displayName = value; isDirty = true;}}
        [BsonIgnore] public string country {get => _country; set {_country = value; isDirty = true;}}

        [BsonIgnore] public int eloScore {get => _eloScore; set {_eloScore = value; isDirty = true;}}
        [BsonIgnore] public double rnd {get => _rnd; set {_rnd = value; isDirty = true;}}
        [BsonIgnore] public long expireAt {get => _expireAt; set {_expireAt = value; isDirty = true;}}
        [BsonIgnore] public int score {get => _eloScore; set {_eloScore = value; isDirty = true;}}
        [BsonIgnore] public string retentionDay {get => _retentionDay; set {_retentionDay = value; isDirty = true;}}
        [BsonIgnore] public int tournamentMaxScore {get => _eloScore; set {_eloScore = value; isDirty = true;}}
        [BsonIgnore] public int tournamentSlot {get => _tournamentSlot; set {_tournamentSlot = value; isDirty = true;}}
        [BsonIgnore] public long lastActiveTime {get => _lastActiveTime; set {_lastActiveTime = value; isDirty = true;}}
        [BsonIgnore] public long joinTime {get => _joinTime; set {_joinTime = value; isDirty = true;}}
        [BsonIgnore] public int playerTimeZoneSlot {get => _playerTimeZoneSlot; set {_playerTimeZoneSlot = value; isDirty = true;}}

        [BsonIgnore] public PlayerMiniProfileData playerMiniProfile {get => _playerMiniProfile; set {_playerMiniProfile = value; isDirty = true;}}
        [BsonIgnore] public string playerMiniProfileAvatarId {get => _playerMiniProfile._avatarId; set {_playerMiniProfile._avatarId = value; isDirty = true;}}
        [BsonIgnore] public string playerMiniProfileAvtarBgColor {get => _playerMiniProfile._avatarBgColor; set {_playerMiniProfile._avatarBgColor = value; isDirty = true;}}
        [BsonIgnore] public string playerMiniProfileUploadPicId {get => _playerMiniProfile._uploadPicId; set {_playerMiniProfile._uploadPicId = value; isDirty = true;}}
        [BsonIgnore] public int playerMiniProfileEventGlow {get => _playerMiniProfile._eventGlow; set {_playerMiniProfile._eventGlow = value; isDirty = true;}}

        public TournamentEntryData()
        {
            _playerMiniProfile = new PlayerMiniProfileData();
        }
    }

    public class TournamentLeaderboardEntry
    {
        [BsonElement("playerId")][BsonRepresentation(MongoDB.Bson.BsonType.String)]     public string playerId;
        [BsonElement("score")][BsonRepresentation(MongoDB.Bson.BsonType.String)]        public int score;
        [BsonElement("displayName")][BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string displayName;
        [BsonElement("league")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]        public int league;
        [BsonElement("country")][BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string country;
        [BsonElement("playerMiniProfile")]                                              public PlayerMiniProfileData playerMiniProfile;

        public TournamentLeaderboardEntry(string playerId, int score, PlayerMiniProfileData playerMiniProfile, string country, string displayName)
        {
            this.playerId = playerId;
            this.score = score;
            this.displayName = displayName;
            this.country = country;
            this.playerMiniProfile = playerMiniProfile;
        }
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
        public TournamentEntryData TournamentEntry { get => _entry != null && _entry.isCached ? _entry : Get(); }

        public void ReadOnly() { _isCached = false; }
        
        public TournamentEntryModel(SocialEdgeTournamentContext socialEdgeTournament)
        {
            _isCached = true;
            _socialEdgeTournament = socialEdgeTournament;
            _socialEdgeTournament.SetDirtyBit(CacheTournamentDataSegments.TOURNAMENT_ENTRY);
        } 

        public TournamentEntryData Create(string id, string collectionName)
        {
            _id = id;
            _collectionName = collectionName;
            return _entry = new TournamentEntryData();
        }

        public TournamentEntryData Get(string id = null, string collectionName = null)
        {
            if (id == _id && collectionName == _collectionName && _entry != null)
                return _entry;

            var entryId = id != null ? id : _id;
            var tournamentCollectionName = collectionName != null ? collectionName : _collectionName;

            if (tournamentCollectionName == null || string.IsNullOrEmpty(id))
                return null;

            var collection = SocialEdge.DataService.GetCollection<TournamentEntryModelDocument>(tournamentCollectionName);
            var projection = Builders<TournamentEntryModelDocument>.Projection.Exclude("_id").Include(typeof(TournamentEntryData).Name);
            var taskT = collection.FindOneById<TournamentEntryData>(entryId, projection);
            taskT.Wait();
            if (taskT.Result != null) 
            {
                taskT.Result.isDirty = false;
                _id = entryId;
                _collectionName = tournamentCollectionName;
            }
            SocialEdge.Log.LogInformation("Task fetch TOURNAMENT_ENTRY:" + (taskT.Result != null ? "(success)" : "(null)"));
            return taskT.Result != null ? _entry = taskT.Result : null;
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

