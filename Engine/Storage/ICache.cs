/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using StackExchange.Redis;

namespace SocialEdgeSDK.Server.DataService
{
    public interface ICache
    {
        Task<bool> Set(string key, string value);
        Task<string> Get(string key);
        Task AddPlayerToRoom(string roomId, string playerId, string playerName="");
        Task<bool> DeleteRoom(string roomId);
        Task<HashEntry[]> GetRoom(string roomId);
        Task RemovePlayerFromRoom(string roomId, string playerId, string playerName="");
        long Increment(string key, long value);
        bool KeyDelete(string key);
        long GetValue(string key);
        bool SetExpiry(string key, double expireAfterSec);



    }
}