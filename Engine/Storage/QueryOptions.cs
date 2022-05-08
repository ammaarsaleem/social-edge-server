/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using MongoDB.Driver;
using MongoDB.Bson;
namespace SocialEdgeSDK.Server.DataService
{
    public static class QueryOptions
    {
        public static FilterDefinition<BsonDocument> Filter;
        public static UpdateDefinition<BsonDocument> UpdateDefinition;
        
    }
}