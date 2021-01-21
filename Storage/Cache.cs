using System;
using PlayFab;
using SocialEdge.Server.Common.Constants;
using System.Net.Http;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using SocialEdge.Server.Common.Utils;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using StackExchange.Redis;

namespace SocialEdge.Server.Cache 
{
    public class Cache : ICache
    {

        private readonly IMongoCollection<BsonDocument> _playerCollection;
        // private readonly ILogger _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _cacheDb;

        public Cache(ConnectionMultiplexer redis)
        {
            _redis = redis;
            _cacheDb = _redis.GetDatabase();
        }

        // private readonly IMongoCollection<Album> _albums;

        public async Task<bool> Set(RedisKey key, RedisValue value)

        {
            try
            {
                return await _cacheDb.StringSetAsync(key, value);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<RedisValue> Get(RedisKey key)
        {
            try
            {
                return await _cacheDb.StringGetAsync(key);
            }   
            catch(Exception e)
            {
                throw e;
            }         
        }

        public async Task AddPlayerToRoom(string roomId, string playerId, string playerName = "")
        {
            string hashKey = "roomId::"+roomId;
            if(!string.IsNullOrEmpty(playerId))
            {
                var player = CreateCachePlayer(playerId,playerName);
                await _cacheDb.HashSetAsync(hashKey,player);
            }
        }

        public async Task RemovePlayerFromRoom(string roomId, string playerId, string playerName)
        {
            string hashKey = "roomId::"+roomId;
            string userIdKey = "user::"+playerId;
            string nameKey = "user::"+playerName+"::name";

            if(!string.IsNullOrEmpty(playerId))
            {
                await _cacheDb.HashDeleteAsync(hashKey,new RedisValue[]
                {
                    userIdKey,
                    nameKey
                });
            }
        }

        public async Task<bool> DeleteRoom(string roomId)
        {
            string hashKey = "roomId::"+roomId;
            bool result = await _cacheDb.KeyDeleteAsync(hashKey);
            return result;
        }

        public async Task<HashEntry[]> GetRoom(string roomId)
        {
            string hashKey = "roomId::"+roomId;
            HashEntry[] result = await _cacheDb.HashGetAllAsync(hashKey);
            return result;
        }

        public HashEntry[] CreateCachePlayer(string id, string name)
        {
            string userKey = "user::"+id;
            string nameKey = "user::"+id+"::name";

            var player = new HashEntry[] {
            new HashEntry(userKey, id),
            new HashEntry(nameKey, name)
            };

            return player;
        }

    }
}