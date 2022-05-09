/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PlayFab.ProfilesModels;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Context
{
    public static class CacheSegment
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
        public const ulong READONLY = FRIENDS | FRIENDS_PROFILES | INVENTORY;
    }

    public class SocialEdgePlayerContext
    {
        private ulong _fillMask;
        private ulong _dirtyMask;
        private delegate bool CacheFnType();
        private Dictionary<ulong, CacheFnType> _fillMap;
        private Dictionary<ulong, CacheFnType> _writeMap;

        private string _playerId;
        private string _entityToken;
        private string _entityId;
        private Dictionary<string, EntityDataObject> _publicDataObjs;
        private BsonDocument _mongoDocIds;
        private BsonDocument _inbox;
        private string _inboxId;
        private BsonDocument _chat;
        private string _chatId;
        private BsonDocument _publicData;
        private BsonDocument _activeInventory;
        private List<FriendInfo> _friends;
        private List<EntityProfileBody> _friendsProfiles;
        private List<ItemInstance> _inventory;
        private Dictionary<string, int> _virtualCurrency;
        private Dictionary<string, int> _virtualCurrencyChange;

        public string PlayerId { get => _playerId; }
        public string EntityToken { get => _entityToken; }
        public string EntityId { get => _entityId; }
        public string InboxId { get => _inboxId; }

        public BsonDocument ActiveInventory { get => (((_fillMask & CacheSegment.ACTIVE_INVENTORY) != 0) || (CacheFill(CacheSegment.ACTIVE_INVENTORY))) ? _activeInventory : null; }
        public BsonDocument PublicData { get => (((_fillMask & CacheSegment.PUBLIC_DATA) != 0) || (CacheFill(CacheSegment.PUBLIC_DATA))) ? _publicData : null; }
        public BsonDocument Inbox { get => (((_fillMask & CacheSegment.INBOX) != 0) || (CacheFill(CacheSegment.INBOX))) ? _inbox : null; }                                                
        public BsonDocument Chat { get => (((_fillMask & CacheSegment.CHAT) != 0) || (CacheFill(CacheSegment.CHAT))) ? _chat : null; }
        public List<FriendInfo> Friends { get => (((_fillMask & CacheSegment.FRIENDS) != 0) || (CacheFill(CacheSegment.FRIENDS))) ? _friends : null; }
        public List<EntityProfileBody> FriendsProfiles { get => (((_fillMask & CacheSegment.FRIENDS_PROFILES) != 0) || (CacheFill(CacheSegment.FRIENDS_PROFILES))) ? _friendsProfiles : null; }
        public List<ItemInstance> Inventory { get => (((_fillMask & CacheSegment.INVENTORY) != 0) || (CacheFill(CacheSegment.INVENTORY))) ? _inventory : null; }
        public Dictionary<string, int> VirtualCurrency { get => (((_fillMask & CacheSegment.INVENTORY) != 0) || (CacheFill(CacheSegment.INVENTORY))) ? _virtualCurrency : null; }

        public string PublicDataJson { get => _publicData.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }
        public string InboxJson { get => _inbox.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }
        public string ChatJson { get => _chat.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }

        public bool SetDirtyBit(ulong dirtyMask)
        {
            if ((dirtyMask & CacheSegment.READONLY) != 0)
            {
                SocialEdge.Log.LogInformation("Error: This Cache segment is readonly!");
            }

            _dirtyMask |= dirtyMask;
            return true;
        }

        public SocialEdgePlayerContext(dynamic context)
        {
            _playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            _entityId = context.CallerEntityProfile.Entity.Id;
            _publicDataObjs = context.CallerEntityProfile.Objects;

            _fillMap = new Dictionary<ulong, CacheFnType>()
            {
                {CacheSegment.NONE, CacheFillNone},
                {CacheSegment.PUBLIC_DATA, CacheFillPublicData},
                {CacheSegment.INBOX, CacheFillInbox},
                {CacheSegment.CHAT, CacheFillChat},
                {CacheSegment.FRIENDS, CacheFillFriends},
                {CacheSegment.FRIENDS_PROFILES, CacheFillFriendProfiles},
                {CacheSegment.ACTIVE_INVENTORY, CacheFillActiveInventory},
                {CacheSegment.INVENTORY, CacheFillInventory}
            };

            _writeMap = new Dictionary<ulong, CacheFnType>()
            {
                {CacheSegment.NONE, CacheWriteNone},
                {CacheSegment.PUBLIC_DATA, CacheWritePublicData},
                {CacheSegment.INBOX, CacheWriteInbox},
                {CacheSegment.CHAT, CacheWriteChat},
                {CacheSegment.FRIENDS, CacheWriteReadOnlyError},
                {CacheSegment.FRIENDS_PROFILES, CacheWriteReadOnlyError},
                {CacheSegment.ACTIVE_INVENTORY, CacheWriteReadOnlyError},
                {CacheSegment.INVENTORY, CacheWriteReadOnlyError}
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
                _inboxId = _mongoDocIds["inbox"].ToString();
                _chatId = _mongoDocIds["chat"].ToString();
            }
        }

        private bool ValidateCacheEntityToken()
        {
            if (_entityToken == null)
            {
                var resultT = Player.GetTitleEntityToken();
                resultT.Wait();
                _entityToken = resultT.Result.Result.EntityToken;
            }
            return _entityToken != null;
        }

        private bool CacheFillNone()
        {
            SocialEdge.Log.LogInformation("Initialize empty cache");
            return true;
        }

        private bool CacheWriteNone()
        {
            SocialEdge.Log.LogInformation("Ignore cache write");
            return true;
        }

        private bool CacheWriteReadOnlyError()
        {
            SocialEdge.Log.LogInformation("ERROR: Attempt to write read only cache data!");
            return true;
        }

        private bool CacheFillPublicData()
        {
            _publicData = BsonDocument.Parse(Utils.CleanupJsonString(_publicDataObjs["PublicProfileEx"].EscapedDataObject));
            _fillMask |= _publicData != null ? CacheSegment.PUBLIC_DATA : 0;
            SocialEdge.Log.LogInformation("Parse PUBLIC_DATA");
            return _publicData != null;
        }

        private bool CacheWritePublicData()
        {
            var updatePublicDataT = Player.UpdatePublicData(_entityToken, _entityId, new BsonDocument(){ ["PublicProfileEx"] = _publicData });
            updatePublicDataT.Wait();
            return updatePublicDataT.Result.Error != null;
        }

        private bool CacheFillInbox()
        {
            ValidateCacheMongoDocIds();
             var inboxT = InboxModel.Get(_mongoDocIds["inbox"].ToString());
            _inbox = inboxT.Result != null ? inboxT.Result["inboxData"].AsBsonDocument :null;
            _fillMask |= _inbox != null ? CacheSegment.CHAT : 0;
            SocialEdge.Log.LogInformation("Task fetch INBOX");
            return _inbox != null;
        }

        private bool CacheWriteInbox()
        {
            var inboxT = InboxModel.Set(_inboxId, _inbox);
            inboxT.Wait();
            return inboxT.Result.ModifiedCount != 0;
        }

        private bool CacheFillChat()
        {
            ValidateCacheMongoDocIds();
            var chatT = ChatModel.Get(_chatId);
            chatT.Wait();
            _chat = chatT.Result != null ? chatT.Result["ChatData"].AsBsonDocument : null;
            _fillMask |=  _chat != null ? CacheSegment.CHAT : 0;
            SocialEdge.Log.LogInformation("Task fetch CHAT");
            return _chat != null;
        }

        private bool CacheWriteChat()
        {
            var chatT = ChatModel.Set(_chatId, _chat);
            chatT.Wait();
            return chatT.Result.ModifiedCount != 0;
        }

        private bool CacheFillFriends()
        {
            var friendsT = Player.GetFriendsList(_playerId);
            friendsT.Wait();
            _friends = friendsT.Result.Error == null ? friendsT.Result.Result.Friends : null;
            _fillMask |= _friends != null ? CacheSegment.FRIENDS : 0;
            SocialEdge.Log.LogInformation("Task fetch FRIENDS");
            return _friends != null;
        }

        private bool CacheFillFriendProfiles()
        {
            var success = ValidateCacheEntityToken();
            var friends = Friends;
            var friendsProfilesT = Player.GetFriendProfiles(_friends, _entityToken);
            friendsProfilesT.Wait();
            _friendsProfiles = friendsProfilesT.Result.Error == null ? friendsProfilesT.Result.Result.Profiles : null;
            _fillMask |= _friendsProfiles != null ? CacheSegment.FRIENDS_PROFILES : 0;
            SocialEdge.Log.LogInformation("Task fetch FRIENDS_PROFILES");
            return _friendsProfiles != null;
        }

        private bool CacheFillActiveInventory()
        {
            _activeInventory = BsonDocument.Parse(_publicDataObjs["ActiveInventory"].EscapedDataObject);
            _fillMask |= _activeInventory != null ? CacheSegment.ACTIVE_INVENTORY : 0;
            SocialEdge.Log.LogInformation("Parse ACTIVE_INVENTORY");
            return _activeInventory != null;
        }

        private bool CacheFillInventory()
        {
            var getPlayerInventoryT = Player.GetPlayerInventory(_playerId);
            getPlayerInventoryT.Wait();
            _inventory = getPlayerInventoryT.Result.Error == null ? getPlayerInventoryT.Result.Result.Inventory : null;
            _virtualCurrency = _inventory != null ? getPlayerInventoryT.Result.Result.VirtualCurrency : null;
            _fillMask |= _inventory != null ? CacheSegment.INVENTORY : 0;
            SocialEdge.Log.LogInformation("Task fetch INVENTORY/VIRTUALCURRENCY");
            return _inventory != null;
        }

        public bool CacheFill(ulong fetchMask)
        {
            return (bool)_fillMap[fetchMask]?.Invoke();
        }

        public bool CacheFillBatch(ulong fetchMask)
        {
            if (fetchMask == 0)
                return true;

            ulong bit = 0x1;
            while (bit != (CacheSegment.MAX << 1))
            {
                if ((bit & fetchMask) != 0)
                    _fillMap[bit]?.Invoke();
                
                bit <<= 1;
            }
            return true;
        }

        public bool CacheWriteBatch()
        {
            if (_dirtyMask == 0)
                return true;

            ulong bit = 0x1;
            while (bit != (CacheSegment.MAX << 1))
            {
                if ((bit & _dirtyMask) != 0)
                    _writeMap[bit]?.Invoke();
                
                bit <<= 1;
            }
            return true;
        }
   }
}