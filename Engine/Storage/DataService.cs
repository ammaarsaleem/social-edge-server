/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using SocialEdgeSDK.Server.Context;
using MongoDB.Driver;
using StackExchange.Redis;
using Azure.Storage.Blobs;

namespace SocialEdgeSDK.Server.DataService
{
    public class DataService : IDataService
    {
        #region Mongo members
        private readonly MongoClient _dbClient;
        private readonly IMongoDatabase _database;
        #endregion

        #region Redis members
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _cacheDb;
        #endregion

        public readonly BlobContainerClient _blobContainerClient;

        //ICollection _collection;
        ICache _cache;
        IBlobStorage _blobStorage;

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        public DataService(MongoClient mongoClient, ConnectionMultiplexer redisConn, BlobContainerClient blobConn)
        {
            _dbClient = mongoClient;
            string dbName = ConfigConstants.DATABASE;
            _database = _dbClient.GetDatabase(dbName);

            _redis = redisConn;
            _cacheDb = _redis.GetDatabase();
            _blobContainerClient = blobConn;
        }

        public ICollection<T> GetCollection<T>(string name)
        {
            ICollection<T> _collection = new Collection<T>(_database,name);
            if(_collection!=null)
            {
                return _collection;
            }
            return null;
        }

        public IBlobStorage GetBlobStorage()
        {
            _blobStorage = new BlobStorage(_blobContainerClient);
            return _blobStorage;
        }

        public ICache GetCache()
        {
            _cache = new Cache(_cacheDb);
            return _cache;
        }
    }
}