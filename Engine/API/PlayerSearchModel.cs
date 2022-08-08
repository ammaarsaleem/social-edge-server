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
    public class PlayerSearchData : DataModelBase
    {
        #pragma warning disable format
        [BsonElement("displayName")][BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string _displayName;
        [BsonElement("tag")][BsonRepresentation(MongoDB.Bson.BsonType.String)]              public string _tag;
        [BsonElement("activeTimeStamp")][BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long _activeTimeStamp;
        #pragma warning restore format
        
        [BsonIgnore] public string displayName { get => _displayName; set { _displayName = value; isDirty = true; } }
        [BsonIgnore] public string tag { get => _tag; set { _tag = value; isDirty = true; } }
        [BsonIgnore] public long activeTimeStamp { get => _activeTimeStamp; set { _activeTimeStamp = value; isDirty = true; } }
    }

    public class PlayerSearchDataModelDocument
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)] public string _id;
        [BsonElement("PlayerSearchDataModel")] public PlayerSearchData _model;
    }

    public static class PlayerSearch
    {
        [BsonIgnore] const string COLLECTION = "playerSearch";

        public static void Register(string dbId, string tag, string displayName)
        {
            PlayerSearchData model = new PlayerSearchData();
            model.tag = tag;
            model.displayName = displayName;
            model.activeTimeStamp = Utils.UTCNow();

            var collection = SocialEdge.DataService.GetCollection<PlayerSearchDataModelDocument>(COLLECTION);
            var taskT = collection.UpdateOneById<PlayerSearchData>(dbId, "PlayerSearchData", model, true);
            SocialEdge.Log.LogInformation("Task flush PLAYER_SEARCH_MODEL");
        }

        public static List<PlayerSearchDataModelDocument> Search(string matchString, int skip, int size)
        {
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<PlayerSearchDataModelDocument>(COLLECTION);
            FieldDefinition<PlayerSearchDataModelDocument> field = "PlayerSearchData" + ".displayName";
            var filter = Builders<PlayerSearchDataModelDocument>.Filter.Regex(field, new BsonRegularExpression( "^"+matchString+".*", "i"));
            return collection.Find(filter).Skip(skip).Limit(size).ToList();
        }

        public static PlayerSearchDataModelDocument SearchTag(string tag)
        {
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<PlayerSearchDataModelDocument>(COLLECTION);
            var filter = Builders<PlayerSearchDataModelDocument>.Filter.Eq("PlayerSearchData" + ".tag", tag);
            return collection.Find(filter).FirstOrDefault();
        }
    }
}

