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
    public interface ICache
    {
        Task<bool> Set(RedisKey key, RedisValue value);
        Task<RedisValue> Get(RedisKey key);
        Task AddPlayerToRoom(string roomId, string playerId, string playerName="");
        Task<bool> DeleteRoom(string roomId);
        Task<HashEntry[]> GetRoom(string roomId);
    }
}