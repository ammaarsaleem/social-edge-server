/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using SocialEdgeSDK.Server.Context;
using MongoDB.Driver;
namespace SocialEdgeSDK.Server.DataService
{
    public class DataService : IDataService
    {
        #region Mongo members
        private readonly MongoClient _dbClient;
        private readonly IMongoDatabase _database;
        #endregion

        #region Redis members
        //private readonly ConnectionMultiplexer _redis;
        //private readonly IDatabase _cacheDb;
        #endregion
        ICollection _collection;
        //ICache _cache;

        public DataService(MongoClient mongoClient)//, ConnectionMultiplexer redisConn)
        {
            _dbClient = mongoClient;
            string dbName = ConfigConstants.DATABASE;
            _database = _dbClient.GetDatabase(dbName);

            //_redis = redisConn;
            //_cacheDb = _redis.GetDatabase();
        }

        public ICollection GetCollection(string name)
        {
            _collection = new Collection(_database,name);
            if(_collection!=null)
            {
                return _collection;
            }
            return null;
        }

        //public ICache GetCache()
        //{
        //    _cache = new Cache(_cacheDb);
        //    return _cache;
        //}
    }
}