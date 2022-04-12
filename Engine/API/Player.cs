using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;
using PlayFab.AuthenticationModels;
using System.Collections.Generic;

namespace SocialEdge.Server.Api
{
    public static class Player
    {
        public static async Task<PlayFabResult<GetFriendsListResult>> GetFriendsList(string playerId)
        {
            var request = new GetFriendsListRequest
            {
                PlayFabId = playerId,
                IncludeFacebookFriends = true,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowAvatarUrl = true,
                    ShowLinkedAccounts = true,
                    ShowBannedUntil = true,
                    ShowCreated = true,
                    ShowDisplayName = true,
                    ShowLastLogin = true,
                    ShowLocations = true,
                    ShowTotalValueToDateInUsd = true,
                    ShowOrigination = true
                }
            };

            var result = await PlayFabServerAPI.GetFriendsListAsync(request);

            return result;
        }

        public static async Task<PlayFabResult<GetEntityProfilesResponse>> GetFriendProfiles(List<FriendInfo> friends, string etoken)
        {
            List<PlayFab.ProfilesModels.EntityKey> entities = new List<PlayFab.ProfilesModels.EntityKey>();
            foreach (var friend in friends)
            {
                entities.Add(new PlayFab.ProfilesModels.EntityKey() { Id = friend.Tags[1], Type = "title_player_account"});
            }

            var request = new GetEntityProfilesRequest();
            request.Entities = new List<PlayFab.ProfilesModels.EntityKey>();
            request.AuthenticationContext = new PlayFabAuthenticationContext();
            request.Entities = entities;
            request.AuthenticationContext.EntityToken = etoken;
            var result = await PlayFabProfilesAPI.GetProfilesAsync(request);

            return result;
        }

        public static async Task<PlayFabResult<GetEntityTokenResponse>> GetTitleEntityToken()
        {
            var request = new GetEntityTokenRequest();
            var result = await PlayFab.PlayFabAuthenticationAPI.GetEntityTokenAsync(request);
            return result;
        }


    }
}