/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SocialEdgeSDK.Server.DataService 
{
    public class Cache : ICache
    {
        private readonly IDatabase _cacheDb;

        public Cache(IDatabase cacheDb)
        {
            _cacheDb = cacheDb;
        }
        public async Task<bool> Set(string key, string value)
        {
            try
            {
                return await _cacheDb.StringSetAsync(key, value);
            }
            catch (Exception e)
            {
                // The object may be disposed
                return false;
            }
        }
        public async Task<string> Get(string key)
        {
            try
            {
                var result = await _cacheDb.StringGetAsync(key);
                return result;
            }   
            catch(Exception e)
            {
                // The object may be disposed
                return null;
            }         
        }

        public async Task AddPlayerToRoom(string roomId, string playerId, string playerName = "")
        {
            string hashKey = GetRoomKey(roomId);
            if(!string.IsNullOrEmpty(playerId))
            {
                var player = CreateCachePlayer(playerId,playerName);
                await _cacheDb.HashSetAsync(hashKey,player);
            }
        }

        public async Task RemovePlayerFromRoom(string roomId, string playerId, string playerName="")
        {
            string hashKey = GetRoomKey(roomId);
            string userIdKey = GetUserIdKey(playerId);
            string nameKey = GetUserNameKey(playerId);

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
            string hashKey = GetRoomKey(roomId);
            bool result = await _cacheDb.KeyDeleteAsync(hashKey);
            return result;
        }

        public async Task<HashEntry[]> GetRoom(string roomId)
        {
            string hashKey = GetRoomKey(roomId);
            HashEntry[] result = await _cacheDb.HashGetAllAsync(hashKey);
            return result;
        }

        public HashEntry[] CreateCachePlayer(string id, string name)
        {
            string userKey = GetUserIdKey(id);
            string nameKey = GetUserNameKey(id);

            var player = new HashEntry[] {
            new HashEntry(userKey, id),
            new HashEntry(nameKey, name)
            };

            return player;
        }

        private string GetRoomKey(string roomId)
        {
            return "roomId::"+roomId;
        }

        private string GetUserIdKey(string id)
        {
            return "user::"+id;
        }

        private string GetUserNameKey(string id)
        {
            return "user::"+id+"::name";
        }

    }
}