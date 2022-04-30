using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;
using PlayFab.AuthenticationModels;
using System.Collections.Generic;
using PlayFab.DataModels;

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

            return await PlayFabServerAPI.GetFriendsListAsync(request);
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

            return await PlayFabProfilesAPI.GetProfilesAsync(request);
        }
        public static async Task<PlayFabResult<GetEntityTokenResponse>> GetTitleEntityToken()
        {
            var request = new GetEntityTokenRequest();
            return await PlayFab.PlayFabAuthenticationAPI.GetEntityTokenAsync(request);
        }
        public static async Task<PlayFabResult<GetObjectsResponse>> GetPublicData(string entityToken, string entityId)
        {
            PlayFab.DataModels.GetObjectsRequest request = new PlayFab.DataModels.GetObjectsRequest();
            request.Entity = new PlayFab.DataModels.EntityKey();
            request.AuthenticationContext = new PlayFabAuthenticationContext();
            request.AuthenticationContext.EntityToken = entityToken;
            request.Entity.Id = entityId;
            request.Entity.Type = "title_player_account";

            return await PlayFabDataAPI.GetObjectsAsync(request);
        }
        public static async Task<PlayFabResult<SetObjectsResponse>> UpdatePublicData(string entityToken, string entityId, dynamic dataDict)
        {
            List<SetObject> dataList =  new List<SetObject>();
            foreach (var dataItem in dataDict)
            {
                SetObject obj = new SetObject();
                obj.ObjectName = dataItem.Name.ToString();
                obj.DataObject = dataItem.Value.ToString();
                dataList.Add(obj);
            }

            SetObjectsRequest request = new SetObjectsRequest();
            request.Entity = new PlayFab.DataModels.EntityKey();
            request.AuthenticationContext = new PlayFabAuthenticationContext();
            request.AuthenticationContext.EntityToken = entityToken;
            request.Entity.Id = entityId;
            request.Entity.Type = "title_player_account";
            request.Objects = dataList;

            return await PlayFabDataAPI.SetObjectsAsync(request);
        }
        public static async Task<PlayFabResult<GetUserDataResult>> GetPlayerData(string playerId, List<string> keys)
        {
            PlayFab.ServerModels.GetUserDataRequest request = new PlayFab.ServerModels.GetUserDataRequest();
            request.PlayFabId = playerId;
            request.Keys = keys;

            return await PlayFab.PlayFabServerAPI.GetUserReadOnlyDataAsync(request);
        }        
        public static async Task<PlayFabResult<UpdateUserDataResult>> UpdatePlayerData(string playerId, dynamic dataDict)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (var dataItem in dataDict)
            {
                data.Add(dataItem.Name.ToString(), dataItem.Value.ToString());
            }
            
            PlayFab.ServerModels.UpdateUserDataRequest request = new PlayFab.ServerModels.UpdateUserDataRequest();
            request.PlayFabId = playerId;
            request.Data = dataDict;

            return await PlayFab.PlayFabServerAPI.UpdateUserReadOnlyDataAsync(request);
        }
    }
}