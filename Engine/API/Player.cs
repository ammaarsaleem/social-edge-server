/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.AuthenticationModels;
using PlayFab.DataModels;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Api
{
    public class OnlineStatusNotifyMessageData
    {
        public string playerId;
        public bool isOnline;
    }

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

        public static async Task<PlayFabResult<GetFriendsListResult>> GetFriendsList(string playerId, bool includeFacebookFriends = true)
        {
            var request = new GetFriendsListRequest
            {
                PlayFabId = playerId,
                IncludeFacebookFriends = includeFacebookFriends,
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

            var task = await PlayFabServerAPI.GetFriendsListAsync(request);
            
            if(task.Error != null && task.Error.Error == PlayFabErrorCode.FacebookAPIError)
            {
                return await GetFriendsList(playerId, false);
            }

            return task;
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
            var sortById = Builders<PlayerModelDocument>.Sort.Descending("_id");
            FilterDefinition<PlayerModelDocument> filter = Builders<PlayerModelDocument>.Filter.In<string>("_id", friendsDBIds);
            var projection = Builders<PlayerModelDocument>.Projection.Expression(item => 
                                                            new PublicProfileEx(
                                                                    item._model.Info.isOnline,
                                                                    item._model.Info.created,
                                                                    item._model.Info.eloScore, 
                                                                    item._model.Info.trophies2, 
                                                                    item._model.Info.earnings,
                                                                    item._model.Info.gamesWon,
                                                                    item._model.Info.gamesLost,
                                                                    item._model.Info.gamesDrawn));

            return collection.Find(filter).Sort(sortById).Project(projection).ToList<PublicProfileEx>();
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

        public static async Task<PlayFabResult<PlayFab.ServerModels.GrantItemsToUserResult>> GrantItems(string playerId, List<string> itemsList)
        {
            var request = new PlayFab.ServerModels.GrantItemsToUserRequest();
            request.PlayFabId = playerId;
            request.ItemIds = itemsList;
            return await PlayFab.PlayFabServerAPI.GrantItemsToUserAsync(request);
        }

        public static async Task<PlayFabResult<PlayFab.ServerModels.ModifyItemUsesResult>> ModifyItemUses(string playerId, string itemInstanceId, int usesToAdd)
        {
            var request = new PlayFab.ServerModels.ModifyItemUsesRequest();
            request.PlayFabId = playerId;
            request.ItemInstanceId = itemInstanceId;
            request.UsesToAdd = usesToAdd;
            return await PlayFab.PlayFabServerAPI.ModifyItemUsesAsync(request);
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
            return randomAvatar.ItemId.ToString();
        }

        public static string GetAbbriviatedName(string name)
        {
            string[] split = name.Trim().Split(" ");
            return split.Length == 1 ? name : split[0] + " " + split[1].Substring(0, 1) + ".";
        }

        private static string GenerateAvatarBgColor()
        {
            var colorCodesArray = Settings.GameSettings["AvatarBgColors"];
            var randomColorCode = colorCodesArray[(int)(Math.Floor(new Random().NextDouble() * colorCodesArray.Count))];
            return randomColorCode.ToString();
        }

        public static void NewPlayerInit(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, string deviceId, string fbId, string appleId)
        {
            string playerId = socialEdgePlayer.PlayerId;
            string entityToken = socialEdgePlayer.EntityToken;
            string entityId = socialEdgePlayer.EntityId;

            //check if Gamespark data exists, init with GS data
            GSPlayerModelDocument gsPlayerData = socialEdgePlayer.PlayerModel.GetGSPlayerData(socialEdgePlayer, deviceId, fbId, appleId);
            if(gsPlayerData != null)
            {
                BsonDocument playerDocument = gsPlayerData.document;
                InitPlayerWithGsData(socialEdgePlayer, socialEdgeTournament, playerDocument);
                socialEdgePlayer.PlayerModel.Meta.isInitialized = true;
                Inbox.CreateAnnouncementMessage(socialEdgePlayer, "Important Update","Hey Champ!\nWe changed our server providers to bring you the best multiplayer gameplay experience. Our team worked hard to implement this technology switch. Please help us with your feedback and report issues on support.\nNow let's enjoy the game :)");
            }
            else // init fresh new player
            {
                SocialEdge.Log.LogInformation("CREATING NEW PLAYER . . . ");
                var newTag = Player.GenerateTag();
                var newName = Player.GenerateDisplayName();
                var avatar = GenerateAvatar();
                var avatarBgColor = GenerateAvatarBgColor();

                socialEdgePlayer.PlayerModel.CreateDefaults();
                socialEdgePlayer.PlayerModel.Meta.clientVersion = "0.0.1";
                socialEdgePlayer.PlayerModel.Meta.isInitialized = true;
                socialEdgePlayer.PlayerModel.Info.tag = newTag;
                socialEdgePlayer.PlayerModel.Info.created = socialEdgePlayer.Created;
                socialEdgePlayer.PlayerModel.Info.eloScore = 775;

                CatalogItem defaultSkin = SocialEdge.TitleContext.GetCatalogItem("SkinWood");
                PlayerInventoryItem skinItem = socialEdgePlayer.PlayerModel.Info.CreatePlayerInventoryItem();
                skinItem.kind = defaultSkin.Tags[0];
                skinItem.key = defaultSkin.ItemId;
                socialEdgePlayer.PlayerModel.Info.activeInventory.Add(skinItem);

                var addInventoryT = GrantItems(playerId, new List<string>{ "DefaultOwnedItems", "SkinWood"});
                InboxModel.Init(socialEdgePlayer.InboxId);

                socialEdgePlayer.MiniProfile.AvatarId = avatar;
                socialEdgePlayer.MiniProfile.AvatarBgColor = avatarBgColor;
                socialEdgePlayer.MiniProfile.UploadPicId = null;
                socialEdgePlayer.MiniProfile.EventGlow = 0;
                socialEdgePlayer.MiniProfile.isDirty = false;
                socialEdgePlayer.DisplayName = newName;
                UpdatePlayerAvatarData(playerId, socialEdgePlayer.MiniProfile);
                var updateDisplayNameT = UpdatePlayerDisplayName(playerId, newName);

                Task.WaitAll(updateDisplayNameT, addInventoryT);
            }
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
            playerPublicProfile._fbId = socialEdgePlayer.PlayerModel.Info.fbId;
            playerPublicProfile._location = socialEdgePlayer.CombinedInfo.PlayerProfile.Locations[0].CountryCode.ToString();
            playerPublicProfile._created = socialEdgePlayer.PlayerModel.Info.created;
            playerPublicProfile._lastLogin = DateTime.UtcNow;
            playerPublicProfile._activeInventory = socialEdgePlayer.PlayerModel.Info.activeInventory;
            playerPublicProfile._earnings = socialEdgePlayer.PlayerModel.Info.earnings;
            playerPublicProfile._eloScore = socialEdgePlayer.PlayerModel.Info.eloScore;
            playerPublicProfile._gamesDrawn = socialEdgePlayer.PlayerModel.Info.gamesDrawn;
            playerPublicProfile._gamesLost = socialEdgePlayer.PlayerModel.Info.gamesLost;
            playerPublicProfile._gamesWon = socialEdgePlayer.PlayerModel.Info.gamesWon;
            playerPublicProfile._trophies = socialEdgePlayer.PlayerModel.Info.trophies;
            playerPublicProfile._trophies2 = socialEdgePlayer.PlayerModel.Info.trophies2;
            playerPublicProfile.playerMiniProfile = socialEdgePlayer.MiniProfile;
            playerPublicProfile._clientVersion = socialEdgePlayer.PlayerModel.Meta.clientVersion;
            playerPublicProfile._storeId = socialEdgePlayer.PlayerModel.Meta.storeId;
            playerPublicProfile._lifeTimeStarsReceivedLevel = socialEdgePlayer.PlayerModel.Info.lifeTimeStarsReceivedLevel;

            if(socialEdgePlayer.PlayerModel.Info.retentionData.Count <=7){
                int dayNumber = (int)(DateTime.UtcNow - socialEdgePlayer.PlayerModel.Info.created).TotalDays;
                socialEdgePlayer.PlayerModel.Info.retentionData.Add("D" + dayNumber);
            }

            playerPublicProfile._retentionData = socialEdgePlayer.PlayerModel.Info.retentionData;

            return playerPublicProfile;
        }
        public static void NotifyOnlineStatus(SocialEdgePlayerContext socialEdgePlayer, bool isOnline)
        {
            if (socialEdgePlayer.PlayerModel.Friends.friends.Count == 0)
                return;

            string[] friendIds = new List<string>(socialEdgePlayer.PlayerModel.Friends.friends.Keys).ToArray();
            OnlineStatusNotifyMessageData notifyOnlineMessage = new OnlineStatusNotifyMessageData();
            notifyOnlineMessage.playerId = socialEdgePlayer.PlayerId;
            notifyOnlineMessage.isOnline = isOnline;
            new SocialEdgeMessage(socialEdgePlayer.PlayerId, notifyOnlineMessage, nameof(OnlineStatusNotifyMessageData), friendIds).Send();
        } 
        public static void InitPlayerWithGsData(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, BsonDocument playerDocument)
        {
            string playerId = socialEdgePlayer.PlayerId;
            SocialEdge.Log.LogInformation("PLAYER isInitialized FLAG > > > > > > > " + socialEdgePlayer.PlayerModel.Meta.isInitialized);
            
            if(socialEdgePlayer.PlayerModel.Meta.isInitialized == true){
                socialEdgePlayer.PlayerModel.GsMigrationResetDefaults();
            }else{
                socialEdgePlayer.PlayerModel.CreateDefaults();
            }

            InboxModel.Init(socialEdgePlayer.InboxId);

            string userID   = Utils.GetString(playerDocument, "userId");
            string deviceId = Utils.GetString(playerDocument, "deviceId"); 
            string FbId     = Utils.GetString(playerDocument, "facebookId");
            string appleId  = Utils.GetString(playerDocument, "appleId"); 
            string storeId  = Utils.GetString(playerDocument, "storeId"); 

            SocialEdge.Log.LogInformation("MIGRATE DATA FOUND WITH ID . . . . " + userID);
     
            BsonDocument sparkPlayer = Utils.GetDocument(playerDocument, "sparkPlayer");
            BsonDocument playerData = Utils.GetDocument(playerDocument, "playerData");
            BsonDocument priv = Utils.GetDocument(playerData, "priv");

            if(sparkPlayer != null)
            {
                DateTime creationTime = sparkPlayer["creationDate"].ToUniversalTime();
                socialEdgePlayer.PlayerModel.Info.created = creationTime;
                DateTime lastSceen =   sparkPlayer["lastSeen"].ToUniversalTime();

                string displayName = Utils.GetString(sparkPlayer,"displayName"); 
                if(priv != null){
                    string editedName = Utils.GetString(priv, "editedName");
                    displayName =  editedName != "" ? editedName : displayName;
                }
                
                displayName = displayName.Length > 15 ? displayName.Substring(0, 15) : displayName;
                displayName = displayName.Length < 3 ? displayName.PadRight(3, '~') : displayName;
                socialEdgePlayer.DisplayName = displayName;
                var updateDisplayNameT = UpdatePlayerDisplayName(playerId, displayName);
                updateDisplayNameT.Wait();

                int numCoins = Utils.GetInt(sparkPlayer, "coins"); 
                int numGems  = Utils.GetInt(sparkPlayer, "gems"); 
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", numCoins);
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("GM", numGems);

                socialEdgePlayer.PlayerModel.Economy.piggyBankGems          = Utils.GetInt(sparkPlayer, "piggyBank");
                socialEdgePlayer.PlayerModel.Tournament.tournamentMaxScore  = Utils.GetInt(sparkPlayer, "tournamentMaxScore");
                socialEdgePlayer.PlayerModel.Info.playDays                  = Utils.GetInt(sparkPlayer, "playDays");
                //socialEdgePlayer.PlayerModel.Economy.dollarSpent  = Utils.GetInt(sparkPlayer, "dollarSpent");
                //socialEdgePlayer.PlayerModel.Economy.flagMask  = Utils.GetInt(sparkPlayer, "flagMask");
   
                BsonDocument inventory = Utils.GetDocument(sparkPlayer, "inventory");
                if(inventory != null){
                    UpdateInventoryWithGsData(socialEdgePlayer, inventory);
                }

                //update daily games count
                BsonArray dailyChallengeCount = Utils.GetArray(sparkPlayer, "challengeCount"); 
                if(dailyChallengeCount != null){
                    UpdateDailyGamesCount(socialEdgePlayer, dailyChallengeCount);
                }
            }

            if(playerData != null)
            {
                BsonDocument pub = Utils.GetDocument(playerData, "pub");
                if(pub != null)
                {
                    //socialEdgePlayer.PlayerModel.Meta.isInitialized = true;
                    socialEdgePlayer.PlayerModel.Info.tag           = Utils.GetString(pub, "tag"); 
                    socialEdgePlayer.PlayerModel.Info.eloScore      = Utils.GetInt(pub, "eloScore"); 
                    socialEdgePlayer.PlayerModel.Info.eloCompletedPlacementGames = Utils.GetInt(pub, "eloCompletedPlacementGames"); 
                    socialEdgePlayer.PlayerModel.Info.gamesWon      = Utils.GetInt(pub, "gamesWon"); 
                    socialEdgePlayer.PlayerModel.Info.gamesLost     = Utils.GetInt(pub, "gamesLost"); 
                    socialEdgePlayer.PlayerModel.Info.gamesDrawn    = Utils.GetInt(pub, "gamesDrawn"); 
                    socialEdgePlayer.PlayerModel.Info.trophies      = Utils.GetInt(pub, "trophies"); 
                    socialEdgePlayer.PlayerModel.Info.trophies2     = Utils.GetInt(pub, "trophies2"); 
                    //socialEdgePlayer.PlayerModel.Info.countryFlag = Utils.GetString(pub, "countryFlag"); 
                }

                if(priv != null)
                {
                    //socialEdgePlayer.PlayerModel.Meta.clientVersion        = Utils.GetString(priv, "clientVersion");
                    socialEdgePlayer.PlayerModel.Economy.isPremium          = Utils.GetBool(priv, "isPremium");
                    socialEdgePlayer.PlayerModel.Events.eventTimeStamp      = Utils.GetLong(priv,"eventTimeStamp");
                    socialEdgePlayer.PlayerModel.Events.dailyEventExpiryTimestamp = Utils.GetLong(priv,"dailyEventExpiryTimestamp");
                    socialEdgePlayer.PlayerModel.Events.dailyEventProgress  = Utils.GetInt(priv,"dailyEventProgress");
                    socialEdgePlayer.PlayerModel.Events.dailyEventState     = Utils.GetString(priv, "dailyEventState");
                    socialEdgePlayer.PlayerModel.Economy.balloonRewardsClaimedCount = Utils.GetInt(priv,"balloonRewardsClaimedCount");
                    socialEdgePlayer.PlayerModel.Economy.chestUnlockTimestamp = Utils.GetLong(priv,"chestUnlockTimestamp");
                    socialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp  = Utils.GetLong(priv,"rvUnlockTimestamp");
                    socialEdgePlayer.PlayerModel.Economy.shopRvMaxReward    = Utils.GetInt(priv,"shopRvMaxReward");
                    socialEdgePlayer.PlayerModel.Economy.shopRvDefaultBet   = Utils.GetInt(priv,"shopRvDefaultBet");
                    socialEdgePlayer.PlayerModel.Economy.shopRvRewardClaimedCount = Utils.GetInt(priv,"shopRvRewardClaimedCount");
                    socialEdgePlayer.PlayerModel.Economy.shopRvRewardCooldownTimestamp = Utils.GetLong(priv,"shopRvRewardCooldownTimestamp");
                    socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = Utils.GetLong(priv,"piggyBankExpiryTimestamp");
                    socialEdgePlayer.PlayerModel.Economy.piggyBankDoublerExipryTimestamp = Utils.GetLong(priv,"piggyBankDoublerExipryTimestamp");
                    socialEdgePlayer.PlayerModel.Economy.subscriptionExpiryTime = Utils.GetLong(priv,"subscriptionExpiryTime");
                    socialEdgePlayer.PlayerModel.Economy.subscriptionType = Utils.GetString(priv, "subscriptionType");
                    socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier = Utils.GetString(priv, "dynamicBundlePurchaseTier");
                    socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier = Utils.GetString(priv, "dynamicBundleDisplayTier");
                    socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTierNew = Utils.GetString(priv, "dynamicBundlePurchaseTierNew");
                    socialEdgePlayer.PlayerModel.Economy.freePowerPlayExipryTimestamp = Utils.GetLong(priv, "freePowerPlayExipryTimestamp");
                    socialEdgePlayer.PlayerModel.Economy.jackpotNotCollectedCounter =  Utils.GetInt(priv, "jackpotNotCollectedCounter");

                    BsonArray playerActiveInventory = Utils.GetArray(priv, "playerActiveInventory");
                    if(playerActiveInventory != null){
                        InitActiveInventoryWithGsData(socialEdgePlayer, playerActiveInventory, pub);
                        socialEdgePlayer.PlayerModel.Info.fbId = FbId;
                    }

                    BsonArray dailyEventRewards = Utils.GetArray(priv, "dailyEventRewards");
                    if(dailyEventRewards != null){
                        InitDailyRewardFromGS(socialEdgePlayer, dailyEventRewards);
                    }

                     BsonDocument activeTournaments = Utils.GetDocument(priv, "activeTournaments");
                    if(activeTournaments != null){
                        InitActiveTournmentScore(socialEdgePlayer, socialEdgeTournament, activeTournaments);
                    }
                }
            }
        }
        public static void UpdateDailyGamesCount(SocialEdgePlayerContext socialEdgePlayer, BsonArray dailyGamesCount)
         {
             if(dailyGamesCount != null)
             {
                for(int i=0; i<dailyGamesCount.Count; i++)
                {
                    BsonDocument dailyData = dailyGamesCount[i].AsBsonDocument;
                    BsonElement dailyElement = dailyData.GetElement(0);
                    string theDate = dailyElement.Name;
                    BsonDocument dailyMatches = dailyElement.Value.AsBsonDocument;
                    GameResults gameResults = new GameResults();
                    gameResults.won   = Utils.GetInt(dailyMatches, "win");
                    gameResults.lost  = Utils.GetInt(dailyMatches, "loss");
                    gameResults.drawn = Utils.GetInt(dailyMatches, "draw");

                    DateTime oDate = DateTime.Parse(theDate);
                    var dateKey = oDate.ToShortDateString();
                    socialEdgePlayer.PlayerModel.Info.gamesPlayedPerDay[dateKey] = gameResults;
                }
             }
         }
        public static void UpdateInventoryWithGsData(SocialEdgePlayerContext socialEdgePlayer, BsonDocument inventory)
        {
            Dictionary<string, int> gsItemDictionary = new Dictionary<string, int>();
            //Add object in Inventory
            if(inventory != null)
            {
                List<string> iventoryItemsList = new List<string>();
                foreach (BsonElement element in inventory) 
                {
                    string shortCode = element.Name;
                    if(shortCode == "DefaultOwnedItemsV1" || shortCode == "DefaultOwnedItemsV2"){
                        continue;
                    }

                    CatalogItem itemData = null;
                    if(SocialEdge.TitleContext.GetCatalogItemDictionary().ContainsKey(shortCode)){
                        itemData = SocialEdge.TitleContext.GetCatalogItemDictionary()[shortCode];
                    }

                    if(itemData != null && !iventoryItemsList.Contains(itemData.ItemId)){
                        iventoryItemsList.Add(itemData.ItemId);
                        BsonValue value = element.Value;
                        gsItemDictionary.Add(itemData.ItemId, value.AsInt32);
                    }                
                }

                if(iventoryItemsList.Count > 0){
                    var addInventoryT = GrantItems(socialEdgePlayer.PlayerId, iventoryItemsList);
                    addInventoryT.Wait();
                }
            }

            int deductCoins = 0;
            int deductGems  = 0;  
            //Modify item count
            foreach (var item in gsItemDictionary)
            {
                ItemInstance itemData = socialEdgePlayer.PlayerEconomy.GetInventoryItemWithItemID(socialEdgePlayer, item.Key);
                if(itemData != null)
                {
                    if(item.Value > itemData.RemainingUses){
                        itemData.RemainingUses = item.Value - itemData.RemainingUses;
                        var modifyT = ModifyItemUses(socialEdgePlayer.PlayerId, itemData.ItemInstanceId, (int)itemData.RemainingUses);
                        modifyT.Wait();
                        SocialEdge.Log.LogInformation("UPDATE ITEM  :" +  itemData.ItemId + " RemainingUses : " + itemData.RemainingUses);
                    }

                    CatalogItem catalogItemData = SocialEdge.TitleContext.GetCatalogItem(item.Key);
                    if(catalogItemData.Bundle != null && catalogItemData.Bundle.BundledVirtualCurrencies != null){

                        if(catalogItemData.Bundle.BundledVirtualCurrencies.ContainsKey("CN"))
                        {
                            deductCoins += ((int)catalogItemData.Bundle.BundledVirtualCurrencies["CN"]);
                        }

                        if(catalogItemData.Bundle.BundledVirtualCurrencies.ContainsKey("GM"))
                        {
                            deductGems += ((int)catalogItemData.Bundle.BundledVirtualCurrencies["GM"]);
                        }
                    }
                }
                else
                {
                    SocialEdge.Log.LogInformation("ITEM NOT FOUND :" +  item.Key);
                }
            }

            if(deductCoins > 0)
            {
                SocialEdge.Log.LogInformation("TOTAL DEDCUT COINS: " + deductCoins);
                socialEdgePlayer.PlayerEconomy.SubtractVirtualCurrency("CN", deductCoins);
            }

            if(deductGems > 0)
            {
                SocialEdge.Log.LogInformation("TOTAL DEDCUT GEMS: " + deductGems);
                socialEdgePlayer.PlayerEconomy.SubtractVirtualCurrency("GM", deductGems);
            }
        }

        public static void InitActiveInventoryWithGsData(SocialEdgePlayerContext socialEdgePlayer, BsonArray playerActiveInventory, BsonDocument pub)
        {
            if(playerActiveInventory != null)
            {
                string Avatar = GenerateAvatar();
                string AvatarBgColor = GenerateAvatarBgColor();

                for(int i=0; i<playerActiveInventory.Count; i++)
                {
                    BsonDocument dataItem = playerActiveInventory[i].AsBsonDocument;
                    string itemType  = Utils.GetString(dataItem, "kind");
                    string itemValue = Utils.GetString(dataItem, "shopItemKey");
                    if(itemType == "Skin")
                    {
                        string skin = itemValue;
                        CatalogItem activeSkin = SocialEdge.TitleContext.GetCatalogItem(skin);
                        PlayerInventoryItem skinItem = socialEdgePlayer.PlayerModel.Info.CreatePlayerInventoryItem();
                        skinItem.kind = activeSkin.Tags[0];
                        skinItem.key = activeSkin.ItemId;
                        socialEdgePlayer.PlayerModel.Info.activeInventory.Add(skinItem);
                    }
                    else if(itemType == "Avatar")
                    {
                        Avatar = itemValue;
                    }
                    else if(itemType == "AvatarBgColor")
                    {
                         AvatarBgColor = itemValue;
                    }
                    else if(itemType == "VideoLesson")
                    {
                        string videoId = itemValue;
                        float progress = Utils.Getfloat(dataItem, "progress");
                        socialEdgePlayer.PlayerModel.Info.videosProgress[videoId] = progress / 100;
                    }
                }

                socialEdgePlayer.MiniProfile.AvatarId = Avatar;
                socialEdgePlayer.MiniProfile.AvatarBgColor = AvatarBgColor;
                socialEdgePlayer.MiniProfile.League = Utils.GetInt(pub, "league");
                socialEdgePlayer.MiniProfile.UploadPicId = null;
                socialEdgePlayer.MiniProfile.EventGlow = Utils.GetBool(pub, "dailyEventRing") ? 1 : 0;
                socialEdgePlayer.MiniProfile.isDirty = false;
                UpdatePlayerAvatarData(socialEdgePlayer.PlayerId, socialEdgePlayer.MiniProfile);                
            }
        }

         public static void InitDailyRewardFromGS(SocialEdgePlayerContext socialEdgePlayer, BsonArray dailyEventRewards)
         {
            if(dailyEventRewards != null && dailyEventRewards.Count > 0)
            {
                for(int i=0; i<dailyEventRewards.Count; i++)
                {
                    BsonDocument dataItem = dailyEventRewards[i].AsBsonDocument;
                    DailyEventRewards rewardItem = new DailyEventRewards();
                    int gems  = Utils.GetInt(dataItem, "gems");
                    int coins = Utils.GetInt(dataItem, "coins");
                    rewardItem.gems = gems;
                    rewardItem.coins = coins;
                    socialEdgePlayer.PlayerModel.Events.dailyEventRewards.Add(rewardItem);
                }
            }
         }

        public static void InitActiveTournmentScore(SocialEdgePlayerContext socialEdgePlayer, SocialEdgeTournamentContext socialEdgeTournament, BsonDocument activeTournaments)
         {
            if(activeTournaments != null)
            {
                foreach (BsonElement element in activeTournaments) 
                {
                    SocialEdge.Log.LogInformation("NAME : : " + element.Name.ToString());
                    BsonDocument dataItem = element.Value.AsBsonDocument;
                    string shortCode  = Utils.GetString(dataItem, "shortCode");
                    if(string.Equals(shortCode, "TournamentWeeklyChampionship"))
                    {
                        long startTime    = Utils.GetLong(dataItem, "startTime");
                        int duration      = Utils.GetInt(dataItem, "duration");
                        int score         = Utils.GetInt(dataItem, "score");
                        bool isEnded = (Utils.UTCNow() <  startTime) || (Utils.UTCNow() > (startTime + duration * 60 * 1000 ));
                        if(!isEnded){
                            socialEdgeTournament.tournamentDefaultStartingScore = score;
                            SocialEdge.Log.LogInformation("PLAYER ACTIVE TOURNMENT IS RUNNING ADD HIS SCORE : : " + score);
                        }
                        else{
                            SocialEdge.Log.LogInformation("PLAYER ACTIVE TOURNMENT ENDDED : : " + startTime + " SCORE : " + score);
                        }
                    }
                }
            }
         }
    }
}