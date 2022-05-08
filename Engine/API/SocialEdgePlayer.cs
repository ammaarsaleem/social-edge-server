/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using MongoDB.Bson;
using SocialEdgeSDK.Server.Api;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.AuthenticationModels;
using System.Collections.Generic;
using PlayFab.ProfilesModels;
using PlayFab.ServerModels;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Context
{
    public static class FetchBits
    {
        public const ulong NONE = 0;
        public const ulong PUBLIC_DATA = 0x1;
        public const ulong INBOX = 0x2;
        public const ulong CHAT = 0x4;
        public const ulong FRIENDS = 0x8;
        public const ulong FRIENDS_PROFILES  = 0x10;
        public const ulong ACTIVE_INVENTORY = 0x20;
        public const ulong INVENTORY = 0x40;
        public const ulong MAX = INVENTORY;

        public const ulong META = PUBLIC_DATA | INBOX | CHAT | FRIENDS_PROFILES | ACTIVE_INVENTORY;
    }

    public class SocialEdgePlayerContext
    {
        public delegate bool ValidateCacheFnType();

        private Dictionary<ulong, ValidateCacheFnType> _fetchMap;
        private ulong _fetchMask;
        private string _playerId;
        private string _entityToken;
        private string _entityId;
        private Dictionary<string, EntityDataObject> _publicDataObjs;
        private BsonDocument _mongoDocIds;
        private BsonDocument _inbox;
        private string _inboxId;
        private BsonDocument _chat;
        private BsonDocument _publicData;
        private BsonDocument _activeInventory;
        private List<FriendInfo> _friends;
        private List<EntityProfileBody> _friendsProfiles;
        private List<ItemInstance> _inventory;
        private Dictionary<string, int> _virtualCurrency;

        public string PlayerId { get => _playerId; }
        public string EntityToken { get => _entityToken; }
        public string EntityId { get => _entityId; }
        public string InboxId { get => _inboxId; }

        public BsonDocument ActiveInventory { get => (((_fetchMask & FetchBits.ACTIVE_INVENTORY) != 0) || (ValidateCacheBit(FetchBits.ACTIVE_INVENTORY))) ? _activeInventory : null; }
        public BsonDocument PublicData { get => (((_fetchMask & FetchBits.PUBLIC_DATA) != 0) || (ValidateCacheBit(FetchBits.PUBLIC_DATA))) ? _publicData : null; }
        public BsonDocument Inbox { get => (((_fetchMask & FetchBits.INBOX) != 0) || (ValidateCacheBit(FetchBits.INBOX))) ? _inbox : null; }                                                
        public BsonDocument Chat { get => (((_fetchMask & FetchBits.CHAT) != 0) || (ValidateCacheBit(FetchBits.CHAT))) ? _chat : null; }
        public List<FriendInfo> Friends { get => (((_fetchMask & FetchBits.FRIENDS) != 0) || (ValidateCacheBit(FetchBits.FRIENDS))) ? _friends : null; }
        public List<EntityProfileBody> FriendsProfiles { get => (((_fetchMask & FetchBits.FRIENDS_PROFILES) != 0) || (ValidateCacheBit(FetchBits.FRIENDS_PROFILES))) ? _friendsProfiles : null; }
        public List<ItemInstance> Inventory { get => (((_fetchMask & FetchBits.INVENTORY) != 0) || (ValidateCacheBit(FetchBits.INVENTORY))) ? _inventory : null; }
        public Dictionary<string, int> VirtualCurrency { get => (((_fetchMask & FetchBits.INVENTORY) != 0) || (ValidateCacheBit(FetchBits.INVENTORY))) ? _virtualCurrency : null; }

        public string PublicDataJson { get => _publicData.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }
        public string InboxJson { get => _inbox.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }
        public string ChatJson { get => _chat.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }

        public SocialEdgePlayerContext(dynamic context)
        {
            _playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            _entityId = context.CallerEntityProfile.Entity.Id;
            _publicDataObjs = context.CallerEntityProfile.Objects;

            _fetchMap = new Dictionary<ulong, ValidateCacheFnType>()
            {
                {FetchBits.NONE, ValidateCacheNone},
                {FetchBits.PUBLIC_DATA, ValidateCachePublicData},
                {FetchBits.INBOX, ValidateCacheInbox},
                {FetchBits.CHAT, ValidateCacheChat},
                {FetchBits.FRIENDS, ValidateCacheFriends},
                {FetchBits.FRIENDS_PROFILES, ValidateCacheFriendProfiles},
                {FetchBits.ACTIVE_INVENTORY, ValidataCacheActiveInventory},
                {FetchBits.INVENTORY, ValidataCacheInventory}
            };
        }

        public string PublicDataObjsJson 
        { 
            get 
            {
                BsonDocument doc = new BsonDocument()
                {
                    ["PublicProfileEx"] = _publicData,
                    ["ActiveInventory"] = _activeInventory
                };
                return doc.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
            }
        }

        private void ValidateCacheMongoDocIds()
        {
            if (_mongoDocIds == null)
            {
                _mongoDocIds = BsonDocument.Parse(_publicDataObjs["DBIds"].EscapedDataObject);
            }
        }

        private async Task<PlayFabResult<GetEntityTokenResponse>> ValidateCacheEntityToken()
        {
            if (_entityToken == null)
            {
                var resultT = await Player.GetTitleEntityToken();
                _entityToken = resultT.Result.EntityToken;
                return resultT;
            }
            return null;
        }

        private bool ValidateCacheNone()
        {
            SocialEdge.Log.LogInformation("Initialize empty cache");
            return true;
        }

        private bool ValidateCachePublicData()
        {
            _publicData = BsonDocument.Parse(Utils.CleanupJsonString(_publicDataObjs["PublicProfileEx"].EscapedDataObject));
            _fetchMask |= _publicData != null ? FetchBits.PUBLIC_DATA : 0;
            SocialEdge.Log.LogInformation("Parse PUBLIC_DATA");
            return _publicData != null;
        }

        private bool ValidateCacheInbox()
        {
            ValidateCacheMongoDocIds();
             var inboxT = InboxModel.Get(_mongoDocIds["inbox"].ToString());
            _inboxId = inboxT.Result != null ? _mongoDocIds["inbox"].ToString() : null;
            _inbox = inboxT != null ? inboxT.Result["inboxData"].AsBsonDocument :null;
            _fetchMask |= _inbox != null ? FetchBits.CHAT : 0;
            SocialEdge.Log.LogInformation("Task fetch INBOX");
            return _inbox != null;
        }

        private bool ValidateCacheChat()
        {
            ValidateCacheMongoDocIds();
            var chatT = SocialEdge.DataService.GetCollection("chat").FindOneById(_mongoDocIds["chat"].ToString());
            chatT.Wait();
            _chat = chatT.Result != null ? chatT.Result : null;
            _fetchMask |=  _chat != null ? FetchBits.CHAT : 0;
            SocialEdge.Log.LogInformation("Task fetch CHAT");
            return _chat != null;
        }

        private bool ValidateCacheFriends()
        {
            var friendsT = Player.GetFriendsList(_playerId);
            friendsT.Wait();
            _friends = friendsT.Result.Error == null ? friendsT.Result.Result.Friends : null;
            _fetchMask |= _friends != null ? FetchBits.FRIENDS : 0;
            SocialEdge.Log.LogInformation("Task fetch FRIENDS");
            return _friends != null;
        }

        private bool ValidateCacheFriendProfiles()
        {
            var entityTokenT = ValidateCacheEntityToken();
            entityTokenT.Wait();
            var friends = Friends;
            var friendsProfilesT = Player.GetFriendProfiles(_friends, _entityToken);
            friendsProfilesT.Wait();
            _friendsProfiles = friendsProfilesT.Result.Error == null ? friendsProfilesT.Result.Result.Profiles : null;
            _fetchMask |= _friendsProfiles != null ? FetchBits.FRIENDS_PROFILES : 0;
            SocialEdge.Log.LogInformation("Task fetch FRIENDS_PROFILES");
            return _friendsProfiles != null;
        }

        private bool ValidataCacheActiveInventory()
        {
            _activeInventory = BsonDocument.Parse(_publicDataObjs["ActiveInventory"].EscapedDataObject);
            _fetchMask |= _activeInventory != null ? FetchBits.ACTIVE_INVENTORY : 0;
            SocialEdge.Log.LogInformation("Parse ACTIVE_INVENTORY");
            return _activeInventory != null;
        }

        private bool ValidataCacheInventory()
        {
            var getPlayerInventoryT = Player.GetPlayerInventory(_playerId);
            getPlayerInventoryT.Wait();
            _inventory = getPlayerInventoryT.Result.Error == null ? getPlayerInventoryT.Result.Result.Inventory : null;
            _virtualCurrency = _inventory != null ? getPlayerInventoryT.Result.Result.VirtualCurrency : null;
            _fetchMask |= _inventory != null ? FetchBits.INVENTORY : 0;
            SocialEdge.Log.LogInformation("Task fetch INVENTORY/VIRTUALCURRENCY");
            return _inventory != null;
        }

        public bool ValidateCacheBit(ulong fetchMask)
        {
            return (bool)_fetchMap[fetchMask]?.Invoke();
        }

        public bool ValidateCache(ulong fetchMask)
        {
            if (fetchMask == 0)
                return true;

            ulong Bit = 0x1;
            while (Bit != (FetchBits.MAX << 1))
            {
                if ((Bit & fetchMask) != 0)
                    _fetchMap[Bit]?.Invoke();
                
                Bit <<= 1;
            }
            return true;
        }
   }
}