using System;
using SocialEdge.Server.Common.Utils;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
namespace SocialEdge.Server.DataService{

   public class Collection : ICollection
   {
       private readonly IMongoCollection<BsonDocument> _collection;
        public Collection(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<BsonDocument>(collectionName);  
        }

        public int Count{ get;}
        public async Task<BsonDocument> FindOneById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id",ObjectId.Parse(id));
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            return result;
        }

        public async Task<BsonDocument> FindOne<T>(string prop, T val)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(prop,val);
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            return result;
        }
        public async Task<BsonDocument> FindOne(FilterDefinition<BsonDocument> Filter)
        {
            var result = await _collection.Find(Filter).FirstOrDefaultAsync();
            return result;
        }
        public async Task<List<BsonDocument>> Find<T>(string prop,T val)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(prop,val);
            var result = await _collection.Find(filter).ToListAsync();
            return result;
        }
        public async Task<List<BsonDocument>> Find(FilterDefinition<BsonDocument> Filter)
        {
            var result = await _collection.Find(Filter).ToListAsync();
            return result;
        }
        public async Task<UpdateResult> UpdateOneById(string id, UpdateDefinition<BsonDocument> updateDefinition,
                                                    bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<BsonDocument>.Filter.Eq("_id",ObjectId.Parse(id));
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var opResult = await _collection.UpdateOneAsync(filter,updateDefinition,updateOptions);
            if(opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount,
                    UpsertedId = opResult.UpsertedId.ToString()
                };
            }

            return result;
        }
        public async Task<UpdateResult> UpdateOne<T>(string prop, T val, 
                                                UpdateDefinition<BsonDocument> updateDefinition, 
                                                bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<BsonDocument>.Filter.Eq(prop,val);
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var opResult = await _collection.UpdateOneAsync(filter,updateDefinition,updateOptions);
            if(opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount,
                    UpsertedId = opResult.UpsertedId.ToString()
                };
            }

            return result;
        }
        public async Task<UpdateResult> UpdateOne(FilterDefinition<BsonDocument> filter,
                                                UpdateDefinition<BsonDocument> updateDefinition,
                                                bool upsert=false)
        {
            UpdateResult result = null;
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var opResult = await _collection.UpdateOneAsync(filter,updateDefinition,updateOptions);
            if(opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount,
                    UpsertedId = opResult.UpsertedId.ToString()
                };
            }

            return result;
        }
        public async Task<UpdateResult> UpdateManyById(string id, UpdateDefinition<BsonDocument> updateDefinition,
                                                    bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<BsonDocument>.Filter.Eq("_id",ObjectId.Parse(id));
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };            
            var opResult = await _collection.UpdateManyAsync(filter,updateDefinition,updateOptions);
            if(opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount,
                    UpsertedId = opResult.UpsertedId.ToString()
                };
            }

            return result;
        }
        public async Task<UpdateResult> UpdateMany<T>(string prop, T val, 
                                                UpdateDefinition<BsonDocument> updateDefinition, 
                                                bool upsert=false)
        {
            UpdateResult result = null;
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var filter = Builders<BsonDocument>.Filter.Eq(prop,val);
            var opResult = await _collection.UpdateManyAsync(filter,updateDefinition,updateOptions);
            if(opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount,
                    UpsertedId = opResult.UpsertedId.ToString()
                };
            }

            return result;
        }
        public async Task<UpdateResult> UpdateMany(FilterDefinition<BsonDocument> filter,
                                                UpdateDefinition<BsonDocument> updateDefinition,
                                                bool upsert=false)
        {
            UpdateResult result = null;
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var opResult = await _collection.UpdateManyAsync(filter,updateDefinition,updateOptions);
            if(opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount,
                    UpsertedId = opResult.UpsertedId.ToString()
                };
            }

            return result;
        }
        public async Task<bool> RemoveOneById(string id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id",ObjectId.Parse(id));
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }
        public async Task<bool> RemoveOne<T>(string prop, T val)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(prop,val);
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }
        public async Task<bool> RemoveOne(FilterDefinition<BsonDocument> filter)
        {
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }
        public async Task<long> RemoveMany(FilterDefinition<BsonDocument> filter)
        {
            var result = await _collection.DeleteOneAsync(filter);
            if(result.IsAcknowledged)
            {
                return result.DeletedCount;
            }
            else
            {
                return 0;
            }
        }
        public async Task<long> RemoveMany<T>(string prop, T val)
        {

            var filter = Builders<BsonDocument>.Filter.Eq(prop,val);
            var result = await _collection.DeleteManyAsync(filter);
            if(result.IsAcknowledged)
            {
                return result.DeletedCount;
            }
            else
            {
                return 0;
            }
        }
        public async Task<bool> InsertOne(BsonDocument document)
        {
            try
            {
                await _collection.InsertOneAsync(document);
                return true;
            }
            catch(Exception e)
            {
                throw new Exception("InsertOne(BsonDocument) failed due to "+ e.InnerException);
            }
        }
        public async Task<bool> InsertMany(List<BsonDocument> documents)
        {
            try
            {
                await _collection.InsertManyAsync(documents);
                return true;
            }
            catch(Exception e)
            {
                throw new Exception("InsertMany(BsonDocument) failed due to "+ e.InnerException);
            }
        }
        public Task<bool> Save(BsonDocument document)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> EnsureIndex(List<string> key)
        {
             var indexKeysDefine= Builders<BsonDocument>.IndexKeys.Ascending(indexKey => key);
            CreateIndexModel<BsonDocument> model;
            model = new CreateIndexModel<BsonDocument>(key);
            // Create the unique index on the field 'title'
            var options = new CreateIndexOptions { Unique = true };
            await _collection.Indexes.CreateOneAsync(model);
        }

   }
    
}