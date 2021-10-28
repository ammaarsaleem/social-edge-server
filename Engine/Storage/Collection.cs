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

        public long EstimatedDocumentCount{ get => GetEstimatedDocs();}
        public long DocumentCount{ get => GetCount();}
        private long GetCount()
        {
            var filter = Builders<BsonDocument>.Filter;
            return _collection.CountDocuments(new BsonDocument());
        }

        private long GetEstimatedDocs()
        {
            var filter = Builders<BsonDocument>.Filter;
            return _collection.EstimatedDocumentCount();
        }
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

        public async Task<UpdateResult> UpdateOneById<T>(string id, string prop, T val, bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<BsonDocument>.Filter.Eq("_id",ObjectId.Parse(id));
            var updateDefinition = Builders<BsonDocument>.Update.Set(prop,val);
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
        public async Task<UpdateResult> UpdateOne<T,V>(string filterProp, T filterVal, 
                                        string updateProp, V updateVal,
                                        bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<BsonDocument>.Filter.Eq(filterProp,filterVal);
            var updateDefinition = Builders<BsonDocument>.Update.Set(updateProp,updateVal);
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
        public async Task<UpdateResult> UpdateOne<T>(string filterProp, T filterVal, 
                                                UpdateDefinition<BsonDocument> updateDefinition, 
                                                bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<BsonDocument>.Filter.Eq(filterProp,filterVal);
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

        public async Task<UpdateResult> UpdateMany<T,V>(string filterProp, T filterVal, string updateProp, V updateVal, bool upsert=false)
        {
            UpdateResult result = null;
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var filter = Builders<BsonDocument>.Filter.Eq(filterProp,filterVal);
            var updateDefinition = Builders<BsonDocument>.Update.Set(updateProp,updateVal);
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
        public async Task<string> CreateIndex(string key, int direction,  bool unique,
                                             string name, TimeSpan? TTL)
        {

            CreateIndexModel<BsonDocument> indexModel;
            var indexKey = CreateIndexKey(key,direction);
            var options = CreateIndexOptions(name,TTL,unique);

            if(options==null)
            {
                indexModel = new CreateIndexModel<BsonDocument>(indexKey);    
            }
            else
            {
                indexModel = new CreateIndexModel<BsonDocument>(indexKey,options);
            }
            var result = await _collection.Indexes.CreateOneAsync(indexModel);
            return result;            
        }

        public async Task<string> CreateIndex(string key1,string key2, 
                                            int direction1=1, int direction2=1,
                                            bool unique=false,string name=null, 
                                            TimeSpan? TTL=null)
        {
            CreateIndexModel<BsonDocument> indexModel;
            var indexKey1 = CreateIndexKey(key1,direction1);
            var indexKey2 = CreateIndexKey(key2,direction2);

            var indexDefinition = Builders<BsonDocument>.IndexKeys.Combine(indexKey1,indexKey2);


            var options = CreateIndexOptions(name,TTL,unique);

            if(options==null)
            {
                indexModel = new CreateIndexModel<BsonDocument>(indexDefinition);    
            }
            else
            {
                indexModel = new CreateIndexModel<BsonDocument>(indexDefinition,options);
            }
            var result = await _collection.Indexes.CreateOneAsync(indexModel);
            return result;            
        }


        private IndexKeysDefinition<BsonDocument> CreateIndexKey(string key, int direction)
        {
            var indexBuilder = Builders<BsonDocument>.IndexKeys;
            IndexKeysDefinition<BsonDocument> indexKey;
            if(direction<0)
            {
                indexKey= indexBuilder.Descending(key);
            }
            else
            {
                indexKey = indexBuilder.Ascending(key);
            }

            return indexKey;
        }


        private CreateIndexOptions CreateIndexOptions(string name, TimeSpan? TTL, bool unique=false)
        {
            CreateIndexOptions options = null;
            if(string.IsNullOrEmpty(name) && TTL==null && unique==false)
            {
                options = null;
            }

            else if(!string.IsNullOrEmpty(name) && TTL!=null)
            {
                options = new CreateIndexOptions
                {
                    Name = name,
                    ExpireAfter = TTL,
                    Unique = unique
                };
            }

            else if (!string.IsNullOrEmpty(name))
            {
                options = new CreateIndexOptions
                {
                    Name = name,
                    Unique = unique
                };
            }

            else if(TTL!=null)
            {
                options = new CreateIndexOptions
                {
                    ExpireAfter = TTL,
                    Unique = unique
                };
            }
            else if(unique==true)
            {
                options = new CreateIndexOptions
                {
                    Unique = unique
                };
            }

            return options;
        }

   }
    
}