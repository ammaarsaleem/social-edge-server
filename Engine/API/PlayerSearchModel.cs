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
    public class PlayerPublicProfile : DataModelBase
    {
        #pragma warning disable format
        [BsonElement("playerMiniProfile")]                                                  public PlayerMiniProfileData _playerMiniProfile;
        [BsonElement("displayName")][BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string _displayName;
        [BsonElement("location")][BsonRepresentation(MongoDB.Bson.BsonType.String)]         public string _location;
        [BsonElement("created")][BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]        public DateTime _created;
        [BsonElement("lastLogin")][BsonRepresentation(MongoDB.Bson.BsonType.String)]        public DateTime _lastLogin;

        [BsonElement("eloScore")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _eloScore;
        [BsonElement("trophies")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _trophies;
        [BsonElement("trophies2")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]         public int _trophies2;
        [BsonElement("earnings")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _earnings;
        [BsonElement("gamesWon")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int _gamesWon;
        [BsonElement("gamesLost")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]         public int _gamesLost;
        [BsonElement("gamesDrawn")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]        public int _gamesDrawn;
        [BsonElement("activeInventory")]                                                    public List<PlayerInventoryItem> _activeInventory;
        #pragma warning restore format

        [BsonIgnore] public PlayerMiniProfileData playerMiniProfile { get => _playerMiniProfile; set { _playerMiniProfile = value; isDirty = true; } }
        [BsonIgnore] public string displayName { get => _displayName; set { _displayName = value; isDirty = true; } }
        [BsonIgnore] public string location { get => _location; set { _location = value; isDirty = true; } }
        [BsonIgnore] public DateTime created { get => _created; set { _created = value; isDirty = true; } }
        [BsonIgnore] public DateTime lastLogin { get => _lastLogin; set { _lastLogin = value; isDirty = true; } }

        [BsonIgnore] public int eloScore { get => _eloScore; set { _eloScore = value; isDirty = true; } }
        [BsonIgnore] public int trophies { get => _trophies; set { _trophies = value; isDirty = true; } }
        [BsonIgnore] public int trophies2 { get => _trophies2; set { _trophies2 = value; isDirty = true; } }
        [BsonIgnore] public int earnings { get => _earnings; set { _earnings = value; isDirty = true; } }
        [BsonIgnore] public int gamesWon { get => _gamesWon; set { _gamesWon = value; isDirty = true; } }
        [BsonIgnore] public int gamesLost { get => _gamesLost; set { _gamesLost = value; isDirty = true; } }
        [BsonIgnore] public int gamesDrawn { get => _gamesDrawn; set { _gamesDrawn = value; isDirty = true; } }
        [BsonIgnore] public List<PlayerInventoryItem> activeInventory { get => _activeInventory; set { _activeInventory = value; isDirty = true; } }
    }

    public class PlayerSearchData : DataModelBase
    {
        #pragma warning disable format
        [BsonElement("activeTimeStamp")][BsonRepresentation(MongoDB.Bson.BsonType.DateTime)]    public DateTime _activeTimeStamp;
        [BsonElement("tag")][BsonRepresentation(MongoDB.Bson.BsonType.String)]                  public string _tag;
        [BsonElement("publicProfile")]                                                          public PlayerPublicProfile _publicProfile;
        #pragma warning restore format
        
        [BsonIgnore] public DateTime activeTimeStamp { get => _activeTimeStamp; set { _activeTimeStamp = value; isDirty = true; } }
        [BsonIgnore] public string tag { get => _tag; set { _tag = value; isDirty = true; } }
        [BsonIgnore] public PlayerPublicProfile publicProfile { get => _publicProfile; set { _publicProfile = value; isDirty = true; } }
    }

    public class PlayerSearchDataModelDocument
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] public string _id;
        [BsonElement("PlayerSearchData")] public PlayerSearchData _model;
    }

    public static class PlayerSearch
    {
        [BsonIgnore] const string COLLECTION = "playerSearch";

        public static void Register(SocialEdgePlayerContext socialEdgePlayer)
        {
            string dbId =  socialEdgePlayer.PlayerDBId;
            string tag = socialEdgePlayer.PlayerModel.Info.tag;
            PlayerPublicProfile publicProfile = socialEdgePlayer.PublicProfile;

            PlayerSearchData model = new PlayerSearchData();
            model.activeTimeStamp = DateTime.UtcNow;
            model.tag = tag;
            model.publicProfile = publicProfile;

            var collection = SocialEdge.DataService.GetCollection<PlayerSearchDataModelDocument>(COLLECTION);
            var taskT = collection.UpdateOneById<PlayerSearchData>(dbId, "PlayerSearchData", model, true);
            SocialEdge.Log.LogInformation("Task flush PLAYER_SEARCH_MODEL");
        }

        public static List<PlayerSearchDataModelDocument> Search(string matchString, int skip, int size, List<string> excludeIds)
        {
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<PlayerSearchDataModelDocument>(COLLECTION);
            FieldDefinition<PlayerSearchDataModelDocument> field = "PlayerSearchData.publicProfile" + ".displayName";
            var filter = Builders<PlayerSearchDataModelDocument>.Filter.Regex(field, new BsonRegularExpression( "^"+matchString+".*", "i"));
            filter = filter | Builders<PlayerSearchDataModelDocument>.Filter.Eq("PlayerSearchData" + ".tag", matchString);
            filter = filter & Builders<PlayerSearchDataModelDocument>.Filter.Nin("_id", excludeIds);
            return collection.Find(filter).Skip(skip).Limit(size).ToList();
        }
    }
}

