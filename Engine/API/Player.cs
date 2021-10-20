using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
namespace SocialEdge.Server.Api
{
    public static class Player
    {
        public static async Task<PlayFabResult<GetFriendsListResult>> GetFriendsList(string playerId)
        {
            var request = new GetFriendsListRequest
            {
                PlayFabId = playerId,
                IncludeFacebookFriends = true
            };

            var result = await PlayFabServerAPI.GetFriendsListAsync(request);
            return result;
        }
    }
}