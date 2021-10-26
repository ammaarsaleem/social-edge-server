using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace SocialEdge.Server.DataService
{
    public interface ICollection
    {
        int Count{ get;}
        Task<BsonDocument> FindOneById(string id);
        Task<BsonDocument> FindOne<T>(string prop, T val);
        Task<BsonDocument> FindOne(FilterDefinition<BsonDocument> Filter);
        Task<List<BsonDocument>> Find<T>(string prop,T val);
        Task<List<BsonDocument>> Find(FilterDefinition<BsonDocument> Filter);
        Task<UpdateResult> UpdateOneById(string id, UpdateDefinition<BsonDocument> UpdateDefinition, bool upsert=false);
        Task<UpdateResult> UpdateOne<T>(string prop, T val, UpdateDefinition<BsonDocument> UpdateDefinition, bool upsert=false);
        Task<UpdateResult> UpdateOne(FilterDefinition<BsonDocument> Filter,UpdateDefinition<BsonDocument> UpdateDefinition, bool upsert=false);
        Task<UpdateResult> UpdateManyById(string id, UpdateDefinition<BsonDocument> UpdateDefinition, bool upsert=false);
        Task<UpdateResult> UpdateMany<T>(string id,T val, UpdateDefinition<BsonDocument> UpdateDefinition, bool upsert=false);
        Task<UpdateResult> UpdateMany(FilterDefinition<BsonDocument> Filter,UpdateDefinition<BsonDocument> UpdateDefinition, bool upsert=false);
        Task<bool> RemoveOneById(string id);
        Task<bool> RemoveOne<T>(string prop, T val);
        Task<bool> RemoveOne(FilterDefinition<BsonDocument> Filter);
        Task<long> RemoveMany<T>(string prop, T val);
        Task<long> RemoveMany(FilterDefinition<BsonDocument> Filter);
        Task<bool> InsertOne(BsonDocument document);
        Task<bool> InsertMany(List<BsonDocument> document);
        Task<bool> Save(BsonDocument document);
        Task<bool> EnsureIndex();




    }
}