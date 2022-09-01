/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace SocialEdgeSDK.Server.DataService{

   public class Collection<T> : ICollection<T>
   {
        private readonly IMongoCollection<T> _collection;

        public Collection(IMongoDatabase database, string collectionName)
        {
            _collection = database.GetCollection<T>(collectionName);  
        }

        public long EstimatedDocumentCount{ get => GetEstimatedDocs();}
        public long DocumentCount{ get => GetCount();}

        private long GetCount()
        {
            var filter = Builders<BsonDocument>.Filter;
            return _collection.CountDocuments(new BsonDocument());
        }

        public async Task<long> Count(FilterDefinition<T> filter)
        {
            return await _collection.CountDocumentsAsync(filter);
        }

        private long GetEstimatedDocs()
        {
            var filter = Builders<BsonDocument>.Filter;
            return _collection.EstimatedDocumentCount();
        }
        public async Task<U> FindOneById<U>(string id, ProjectionDefinition<T> projection)
        {
            var filter = Builders<T>.Filter.Eq("_id",ObjectId.Parse(id));
            var result = await _collection.Find<T>(filter).Project<U>(projection).FirstOrDefaultAsync<U>();
            
            return result;
        }

        public async Task<T> FindOneById(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id",ObjectId.Parse(id));
            var result = await _collection.Find(filter).FirstOrDefaultAsync();
            
            return result;
        }

        public async Task<T> FindOne<S>(string prop, S val)
        {
            var filter = Builders<T>.Filter.Eq(prop, val);
            var result = await _collection.Find(filter).FirstOrDefaultAsync<T>();
            return result;
        }
        public async Task<T> FindOne<S>(string prop, S val, ProjectionDefinition<T> projection)        
        {
            var filter = Builders<T>.Filter.Eq(prop,val);
            var result = await _collection.Find(filter).Project<T>(projection).FirstOrDefaultAsync<T>();
            return result;            
        }
        public async Task<T> FindOne(FilterDefinition<T> Filter)
        {
            var result = await _collection.Find(Filter).FirstOrDefaultAsync<T>();
            return result;
        }
        public async Task<P> FindOne<P>(FilterDefinition<T> Filter, 
                                                ProjectionDefinition<T> projection)
        {
            var result = await _collection.Find(Filter).Project<P>(projection).FirstOrDefaultAsync<P>();
            return result;
        }        
        public async Task<List<T>> Find<S>(string prop,S val)
        {
            var filter = Builders<T>.Filter.Eq(prop,val);
            var result = await _collection.Find(filter).ToListAsync<T>();
            return result;
        }
        public async Task<List<T>> Find<S>(string prop, S val, ProjectionDefinition<T> projection)
        {
            var filter = Builders<T>.Filter.Eq(prop,val);
            var result = await _collection.Find(filter).Project<T>(projection).ToListAsync<T>();
            return result;
        }        
        public async Task<List<T>> Find(FilterDefinition<T> Filter)
        {
            var result = await _collection.Find(Filter).ToListAsync<T>();
            return result;
        }
        public async Task<List<T>> Find(FilterDefinition<T> Filter,
                                                    ProjectionDefinition<T> projection)
        {
            var result = await _collection.Find(Filter).Project<T>(projection).ToListAsync<T>();
            return result;
        }        

        public async Task<UpdateResult> ReplaceOneById(string id, T val, bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
            var updateOptions = new ReplaceOptions
            {
                IsUpsert = upsert,
                BypassDocumentValidation = true
            };

            var opResult = await _collection.ReplaceOneAsync(filter, val, updateOptions);
            if (opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount
                };
            }
            return result;
        }

        public async Task<UpdateResult> ReplaceOneById(ObjectId id, T val, bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<T>.Filter.Eq("_id", id);
            var updateOptions = new ReplaceOptions
            {
                IsUpsert = upsert,
                BypassDocumentValidation = true
            };

            var opResult = await _collection.ReplaceOneAsync(filter, val, updateOptions);
            if (opResult.IsAcknowledged)
            {
                result = new UpdateResult
                {
                    MatchCount = opResult.MatchedCount,
                    ModifiedCount = opResult.ModifiedCount
                };
            }
            return result;
        }

        public async Task<T> IncAll(string prop, int incBy, bool upsert = false)
        {
            var filter = Builders<T>.Filter.Empty;
            var updates = Builders<T>.Update.Inc(prop, incBy);
            var updateOptions = new FindOneAndUpdateOptions<T, T>();
            updateOptions.IsUpsert = true;
            updateOptions.ReturnDocument = ReturnDocument.After;
            return await _collection.FindOneAndUpdateAsync<T>(filter, updates, updateOptions);
        }

        public async Task<UpdateResult> UpdateOneById<S>(string id, string prop, S val, bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<T>.Filter.Eq("_id",ObjectId.Parse(id));
            var updateDefinition = Builders<T>.Update.Set(prop,val);
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
                    UpsertedId = opResult.UpsertedId != null ? opResult.UpsertedId.ToString() : null
                };
            }

            return result;
        }

        public async Task<UpdateResult> UpdateOneById(string id, UpdateDefinition<T> updateDefinition,
                                                    bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<T>.Filter.Eq("_id",ObjectId.Parse(id));
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
                    UpsertedId = opResult.UpsertedId != null ? opResult.UpsertedId.ToString() : null
                };
            }

            return result;
        }
        public async Task<UpdateResult> UpdateOne<S,U>(string filterProp, S filterVal, 
                                        string updateProp, U updateVal,
                                        bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<T>.Filter.Eq(filterProp,filterVal);
            var updateDefinition = Builders<T>.Update.Set(updateProp,updateVal);
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

        public async Task<UpdateResult> UpdateOne<S>(string filterProp, S filterVal, 
                                                UpdateDefinition<T> updateDefinition, 
                                                bool upsert=false)
        {
            UpdateResult result = null;
            var filter = Builders<T>.Filter.Eq(filterProp,filterVal);
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

        public async Task<UpdateResult> UpdateOne(FilterDefinition<T> filter,
                                                UpdateDefinition<T> updateDefinition,
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

        public async Task<UpdateResult> UpdateMany<S,U>(string filterProp, S filterVal, string updateProp, U updateVal, bool upsert=false)
        {
            UpdateResult result = null;
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var filter = Builders<T>.Filter.Eq(filterProp,filterVal);
            var updateDefinition = Builders<T>.Update.Set(updateProp,updateVal);
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
        public async Task<UpdateResult> UpdateMany<S>(string prop, S val, 
                                                UpdateDefinition<T> updateDefinition, 
                                                bool upsert=false)
        {
            UpdateResult result = null;
            var updateOptions = new UpdateOptions
            {
                IsUpsert = upsert
            };
            var filter = Builders<T>.Filter.Eq(prop,val);
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
        public async Task<UpdateResult> UpdateMany(FilterDefinition<T> filter,
                                                UpdateDefinition<T> updateDefinition,
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
            var filter = Builders<T>.Filter.Eq("_id",ObjectId.Parse(id));
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }
        public async Task<bool> RemoveOne<S>(string prop, S val)
        {
            var filter = Builders<T>.Filter.Eq(prop,val);
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }
        public async Task<bool> RemoveOne(FilterDefinition<T> filter)
        {
            var result = await _collection.DeleteOneAsync(filter);
            return result.IsAcknowledged;
        }
        public async Task<long> RemoveMany(FilterDefinition<T> filter)
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
        public async Task<long> RemoveMany<S>(string prop, S val)
        {

            var filter = Builders<T>.Filter.Eq(prop,val);
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
        public async Task<bool> InsertOne(T document)
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
        public async Task<bool> InsertMany(List<T> documents)
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
        public Task<bool> Save(T document)
        {
            throw new NotImplementedException();
        }
        public async Task<string> CreateIndex(string key, int direction,  bool unique,
                                             string name, TimeSpan? TTL)
        {

            CreateIndexModel<T> indexModel;
            var indexKey = CreateIndexKey(key,direction);
            var options = CreateIndexOptions(name,TTL,unique);

            if(options==null)
            {
                indexModel = new CreateIndexModel<T>(indexKey);    
            }
            else
            {
                indexModel = new CreateIndexModel<T>(indexKey,options);
            }
            var result = await _collection.Indexes.CreateOneAsync(indexModel);
            return result;            
        }

        public async Task<string> CreateIndex(string key1,string key2, 
                                            int direction1=1, int direction2=1,
                                            bool unique=false,string name=null, 
                                            TimeSpan? TTL=null)
        {
            CreateIndexModel<T> indexModel;
            var indexKey1 = CreateIndexKey(key1,direction1);
            var indexKey2 = CreateIndexKey(key2,direction2);

            var indexDefinition = Builders<T>.IndexKeys.Combine(indexKey1,indexKey2);


            var options = CreateIndexOptions(name,TTL,unique);

            if(options==null)
            {
                indexModel = new CreateIndexModel<T>(indexDefinition);    
            }
            else
            {
                indexModel = new CreateIndexModel<T>(indexDefinition,options);
            }
            var result = await _collection.Indexes.CreateOneAsync(indexModel);
            return result;            
        }


        private IndexKeysDefinition<T> CreateIndexKey(string key, int direction)
        {
            var indexBuilder = Builders<T>.IndexKeys;
            IndexKeysDefinition<T> indexKey;
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