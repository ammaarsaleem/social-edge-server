/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;
using PlayFab.AuthenticationModels;
using PlayFab.DataModels;
using MongoDB.Bson;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Api
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
            request.EscapeObject = true;
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
                obj.DataObject = dataItem.Value.ToString();//.Replace("\"", "");
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
            request.Data = data;

            return await PlayFab.PlayFabServerAPI.UpdateUserReadOnlyDataAsync(request);
        }

        public static async Task<PlayFabResult<GetUserInventoryResult>> GetPlayerInventory(string playerId)
        {
            GetUserInventoryRequest request = new GetUserInventoryRequest();
            request.PlayFabId = playerId;            
            return await PlayFab.PlayFabServerAPI.GetUserInventoryAsync(request);
        }

        public static async Task<PlayFabResult<ModifyUserVirtualCurrencyResult>> AddVirtualCurrency(string playerId, int amount, string currentyType)
        {
            AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
            request.Amount = amount;
            request.PlayFabId = playerId;
            request.VirtualCurrency = currentyType;
            return await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
        }

        public static async Task<PlayFabResult<PlayFab.AdminModels.UpdateUserTitleDisplayNameResult>> UpdatePlayerDisplayName(string playerId, string displayName)
        {
            var request = new PlayFab.AdminModels.UpdateUserTitleDisplayNameRequest();
            request.DisplayName = displayName;
            request.PlayFabId = playerId;
            return await PlayFab.PlayFabAdminAPI.UpdateUserTitleDisplayNameAsync(request);
        }

        public static async Task<PlayFabResult<GetUserAccountInfoResult>> GetAccountInfo(string playerId)
        {
            GetUserAccountInfoRequest request = new GetUserAccountInfoRequest();
            request.PlayFabId = playerId;
            return await PlayFab.PlayFabServerAPI.GetUserAccountInfoAsync(request); 
        }

        public static async Task<PlayFabResult<GetPlayerCombinedInfoResult>> GetCombinedInfo(string playerId)
        {
            GetPlayerCombinedInfoRequest request = new GetPlayerCombinedInfoRequest();
            request.PlayFabId = playerId;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetPlayerProfile = true;
            request.InfoRequestParameters.GetUserReadOnlyData = true;
            request.InfoRequestParameters.GetPlayerStatistics = true;
            request.InfoRequestParameters.GetUserInventory = true;
            request.InfoRequestParameters.GetUserVirtualCurrency = true;
            request.InfoRequestParameters.GetUserAccountInfo = true;
            request.InfoRequestParameters.ProfileConstraints = new PlayerProfileViewConstraints();
            request.InfoRequestParameters.ProfileConstraints.ShowLocations = true;
            request.InfoRequestParameters.ProfileConstraints.ShowAvatarUrl = true;
            request.InfoRequestParameters.ProfileConstraints.ShowBannedUntil = true;
            request.InfoRequestParameters.ProfileConstraints.ShowCreated = true;
            request.InfoRequestParameters.ProfileConstraints.ShowDisplayName = true;
            request.InfoRequestParameters.ProfileConstraints.ShowLastLogin = true;

            return await PlayFab.PlayFabServerAPI.GetPlayerCombinedInfoAsync(request);
        }

        public static string GenerateDisplayName()
        {
            var displayNameAdjectiveArray = SocialEdge.TitleContext.GetTitleDataProperty("DisplayNameAdjectives")["Adjectives"].AsBsonArray;
            var displayNameNounsArray = SocialEdge.TitleContext.GetTitleDataProperty("DisplayNameNouns")["Nouns"].AsBsonArray;
            var randomNoun = displayNameNounsArray[(int)Math.Floor(new Random().NextDouble() * displayNameNounsArray.Count)];
            var randomAdjective = displayNameAdjectiveArray[(int)Math.Floor((new Random()).NextDouble() * displayNameAdjectiveArray.Count)];
            
            return randomAdjective + "" + randomNoun;
        }

        public static string GenerateTag()
        {
            const string COUNTER_COLLECTION_NAME = "playertagcounter";
            const string CHARACTER_SET = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const int MIN_LENGTH = 6;

            var collection = SocialEdge.DataService.GetCollection(COUNTER_COLLECTION_NAME);
            var counterDoc =  collection.IncAll("counter", 1);
            
            var salt = Utils.UTCNow().ToString();
            var hashids = new Hashids(salt, MIN_LENGTH, CHARACTER_SET);
            var counter = (int)counterDoc.Result["counter"];
            var tag = hashids.Encode(counter);

            return tag;
        }

        private static string GenerateAvatar()
        {
            var vAvatars = SocialEdge.TitleContext.CatalogItems.Catalog.FindAll(s => s.Tags[0].Equals("Avatar"));
            var randomAvatar = vAvatars[(int)(Math.Floor(new Random().NextDouble() * vAvatars.Count))];
            //BsonDocument avatarInventoryItem = new BsonDocument() {["key"] = randomAvatar.ItemId, ["kind"] = "Avatar"};
            return randomAvatar.ItemId.ToString();
        }

        private static string GenerateAvatarBgColor()
        {
            var colorCodesArray = SocialEdge.TitleContext.GetTitleDataProperty("GameSettings")["AvatarBgColors"];
            var randomColorCode = colorCodesArray[(int)(Math.Floor(new Random().NextDouble() * colorCodesArray.Count))];
            //BsonDocument colorCodeInventoryItem = new BsonDocument() {["key"] = randomColorCode, ["kind"] = "AvatarBgColor"};
            return randomColorCode.ToString();
        }    

        public static void NewPlayerInit(string playerId, string entityToken, string entityId)
        {
            var newName = Player.GenerateDisplayName();
            var newTag = Player.GenerateTag();
            var playerPublicData = SocialEdge.TitleContext.GetTitleDataProperty("NewPlayerSetup")["playerPublicData"];
            var playerData = SocialEdge.TitleContext.GetTitleDataProperty("NewPlayerSetup")["playerData"];
            var coinsCredit = (int)SocialEdge.TitleContext.GetTitleDataProperty("Economy")["BettingIncrements"][0];
            var avatar = GenerateAvatar();
            var avatarBgColor = GenerateAvatarBgColor();

            List<BsonValue> activeInventoryList = playerPublicData["ActiveInventory"]["invl"].ToList();
            var activeInventoryAvatar = activeInventoryList.Find(s => s[1].Equals("Avatar"));
            var activeInventoryBgColor = activeInventoryList.Find(s => s[1].Equals("AvatarBgColor"));

            playerData["coldData"]["isInitialized"] = true;
            playerPublicData["PublicProfileEx"]["tag"] = newTag;
            activeInventoryAvatar["key"] = avatar;
            activeInventoryBgColor["key"] = avatarBgColor;

            string chatDocumentId = InboxModel.Create();
            playerPublicData["DBIds"] = "{\"inbox\":" + "\""+ chatDocumentId +"\"," + "\"chat\":" + "\"\"}";
            
            var UpdatePlayerDataT = UpdatePlayerData(playerId, playerData);
            var addVirualCurrencyT = AddVirtualCurrency(playerId, coinsCredit, "CN");
            var updateDisplayNameT = UpdatePlayerDisplayName(playerId, newName);
            var UpdatePublicDataT = UpdatePublicData(entityToken, entityId, playerPublicData);

            Task.WaitAll(UpdatePlayerDataT, addVirualCurrencyT, updateDisplayNameT, UpdatePublicDataT);
        }
    }
}