/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using Microsoft.Extensions.Logging;
using PlayFab.ProfilesModels;
using PlayFab.ServerModels;
using PlayFab.Samples;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;

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
            bool filled = _socialEdgePlayer.CacheFillSegment(CachePlayerDataSegments.PLAYER_DATA); 
            return filled ? base[key].ToBsonDocument() : null;
        }
    }

    public static class CachePlayerDataSegments
    {
        #pragma warning disable format 
        public const ulong NONE =               0x0000;
        public const ulong PUBLIC_DATA =        0x0001;
        public const ulong INBOX =              0x0002;
        public const ulong CHAT =               0x0004;
        public const ulong FRIENDS =            0x0008;
                                                // 0x0010;
        public const ulong FRIENDS_PROFILESEX = 0x0020;
        public const ulong INVENTORY =          0x0040;
        public const ulong ENTITY_TOKEN =       0x0080;
        public const ulong ENTITY_ID =          0x0100;
        public const ulong COMBINED_INFO =      0x0200;
        public const ulong PLAYER_DATA =        0x0400;
        public const ulong PLAYER_MODEL =       0x0800;
        public const ulong MINI_PROFILE =       0x1000;
        #pragma warning restore format
        
        public const ulong MAX = MINI_PROFILE;

        public const ulong META = INBOX | CHAT;
        public const ulong READONLY = FRIENDS | FRIENDS_PROFILESEX;
    }

    public class SocialEdgePlayerContext : ContextCacheBase
    {
        private string _playerId;
        private string _entityToken;
        private string _entityId;
        private DateTime _creationdDate;
        private PlayerMiniProfileData _miniProfile;
        private Dictionary<string, EntityDataObject> _publicDataObjs;
        private Dictionary<string, InboxDataMessage> _inbox;
        private BsonDocument _chat;
        private BsonDocument _publicData;
        private List<FriendInfo> _friends;
        private List<PublicProfileEx> _friendsProfilesEx;
        private List<ItemInstance> _inventory;
        private Dictionary<string, int> _virtualCurrencyBase;
        private Dictionary<string, int> _virtualCurrency;
        private GetPlayerCombinedInfoResultPayload _combinedInfo;
        private PlayerDataSegment _playerData;
        private PlayerDataModel _playerModel;
        private PlayerEconomy _playerEconomy;

        public string PlayerId => _playerId;
        public DateTime CreationDate => _creationdDate;
        public string InboxId { get => PlayerDBId; }
        public string ChatId { get => PlayerDBId; }
        public string PlayerDBId { get => _playerId.ToLower().PadLeft(24, '0'); }
        public string PlayerIdFromObjectId(ObjectId id) { return id.ToString().TrimStart('0'); }
        public string PlayerIdFromObjectId(string id) { return id.TrimStart('0'); }

        public PlayerDataModel PlayerModel { get => _playerModel != null ? _playerModel : _playerModel = new PlayerDataModel(this); }
        public PlayerEconomy PlayerEconomy { get => _playerEconomy != null ? _playerEconomy : _playerEconomy = new PlayerEconomy(this); }

        public PlayerDataSegment PlayerData { get => _playerData; }
        public PlayerMiniProfileData MiniProfile { get => (((_fillMask & CachePlayerDataSegments.MINI_PROFILE) != 0) || (CacheFillSegment(CachePlayerDataSegments.MINI_PROFILE))) ? _miniProfile : null; }
        public GetPlayerCombinedInfoResultPayload CombinedInfo { get => (((_fillMask & CachePlayerDataSegments.COMBINED_INFO) != 0) || (CacheFillSegment(CachePlayerDataSegments.COMBINED_INFO))) ? _combinedInfo : null; }
        public string EntityId { get => (((_fillMask & CachePlayerDataSegments.ENTITY_ID) != 0) || (CacheFillSegment(CachePlayerDataSegments.ENTITY_ID))) ? _entityId : null; }
        public string EntityToken { get => (((_fillMask & CachePlayerDataSegments.ENTITY_TOKEN) != 0) || (CacheFillSegment(CachePlayerDataSegments.ENTITY_TOKEN))) ? _entityToken : null; }
        public BsonDocument PublicData { get => (((_fillMask & CachePlayerDataSegments.PUBLIC_DATA) != 0) || (CacheFillSegment(CachePlayerDataSegments.PUBLIC_DATA))) ? _publicData : null; }
        public Dictionary<string, InboxDataMessage> Inbox { get => (((_fillMask & CachePlayerDataSegments.INBOX) != 0) || (CacheFillSegment(CachePlayerDataSegments.INBOX))) ? _inbox : null; }                                                
        public BsonDocument Chat { get => (((_fillMask & CachePlayerDataSegments.CHAT) != 0) || (CacheFillSegment(CachePlayerDataSegments.CHAT))) ? _chat : null; }
        public List<FriendInfo> Friends { get => (((_fillMask & CachePlayerDataSegments.FRIENDS) != 0) || (CacheFillSegment(CachePlayerDataSegments.FRIENDS))) ? _friends : null; }
        public List<PublicProfileEx> FriendsProfilesEx { get => (((_fillMask & CachePlayerDataSegments.FRIENDS_PROFILESEX) != 0) || (CacheFillSegment(CachePlayerDataSegments.FRIENDS_PROFILESEX))) ? _friendsProfilesEx : null; }
        public List<ItemInstance> Inventory { get => (((_fillMask & CachePlayerDataSegments.INVENTORY) != 0) || (CacheFillSegment(CachePlayerDataSegments.INVENTORY))) ? _inventory : null; }
        public Dictionary<string, int> VirtualCurrency { get => (((_fillMask & CachePlayerDataSegments.INVENTORY) != 0) || (CacheFillSegment(CachePlayerDataSegments.INVENTORY))) ? _virtualCurrency : null; }

        public string PublicDataJson { get => PublicData != null ? _publicData.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}) : "{}"; }
        public string InboxJson { get => Inbox != null ? _inbox.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}) : "{}"; }
        public string ChatJson { get => Chat != null ? _chat.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}) : "{}"; }

        public PlayerPublicProfile PublicProfile { get => Player.CreatePublicProfile(this); }

        public SocialEdgePlayerContext(string playerId)
        {
            _playerId = playerId;
            SocialEdgePlayerContextInit();
        }

        public SocialEdgePlayerContext(FunctionExecutionContext<dynamic> context)
        {
            _context = context;
            _contextType = ContextType.FUNCTION_EXECUTION_API;
            _playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            _creationdDate = context.CallerEntityProfile.Created;
            _entityId = context.CallerEntityProfile.Entity.Id;
            _publicDataObjs = context.CallerEntityProfile.Objects;
            _fillMask |= _entityId != null ? CachePlayerDataSegments.ENTITY_ID : 0;
            _playerData =  new PlayerDataSegment(this);
            SocialEdgePlayerContextInit();
        }

        public SocialEdgePlayerContext(PlayerPlayStreamFunctionExecutionContext<dynamic> context)
        {
            _contextType = ContextType.PLAYER_PLAYSTREAM;
            _playerId = context.PlayerProfile.PlayerId;
            _entityId = null;
            _publicDataObjs = null;
            _playerData =  new PlayerDataSegment(this);
            SocialEdgePlayerContextInit();
        }

        private void SocialEdgePlayerContextInit()
        {
            _fillMap = new Dictionary<ulong, CacheFnType>()
            {
                {CachePlayerDataSegments.NONE, CacheFillNone},
                {CachePlayerDataSegments.PUBLIC_DATA, CacheFillPublicData},
                {CachePlayerDataSegments.INBOX, CacheFillInbox},
                {CachePlayerDataSegments.CHAT, CacheFillChat},
                {CachePlayerDataSegments.FRIENDS, CacheFillFriends},
                {CachePlayerDataSegments.FRIENDS_PROFILESEX, CacheFillFriendProfilesEx},
                {CachePlayerDataSegments.INVENTORY, CacheFillInventory},
                {CachePlayerDataSegments.ENTITY_TOKEN, CacheFillEntityToken},
                {CachePlayerDataSegments.ENTITY_ID, CacheFillEntityId},
                {CachePlayerDataSegments.COMBINED_INFO, CacheFillCombinedInfo},
                {CachePlayerDataSegments.PLAYER_DATA, CacheFillPlayerData},
                {CachePlayerDataSegments.MINI_PROFILE, CacheFillMiniProfile},
            };

            _writeMap = new Dictionary<ulong, CacheFnType>()
            {
                {CachePlayerDataSegments.NONE, CacheWriteNone},
                {CachePlayerDataSegments.PUBLIC_DATA, CacheWritePublicData},
                {CachePlayerDataSegments.INBOX, CacheWriteInbox},
                {CachePlayerDataSegments.CHAT, CacheWriteChat},
                {CachePlayerDataSegments.FRIENDS, CacheWriteReadOnlyError},
                {CachePlayerDataSegments.FRIENDS_PROFILESEX, CacheWriteReadOnlyError},
                {CachePlayerDataSegments.INVENTORY, CacheWriteInventory},
                {CachePlayerDataSegments.PLAYER_DATA, CacheWritePlayerData},
                {CachePlayerDataSegments.PLAYER_MODEL, CacheWritePlayerModel},
                {CachePlayerDataSegments.MINI_PROFILE, CacheWriteMiniProfile},
            };
        }

        public string PublicDataObjsJson 
        { 
            get 
            {
                // // Leave out DBIds private information for security
                // BsonDocument doc = new BsonDocument();
                // doc.Add("PublicProfileEx", PublicData != null ? PublicData : new BsonDocument());
                // doc.Add("ActiveInventory", ActiveInventory != null ? ActiveInventory : new BsonDocument());
                                
                // return doc.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                return "";
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
            SetDirtyBit(CachePlayerDataSegments.PLAYER_DATA);
            return true;
        }

        public BsonValue GetPlayerData(string key, string segment = null)
        {
            return segment != null ? PlayerData[segment][key] : (PlayerData.LastAccessKey != null ? PlayerData[PlayerData.LastAccessKey][key] : null);
        }

        private bool CacheFillEntityToken()
        {
            // Title entity token
            var resultT = Player.GetTitleEntityToken();
            resultT.Wait();
            _entityToken = resultT.Result.Error == null ? resultT.Result.Result.EntityToken : null;
            _fillMask |= _entityToken != null ? CachePlayerDataSegments.ENTITY_TOKEN : 0;
            SocialEdge.Log.LogInformation("Task fetch ENTITY_TOKEN" + (_entityToken != null ? "(success)" : "(failure)"));
            return _entityToken != null;        
        }

       private bool CacheFillEntityId()
        {
            // Title entity id (title_player_account)
            var resultT = Player.GetAccountInfo(_playerId);
            resultT.Wait();
            _entityId = resultT.Result.Result.UserInfo.TitleInfo.TitlePlayerAccount.Id;
            _fillMask |= _entityId != null ? CachePlayerDataSegments.ENTITY_ID : 0;
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
            _fillMask |= _combinedInfo != null ? CachePlayerDataSegments.COMBINED_INFO : 0;
            SocialEdge.Log.LogInformation("Task fetch COMBINED_INFO");
            return _combinedInfo != null;
        }

        private bool CacheFillPublicData()
        {    
            if (_publicDataObjs == null || _publicDataObjs.ContainsKey("PublicProfileEx"))
            {
                SocialEdge.Log.LogInformation("Called CacheFillPublicData and it was NULLL : " + _publicDataObjs.ToString());
                return false;
            }
                
            _publicData = BsonDocument.Parse(Utils.CleanupJsonString(_publicDataObjs["PublicProfileEx"].EscapedDataObject));
            _fillMask |= _publicData != null ? CachePlayerDataSegments.PUBLIC_DATA : 0;
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
            _fillMask |= _playerData.ElementCount > 0 ? CachePlayerDataSegments.PLAYER_DATA : 0;
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
             var inboxT = InboxModel.Get(InboxId);
             if (inboxT != null) inboxT.Wait();
            _inbox = inboxT != null && inboxT.Result != null ? inboxT.Result._messages : null;
            _fillMask |= _inbox != null ? CachePlayerDataSegments.INBOX : 0;
            SocialEdge.Log.LogInformation("Task fetch INBOX");
            return _inbox != null;
        }

        private bool CacheWriteInbox()
        {
            var inboxT = InboxModel.Set(InboxId, _inbox);
            inboxT.Wait();
            SocialEdge.Log.LogInformation("Task flush INBOX");
            return inboxT.Result.ModifiedCount != 0;
        }

        private bool CacheFillChat()
        {
            var chatT = ChatModel.Get(ChatId);
            if (chatT != null) chatT.Wait();
            _chat = chatT != null && chatT.Result != null ? chatT.Result["ChatData"].AsBsonDocument : null;
            _fillMask |=  _chat != null ? CachePlayerDataSegments.CHAT : 0;
            SocialEdge.Log.LogInformation("Task fetch CHAT");
            return _chat != null;
        }

        private bool CacheWriteChat()
        {
            var chatT = ChatModel.Set(ChatId, _chat);
            chatT.Wait();
            return chatT.Result.ModifiedCount != 0;
        }

        private bool CacheFillFriends()
        {
            var friendsT = Player.GetFriendsList(_playerId);
            friendsT.Wait();
            _friends = friendsT.Result.Error == null ? friendsT.Result.Result.Friends : null;
            _fillMask |= _friends != null ? CachePlayerDataSegments.FRIENDS : 0;
            SocialEdge.Log.LogInformation("Task fetch FRIENDS");
            return _friends != null;
        }

        private bool CacheFillFriendProfilesEx()
        {
            var friends = Friends;
            _friendsProfilesEx = Player.GetFriendProfilesEx(_friends);
            _fillMask |= _friendsProfilesEx != null ? CachePlayerDataSegments.FRIENDS_PROFILESEX : 0;
            SocialEdge.Log.LogInformation("Task fetch FRIENDS_PROFILESEX");
            return _friendsProfilesEx != null;
        }

        private bool CacheFillInventory()
        {
            var getPlayerInventoryT = Player.GetPlayerInventory(_playerId);
            getPlayerInventoryT.Wait();
            _inventory = getPlayerInventoryT.Result.Error == null ? getPlayerInventoryT.Result.Result.Inventory : null;
            _virtualCurrency = _inventory != null ? getPlayerInventoryT.Result.Result.VirtualCurrency : null;
            _virtualCurrencyBase = _virtualCurrency != null ? new Dictionary<string, int>(_virtualCurrency) : null;
            _fillMask |= _inventory != null ? CachePlayerDataSegments.INVENTORY : 0;
            SetDirtyBit(_fillMask & CachePlayerDataSegments.INVENTORY);
            SocialEdge.Log.LogInformation("Task fetch INVENTORY/VIRTUALCURRENCY");
            return _inventory != null;
        }

        private bool CacheWriteInventory()
        {
            int writes = 0;
            foreach (var currency in _virtualCurrency)
            {
                int diff = currency.Value - _virtualCurrencyBase[currency.Key];
                var addTaskT = diff > 0 ? Player.AddVirtualCurrency(PlayerId, diff, currency.Key) : diff < 0 ? Player.SubtractVirtualCurrency(PlayerId, -diff, currency.Key) : null;
                if (diff > 0 || diff < 0)
                {
                    writes++;
                    SocialEdge.Log.LogInformation("Task flush VIRTUAL CURRENCY" + "(" + currency.Key + ")");
                }
            }
            return writes > 0;
        }

        private bool CacheWritePlayerModel()
        {
            return _playerModel != null ? _playerModel.CacheWrite() : false;
        }

        private bool CacheFillMiniProfile()
        {
            SocialEdge.Log.LogInformation("Parse MINI_PROFILE");

            string avatarURL = _context != null && _context.CallerEntityProfile.AvatarUrl != null ? _context.CallerEntityProfile.AvatarUrl.ToString() : CombinedInfo.PlayerProfile.AvatarUrl;
            _miniProfile = avatarURL != null ? BsonSerializer.Deserialize<PlayerMiniProfileData>(avatarURL) : new PlayerMiniProfileData();
            _fillMask |= _miniProfile != null ? CachePlayerDataSegments.MINI_PROFILE : 0;
            // Force write call. Dirty is controlled by mini profile internal data structure
            SetDirtyBit(CachePlayerDataSegments.MINI_PROFILE);
            return _miniProfile != null;
        }

        private bool CacheWriteMiniProfile()
        {
            if (_miniProfile.isDirty)
                SocialEdge.Log.LogInformation("Task flush MINI_PROFILE");

            return _miniProfile.isDirty ? Player.UpdatePlayerAvatarData(_playerId, _miniProfile) : false;
        }
   }
}