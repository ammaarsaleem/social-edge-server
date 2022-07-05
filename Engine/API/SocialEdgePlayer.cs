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
using PlayFab.Samples;

namespace SocialEdgeSDK.Server.Context
{
    public class PlayerDataSegment : BsonDocument
    {
        private SocialEdgePlayerContext _socialEdgePlayer;
        public new BsonDocument this[string key] => Get(key);
        private Dictionary<string, bool> _dirty;
        private string _fillKey;
        private string _lastAccessKey;

        internal string FillKey { get => _fillKey; }
        internal string LastAccessKey { get => _lastAccessKey; }
        public PlayerDataSegment(SocialEdgePlayerContext socialEdgePlayer) { _socialEdgePlayer = socialEdgePlayer; _dirty = new Dictionary<string, bool>(); }
        public void SetDirty(string key) { if (!_dirty.ContainsKey(key)) _dirty.Add(key, true); }
        public bool IsDirty(string key) { return _dirty.ContainsKey(key); }

        private BsonDocument Get(string key)
        {
            _lastAccessKey = key;

            if (this.Contains(key))
                return base[key].ToBsonDocument();

            _fillKey = key;
            bool filled = _socialEdgePlayer.CacheFillSegment(CacheSegment.PLAYER_DATA); 
            return filled ? base[key].ToBsonDocument() : null;
        }
    }

    public static class CacheSegment
    {
        public const ulong NONE =               0x0000;
        public const ulong PUBLIC_DATA =        0x0001;
        public const ulong INBOX =              0x0002;
        public const ulong CHAT =               0x0004;
        public const ulong FRIENDS =            0x0008;
        public const ulong FRIENDS_PROFILES  =  0x0010;
        public const ulong ACTIVE_INVENTORY =   0x0020;
        public const ulong INVENTORY =          0x0040;
        public const ulong ENTITY_TOKEN =       0x0080;
        public const ulong ENTITY_ID =          0x0100;
        public const ulong COMBINED_INFO =      0x0200;
        public const ulong PLAYER_DATA =        0x0400;
        public const ulong MAX = PLAYER_DATA;

        public const ulong META = PUBLIC_DATA | INBOX | CHAT | FRIENDS_PROFILES | ACTIVE_INVENTORY;
        public const ulong READONLY = FRIENDS | FRIENDS_PROFILES | INVENTORY;
    }

    public class SocialEdgePlayerContext
    {
        private enum ContextType
        {
            FUNCTION_EXECUTION_API,
            PLAYER_PLAYSTREAM,
            ENTITY_PLAYSTREAM,
            SCHEDULED_TASK
        }

        private ContextType _contextType;
        private ulong _fillMask;
        private ulong _dirtyMask;
        private delegate bool CacheFnType();
        private Dictionary<ulong, CacheFnType> _fillMap;
        private Dictionary<ulong, CacheFnType> _writeMap;

        private string _playerId;
        private string _entityToken;
        private string _entityId;
         private string _avatarInfo;
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
        private GetPlayerCombinedInfoResultPayload _combinedInfo;
        private PlayerDataSegment _playerData;

        public string PlayerId { get => _playerId; }
        public string AvatarInfo { get => _avatarInfo; }
        public string InboxId { get => _inboxId; }

        public PlayerDataSegment PlayerData { get => _playerData; }
        public GetPlayerCombinedInfoResultPayload CombinedInfo { get => (((_fillMask & CacheSegment.COMBINED_INFO) != 0) || (CacheFillSegment(CacheSegment.COMBINED_INFO))) ? _combinedInfo : null; }
        public string EntityId { get => (((_fillMask & CacheSegment.ENTITY_ID) != 0) || (CacheFillSegment(CacheSegment.ENTITY_ID))) ? _entityId : null; }
        public string EntityToken { get => (((_fillMask & CacheSegment.ENTITY_TOKEN) != 0) || (CacheFillSegment(CacheSegment.ENTITY_TOKEN))) ? _entityToken : null; }
        public BsonDocument ActiveInventory { get => (((_fillMask & CacheSegment.ACTIVE_INVENTORY) != 0) || (CacheFillSegment(CacheSegment.ACTIVE_INVENTORY))) ? _activeInventory : null; }
        public BsonDocument PublicData { get => (((_fillMask & CacheSegment.PUBLIC_DATA) != 0) || (CacheFillSegment(CacheSegment.PUBLIC_DATA))) ? _publicData : null; }
        public BsonDocument Inbox { get => (((_fillMask & CacheSegment.INBOX) != 0) || (CacheFillSegment(CacheSegment.INBOX))) ? _inbox : null; }                                                
        public BsonDocument Chat { get => (((_fillMask & CacheSegment.CHAT) != 0) || (CacheFillSegment(CacheSegment.CHAT))) ? _chat : null; }
        public List<FriendInfo> Friends { get => (((_fillMask & CacheSegment.FRIENDS) != 0) || (CacheFillSegment(CacheSegment.FRIENDS))) ? _friends : null; }
        public List<EntityProfileBody> FriendsProfiles { get => (((_fillMask & CacheSegment.FRIENDS_PROFILES) != 0) || (CacheFillSegment(CacheSegment.FRIENDS_PROFILES))) ? _friendsProfiles : null; }
        public List<ItemInstance> Inventory { get => (((_fillMask & CacheSegment.INVENTORY) != 0) || (CacheFillSegment(CacheSegment.INVENTORY))) ? _inventory : null; }
        public Dictionary<string, int> VirtualCurrency { get => (((_fillMask & CacheSegment.INVENTORY) != 0) || (CacheFillSegment(CacheSegment.INVENTORY))) ? _virtualCurrency : null; }

        public string PublicDataJson { get => PublicData != null ? _publicData.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}) : "{}"; }
        public string InboxJson { get => Inbox != null ? _inbox.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}) : "{}"; }
        public string ChatJson { get => Chat != null ? _chat.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}) : "{}"; }

        public bool SetDirtyBit(ulong dirtyMask)
        {
            if ((dirtyMask & CacheSegment.READONLY) != 0)
            {
                SocialEdge.Log.LogInformation("Error: This Cache segment is readonly!");
            }

            _dirtyMask |= dirtyMask;
            return true;
        }

        public SocialEdgePlayerContext(FunctionExecutionContext<dynamic> context)
        {
            _contextType = ContextType.FUNCTION_EXECUTION_API;
            _playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            _entityId = context.CallerEntityProfile.Entity.Id;
            _avatarInfo = context.CallerEntityProfile.AvatarUrl;
            _publicDataObjs = context.CallerEntityProfile.Objects;
            _fillMask |= _entityId != null ? CacheSegment.ENTITY_ID : 0;
            _playerData =  new PlayerDataSegment(this);
            SocialEdge.Log.LogInformation("SocialEdgePlayerContext _publicDataObjs count . . . " + _publicDataObjs.Keys.Count);

            SocialEdgePlayerContextInit();
        }

        public SocialEdgePlayerContext(PlayerPlayStreamFunctionExecutionContext<dynamic> context)
        {
            _contextType = ContextType.PLAYER_PLAYSTREAM;
            _playerId = context.PlayerProfile.PlayerId;
            _entityId = null;
            _publicDataObjs = null;
            _playerData =  new PlayerDataSegment(this);
            SocialEdge.Log.LogInformation("SocialEdgePlayerContext _playerData 2 . . . " + _playerData.ToString());
            SocialEdgePlayerContextInit();
        }

        private void SocialEdgePlayerContextInit()
        {
            _fillMap = new Dictionary<ulong, CacheFnType>()
            {
                {CacheSegment.NONE, CacheFillNone},
                {CacheSegment.PUBLIC_DATA, CacheFillPublicData},
                {CacheSegment.INBOX, CacheFillInbox},
                {CacheSegment.CHAT, CacheFillChat},
                {CacheSegment.FRIENDS, CacheFillFriends},
                {CacheSegment.FRIENDS_PROFILES, CacheFillFriendProfiles},
                {CacheSegment.ACTIVE_INVENTORY, CacheFillActiveInventory},
                {CacheSegment.INVENTORY, CacheFillInventory},
                {CacheSegment.ENTITY_TOKEN, CacheFillEntityToken},
                {CacheSegment.ENTITY_ID, CacheFillEntityId},
                {CacheSegment.COMBINED_INFO, CacheFillCombinedInfo},
                {CacheSegment.PLAYER_DATA, CacheFillPlayerData}
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
                {CacheSegment.INVENTORY, CacheWriteReadOnlyError},
                {CacheSegment.PLAYER_DATA, CacheWritePlayerData}
            };
        }

        public string PublicDataObjsJson 
        { 
            get 
            {
                // Leave out DBIds private information for security
                BsonDocument doc = new BsonDocument() { ["PublicProfileEx"] = PublicData, ["ActiveInventory"] = ActiveInventory };            
                //string temp = doc.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                return "test"; //doc.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
            }
        }

        private void ValidateCacheMongoDocIds()
        {
            if (_mongoDocIds == null)
            {
                _mongoDocIds = BsonDocument.Parse(Utils.CleanupJsonString(_publicDataObjs["DBIds"].EscapedDataObject));
                _inboxId = _mongoDocIds["inbox"].ToString();
                _chatId = _mongoDocIds["chat"].ToString();
            }
        }

        public bool SetPlayerData(string key, dynamic value, string segment = null)
        {
            if (segment != null)
            {
                var seg = PlayerData[segment];
            }

            if (string.IsNullOrEmpty(PlayerData.LastAccessKey))
                return false;

            PlayerData[PlayerData.LastAccessKey][key] = value;
            PlayerData.SetDirty(PlayerData.LastAccessKey);
            SetDirtyBit(CacheSegment.PLAYER_DATA);
            return true;
        }

        public BsonValue GetPlayerData(string key, string segment = null)
        {
            return segment != null ? PlayerData[segment][key] : (PlayerData.LastAccessKey != null ? PlayerData[PlayerData.LastAccessKey][key] : null);
        }

        private bool CacheFillNone()
        {
            SocialEdge.Log.LogInformation("Initialize empty cache");
            return true;
        }

        private bool CacheFillEntityToken()
        {
            // Title entity token
            var resultT = Player.GetTitleEntityToken();
            resultT.Wait();
            _entityToken = resultT.Result.Error == null ? resultT.Result.Result.EntityToken : null;
            _fillMask |= _entityToken != null ? CacheSegment.ENTITY_TOKEN : 0;
            SocialEdge.Log.LogInformation("Task fetch ENTITY_TOKEN" + (_entityToken != null ? "(success)" : "(failure)"));
            return _entityToken != null;        
        }

       private bool CacheFillEntityId()
        {
            // Title entity id (title_player_account)
            var resultT = Player.GetAccountInfo(_playerId);
            resultT.Wait();
            _entityId = resultT.Result.Result.UserInfo.TitleInfo.TitlePlayerAccount.Id;
            _fillMask |= _entityId != null ? CacheSegment.ENTITY_ID : 0;
            SocialEdge.Log.LogInformation("Task fetch ENTITY_ID");
            return _entityId != null;        
        }

        private bool CacheFillCombinedInfo()
        {
            var resulT = Player.GetCombinedInfo(_playerId);
            resulT.Wait();
            _combinedInfo = resulT.Result.Result.InfoResultPayload;
            if (_combinedInfo != null && _combinedInfo.UserReadOnlyData != null)
            {
                foreach(KeyValuePair<string, UserDataRecord> item in _combinedInfo.UserReadOnlyData)
                {
                    _playerData.Add(item.Key, BsonDocument.Parse(item.Value.Value));
                }
            }
            _fillMask |= _combinedInfo != null ? CacheSegment.COMBINED_INFO : 0;
            SocialEdge.Log.LogInformation("Task fetch COMBINED_INFO");
            return _combinedInfo != null;
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
            SocialEdge.Log.LogInformation("CacheFillPublicData called ******************123 ");

            SocialEdge.Log.LogInformation("_publicDataObjs  : : : !" + _publicDataObjs["PublicProfileEx"].EscapedDataObject.ToString());

            _publicData = BsonDocument.Parse(Utils.CleanupJsonString(_publicDataObjs["PublicProfileEx"].EscapedDataObject));

              if(_publicData == null){
                SocialEdge.Log.LogInformation("ERROR: _publicData  : : : !" + _publicData.ToString());
            }

            _fillMask |= _publicData != null ? CacheSegment.PUBLIC_DATA : 0;
            SocialEdge.Log.LogInformation("Parse PUBLIC_DATA");
            return _publicData != null;
        }

        private bool CacheWritePublicData()
        {
            var updatePublicDataT = Player.UpdatePublicData(EntityToken, EntityId, new BsonDocument(){ ["PublicProfileEx"] = _publicData });
            updatePublicDataT.Wait();
            return updatePublicDataT.Result.Error != null;
        }

        private bool CacheFillPlayerData()
        {
            var playerDataT = Player.GetPlayerData(_playerId, new List<string>() {_playerData.FillKey});
            playerDataT.Wait();
            if (playerDataT.Result.Error == null)
            {
                foreach(KeyValuePair<string, UserDataRecord> item in playerDataT.Result.Result.Data)
                {
                    _playerData.Add(item.Key, BsonDocument.Parse(item.Value.Value));
                }
            }
            _fillMask |= _playerData.ElementCount > 0 ? CacheSegment.PLAYER_DATA : 0;
            SocialEdge.Log.LogInformation("Task fetch PLAYER_DATA[" + _playerData.FillKey + "]");
            return _playerData.Contains(_playerData.FillKey);
        }

        private bool CacheWritePlayerData()
        {
            BsonDocument writeData = new BsonDocument();
            foreach(var data in PlayerData)
            {
                if (PlayerData.IsDirty(data.Name))
                    writeData.Add(data.Name, data.Value);
            }
            var playerDataT = Player.UpdatePlayerData(_playerId, writeData);
            playerDataT.Wait();
            SocialEdge.Log.LogInformation("Task flush PLAYER_DATA");
            return playerDataT.Result.Error != null;
        }

        private bool CacheFillInbox()
        {
            ValidateCacheMongoDocIds();
             var inboxT = InboxModel.Get(_mongoDocIds["inbox"].ToString());
             if (inboxT != null) inboxT.Wait();
            _inbox = inboxT != null && inboxT.Result != null ? inboxT.Result["inboxData"].AsBsonDocument : null;
            _fillMask |= _inbox != null ? CacheSegment.INBOX : 0;
            SocialEdge.Log.LogInformation("Task fetch INBOX");
            return _inbox != null;
        }

        private bool CacheWriteInbox()
        {
            var inboxT = InboxModel.Set(_inboxId, _inbox);
            inboxT.Wait();
            SocialEdge.Log.LogInformation("Task flush INBOX");
            return inboxT.Result.ModifiedCount != 0;
        }

        private bool CacheFillChat()
        {
            ValidateCacheMongoDocIds();
            var chatT = ChatModel.Get(_chatId);
            if (chatT != null) chatT.Wait();
            _chat = chatT != null && chatT.Result != null ? chatT.Result["ChatData"].AsBsonDocument : null;
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
            var friends = Friends;
            var friendsProfilesT = Player.GetFriendProfiles(_friends, EntityToken);
            friendsProfilesT.Wait();
            _friendsProfiles = friendsProfilesT.Result.Error == null ? friendsProfilesT.Result.Result.Profiles : null;
            _fillMask |= _friendsProfiles != null ? CacheSegment.FRIENDS_PROFILES : 0;
            SocialEdge.Log.LogInformation("Task fetch FRIENDS_PROFILES");

            // Remove DBIds private information for security
            if (_friendsProfiles != null)
            {
                foreach(var friendProfile in _friendsProfiles)
                {
                    friendProfile.Objects.Remove("DBIds");
                }
            }

            return _friendsProfiles != null;
        }

        private bool CacheFillActiveInventory()
        {
            _activeInventory = BsonDocument.Parse(Utils.CleanupJsonString(_publicDataObjs["ActiveInventory"].EscapedDataObject));
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

        public bool CacheFillSegment(ulong fetchMask)
        {
            return (bool)_fillMap[fetchMask]?.Invoke();
        }

        public bool CacheFill(ulong fetchMask)
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

        public bool CacheFlush()
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