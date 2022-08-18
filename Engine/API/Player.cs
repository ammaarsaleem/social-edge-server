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
using MongoDB.Driver;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Logging;


namespace SocialEdgeSDK.Server.Api
{
    public static class Player
    {
        public static async Task<PlayFabResult<GetPlayerProfileResult>> GetPlayerProfile(string playerId)
        {
            var request = new GetPlayerProfileRequest();
            request.PlayFabId = playerId;
            request.ProfileConstraints = new PlayerProfileViewConstraints()
            {
                ShowAvatarUrl = true,
                ShowLinkedAccounts = true,
                ShowBannedUntil = true,
                ShowCreated = true,
                ShowDisplayName = true,
                ShowLastLogin = true,
                ShowLocations = true,
                ShowTotalValueToDateInUsd = true,
                ShowStatistics = true,
                ShowOrigination = true
            };

            return await PlayFabServerAPI.GetPlayerProfileAsync(request);
        }

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
                    ShowStatistics = true,
                    ShowOrigination = true
                }
            };

            return await PlayFabServerAPI.GetFriendsListAsync(request);
        }   

        public static List<PublicProfileEx> GetFriendProfilesEx(List<FriendInfo> friends)
        {
            List<string> friendsDBIds = new List<string>();
            foreach (var friend in friends)
            {
                string friendDBId = friend.FriendPlayFabId.ToLower().PadLeft(24, '0'); 
                friendsDBIds.Add(friendDBId);
            }

            const string PLAYERMODEL_COLLECTION_NAME = "playerModel";
            var collection = SocialEdge.DataService.GetDatabase().GetCollection<PlayerModelDocument>(PLAYERMODEL_COLLECTION_NAME);
            FilterDefinition<PlayerModelDocument> filter = Builders<PlayerModelDocument>.Filter.In<string>("_id", friendsDBIds);
            var projection = Builders<PlayerModelDocument>.Projection.Expression(item => 
                                                            new PublicProfileEx(
                                                                    item._model.Info.eloScore, 
                                                                    item._model.Info.trophies, 
                                                                    item._model.Info.earnings,
                                                                    item._model.Info.gamesWon,
                                                                    item._model.Info.gamesLost,
                                                                    item._model.Info.gamesDrawn));

            return collection.Find(filter).Project(projection).ToList<PublicProfileEx>();
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
            List<SetObject> dataList = new List<SetObject>();
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

        public static async Task<PlayFabResult<UpdatePlayerStatisticsResult>> UpdatePlayerStatistics(string playerId, string key, int value)
        {
            List<StatisticUpdate> list = new List<StatisticUpdate>();
            StatisticUpdate entry = new StatisticUpdate();
            entry.StatisticName = key;
            entry.Value = value;
            list.Add(entry);

            UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
            request.PlayFabId = playerId;
            request.Statistics = list;

            return await PlayFab.PlayFabServerAPI.UpdatePlayerStatisticsAsync(request);
        }

        public static async Task<PlayFabResult<GetUserInventoryResult>> GetPlayerInventory(string playerId)
        {
            GetUserInventoryRequest request = new GetUserInventoryRequest();
            request.PlayFabId = playerId;
            return await PlayFab.PlayFabServerAPI.GetUserInventoryAsync(request);
        }

        public static async Task<PlayFabResult<ModifyUserVirtualCurrencyResult>> AddVirtualCurrency(string playerId, int amount, string currencyType)
        {
            AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
            request.Amount = amount;
            request.PlayFabId = playerId;
            request.VirtualCurrency = currencyType;
            return await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
        }

        public static async Task<PlayFabResult<ModifyUserVirtualCurrencyResult>> SubtractVirtualCurrency(string playerId, int amount, string currencyType)
        {
            SubtractUserVirtualCurrencyRequest request = new SubtractUserVirtualCurrencyRequest();
            request.Amount = amount;
            request.PlayFabId = playerId;
            request.VirtualCurrency = currencyType;
            return await PlayFab.PlayFabServerAPI.SubtractUserVirtualCurrencyAsync(request);
        }

        public static async Task<PlayFabResult<PlayFab.AdminModels.UpdateUserTitleDisplayNameResult>> UpdatePlayerDisplayName(string playerId, string displayName)
        {
            var request = new PlayFab.AdminModels.UpdateUserTitleDisplayNameRequest();
            request.DisplayName = displayName;
            request.PlayFabId = playerId;
            return await PlayFab.PlayFabAdminAPI.UpdateUserTitleDisplayNameAsync(request);
        }

        public static bool UpdatePlayerAvatarData(string playerId, PlayerMiniProfileData playerMiniProfile)
        {
            string avatarData = playerMiniProfile.ToJson();
            var request = new PlayFab.ServerModels.UpdateAvatarUrlRequest();
            request.PlayFabId = playerId;
            request.ImageUrl = avatarData;
            var requestT = PlayFab.PlayFabServerAPI.UpdateAvatarUrlAsync(request);
            requestT.Wait();
            return requestT.Result.Error == null;
        }
        public static async Task<PlayFabResult<GetUserAccountInfoResult>> GetAccountInfo(string playerId)
        {
            GetUserAccountInfoRequest request = new GetUserAccountInfoRequest();
            request.PlayFabId = playerId;
            return await PlayFab.PlayFabServerAPI.GetUserAccountInfoAsync(request);
        }
        public static void PurchaseItem(string playerId, string itemId, int price, string vCurrency)
        {
            var taskGrantT = GrantItem(playerId, itemId);
            var taskCurrencyT = SubtractVirtualCurrency(playerId, price, vCurrency);
            Task.WaitAll(taskGrantT, taskCurrencyT);
        }
        public static async Task<PlayFabResult<PlayFab.ServerModels.GrantItemsToUserResult>> GrantItem(string playerId, string itemId)
        {
            var request = new PlayFab.ServerModels.GrantItemsToUserRequest();
            request.PlayFabId = playerId;
            request.ItemIds = new List<string> { itemId };
            return await PlayFab.PlayFabServerAPI.GrantItemsToUserAsync(request);
        }

        public static async Task<PlayFabResult<PlayFab.ServerModels.ConsumeItemResult>> ConsumeItem(string playerId, string itemId)
        {
            ConsumeItemRequest request = new ConsumeItemRequest();
            request.PlayFabId = playerId;
            request.ConsumeCount = 1;
            request.ItemInstanceId = itemId;
            return await PlayFab.PlayFabServerAPI.ConsumeItemAsync(request);
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

            var collection = SocialEdge.DataService.GetCollection<BsonDocument>(COUNTER_COLLECTION_NAME);
            var counterDoc = collection.IncAll("counter", 1);

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
            //var colorCodesArray = SocialEdge.TitleContext.GetTitleDataProperty("GameSettings")["AvatarBgColors"];
            var colorCodesArray = Settings.GameSettings["AvatarBgColors"];
            var randomColorCode = colorCodesArray[(int)(Math.Floor(new Random().NextDouble() * colorCodesArray.Count))];
            //BsonDocument colorCodeInventoryItem = new BsonDocument() {["key"] = randomColorCode, ["kind"] = "AvatarBgColor"};
            return randomColorCode.ToString();
        }

        public static void NewPlayerInit(SocialEdgePlayerContext socialEdgePlayer)
        {
            string playerId = socialEdgePlayer.PlayerId;
            string entityToken = socialEdgePlayer.EntityToken;
            string entityId = socialEdgePlayer.EntityId;

            var newName = Player.GenerateDisplayName();
            var newTag = Player.GenerateTag();
            //var playerPublicData = SocialEdge.TitleContext.GetTitleDataProperty("NewPlayerSetup")["playerPublicData"];
            //var playerData = SocialEdge.TitleContext.GetTitleDataProperty("NewPlayerSetup")["playerData"];
            //var coinsCredit = (int)SocialEdge.TitleContext.GetTitleDataProperty("Economy")["BettingIncrements"][0];
            var coinsCredit = SocialEdge.TitleContext.EconomySettings.BettingIncrements[0];
            var avatar = GenerateAvatar();
            var avatarBgColor = GenerateAvatarBgColor();

            //List<BsonValue> activeInventoryList = playerPublicData["ActiveInventory"]["invl"].ToList();
            // var activeInventoryAvatar = activeInventoryList.Find(s => s[1].Equals("Avatar"));
            //var activeInventoryBgColor = activeInventoryList.Find(s => s[1].Equals("AvatarBgColor"));

            socialEdgePlayer.PlayerModel.CreateDefaults();
            socialEdgePlayer.PlayerModel.Meta.clientVersion = "0.0.1";
            socialEdgePlayer.PlayerModel.Meta.isInitialized = true;
            socialEdgePlayer.PlayerModel.Info.tag = newTag;
            socialEdgePlayer.PlayerModel.Info.eloScore = 775;

            CatalogItem defaultSkin = SocialEdge.TitleContext.GetCatalogItem("SkinDark");
            PlayerInventoryItem skinItem = new PlayerInventoryItem();
            skinItem.kind = defaultSkin.Tags[0];
            skinItem.key = defaultSkin.ItemId;
            socialEdgePlayer.PlayerModel.Info.activeInventory.Add(skinItem);

            var addInventoryT = GrantItem(playerId, defaultSkin.ItemId);

            //playerData["coldData"]["isInitialized"] = true;
            //playerPublicData["PublicProfileEx"]["tag"] = newTag;
            // activeInventoryAvatar["key"] = avatar;
            //activeInventoryBgColor["key"] = avatarBgColor;

            InboxModel.Init(socialEdgePlayer.InboxId);
            //playerPublicData["DBIds"] = "{\"inbox\":" + "\""+ chatDocumentId +"\"," + "\"chat\":" + "\"\"}";

            //String avatarInfo =  avatar + "," + avatarBgColor + "," + "XXX" + "," + "0";
            // PlayerMiniProfileData playerMiniProfile =  new PlayerMiniProfileData();
            socialEdgePlayer.MiniProfile.AvatarId = avatar;
            socialEdgePlayer.MiniProfile.AvatarBgColor = avatarBgColor;
            socialEdgePlayer.MiniProfile.UploadPicId = null;
            socialEdgePlayer.MiniProfile.EventGlow = 0;
            UpdatePlayerAvatarData(playerId, socialEdgePlayer.MiniProfile);
            //var UpdatePlayerDataT = UpdatePlayerData(playerId, playerData);
            var addVirualCurrencyT = AddVirtualCurrency(playerId, coinsCredit, "CN");
            var updateDisplayNameT = UpdatePlayerDisplayName(playerId, newName);
            //var UpdatePublicDataT = UpdatePublicData(entityToken, entityId, playerPublicData);

            //Task.WaitAll(UpdatePlayerDataT, addVirualCurrencyT, updateDisplayNameT, UpdatePublicDataT);
            //Task.WaitAll(addVirualCurrencyT, updateDisplayNameT);
            Task.WaitAll(addVirualCurrencyT, updateDisplayNameT, addInventoryT);
        }
        public static void PlayerCurrenyChanged(SocialEdgePlayerContext socialEdgePlayer, ILogger log)
        {
            int playerGems = (int)socialEdgePlayer.VirtualCurrency["GM"];
            int dynamicBundleMinGemsRequired = (int)Settings.CommonSettings["dynamicBundleMinGemsRequired"];

            SocialEdge.Log.LogInformation("PlayerCurrenyChanged : playerGems : " + playerGems + " dynamicBundleMinGemsRequired: " + dynamicBundleMinGemsRequired);
            if (socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount == 0 && (playerGems < dynamicBundleMinGemsRequired))
            {
                socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount = 1;
            }

            SocialEdge.Log.LogInformation("PlayerCurrenyChanged : socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount: " + socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount);
        }

        public static PlayerPublicProfile CreatePublicProfile(SocialEdgePlayerContext socialEdgePlayer)
        {
            PlayerPublicProfile playerPublicProfile = new PlayerPublicProfile();
            playerPublicProfile._displayName = socialEdgePlayer.CombinedInfo.PlayerProfile.DisplayName;
            playerPublicProfile._location = socialEdgePlayer.CombinedInfo.PlayerProfile.Locations[0].CountryCode.ToString();
            playerPublicProfile._created = socialEdgePlayer.CreationDate;
            playerPublicProfile._lastLogin = DateTime.Now;
            playerPublicProfile._activeInventory = socialEdgePlayer.PlayerModel.Info.activeInventory;
            playerPublicProfile._earnings = socialEdgePlayer.PlayerModel.Info.earnings;
            playerPublicProfile._eloScore = socialEdgePlayer.PlayerModel.Info.eloScore;
            playerPublicProfile._gamesDrawn = socialEdgePlayer.PlayerModel.Info.gamesDrawn;
            playerPublicProfile._gamesLost = socialEdgePlayer.PlayerModel.Info.gamesLost;
            playerPublicProfile._gamesWon = socialEdgePlayer.PlayerModel.Info.gamesWon;
            playerPublicProfile._trophies = socialEdgePlayer.PlayerModel.Info.trophies;
            playerPublicProfile._trophies2 = socialEdgePlayer.PlayerModel.Info.trophies2;
            playerPublicProfile.playerMiniProfile = socialEdgePlayer.MiniProfile;

            return playerPublicProfile;
        }
    }
}