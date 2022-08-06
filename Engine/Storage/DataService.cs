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
        public readonly BlobServiceClient _blobServerClient;
        ICache _cache;

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        public DataService(MongoClient mongoClient, ConnectionMultiplexer redisConn, BlobServiceClient serviceClient)
        {
            _dbClient = mongoClient;
            _database = _dbClient.GetDatabase(ConfigConstants.DATABASE);
            _redis = redisConn;
            _cacheDb = _redis.GetDatabase();
            _blobServerClient = serviceClient;
        }

        public ICollection<T> GetCollection<T>(string name)
        {
            ICollection<T> _collection = new Collection<T>(_database, name);
            return _collection;
        }

        public IBlobStorage GetBlobStorage(string containerName)
        {
            BlobContainerClient containerClient = _blobServerClient.GetBlobContainerClient(containerName);
            return new BlobStorage(containerClient);
        }

        public BlobContainerClient GetContainerClient(string containerName)
        {
            return _blobServerClient.GetBlobContainerClient(containerName);
        }

        public ICache GetCache()
        {
            return _cache = new Cache(_cacheDb);
        }
    }
}