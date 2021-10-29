using System.Threading.Tasks;
using StackExchange.Redis;

namespace SocialEdge.Server.DataService
{
    public interface ICache
    {
        Task<bool> Set(string key, string value);
        Task<string> Get(string key);
        Task AddPlayerToRoom(string roomId, string playerId, string playerName="");
        Task<bool> DeleteRoom(string roomId);
        Task<HashEntry[]> GetRoom(string roomId);
        Task RemovePlayerFromRoom(string roomId, string playerId, string playerName="");
    }
}