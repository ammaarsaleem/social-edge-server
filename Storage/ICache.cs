using System.Threading.Tasks;
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
        Task RemovePlayerFromRoom(string roomId, string playerId, string playerName="");
    }
}