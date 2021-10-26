using MongoDB.Driver;
using MongoDB.Bson;
namespace SocialEdge.Server.DataService
{
    public static class QueryOptions
    {
        public static FilterDefinition<BsonDocument> Filter;
        public static UpdateDefinition<BsonDocument> UpdateDefinition;
        
    }
}