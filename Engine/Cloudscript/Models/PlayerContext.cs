
using MongoDB.Bson;
using SocialEdge.Server.Api;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.AuthenticationModels;
using System.Collections.Generic;
using PlayFab.ProfilesModels;
using SocialEdge.Server.Common.Utils;
using PlayFab.ServerModels;
using PlayFab.DataModels;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.IO;

namespace SocialEdge.Server.Models
{
    public static class FetchBits
    {
        public const uint PUBLIC_DATA = 0x1;
        public const uint INBOX = 0x2;
        public const uint CHAT = 0x4;
        public const uint FRIENDS = 0x8;
        public const uint FRIENDS_PROFILES  = 0x10;
        public const uint ACTIVE_INVENTORY = 0x20;

        public const uint NONE = 0;
        public const uint ALL = PUBLIC_DATA | INBOX | CHAT | FRIENDS | FRIENDS_PROFILES | ACTIVE_INVENTORY;
    }

    public class PlayerContext
    {
        private uint _fetchMask;
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

        public string PlayerId { get => _playerId; }
        public string EntityToken { get => _entityToken; }
        public string EntityId { get => _entityId; }
        public string InboxId { get => _inboxId; }

        public BsonDocument ActiveInventory { get => (((_fetchMask & FetchBits.ACTIVE_INVENTORY) != 0) || (ValidateCache(FetchBits.ACTIVE_INVENTORY).Result & FetchBits.ACTIVE_INVENTORY) != 0) ? _activeInventory : null; }
        public BsonDocument PublicData { get => (((_fetchMask & FetchBits.PUBLIC_DATA) != 0) || (ValidateCache(FetchBits.PUBLIC_DATA).Result & FetchBits.PUBLIC_DATA) != 0) ? _publicData : null; }
        public BsonDocument Inbox { get => (((_fetchMask & FetchBits.INBOX) != 0) || (ValidateCache(FetchBits.INBOX).Result & FetchBits.INBOX) != 0) ? _inbox : null; }                                                
        public BsonDocument Chat { get => (((_fetchMask & FetchBits.CHAT) != 0) || (ValidateCache(FetchBits.CHAT).Result & FetchBits.CHAT) != 0) ? _chat : null; }
        public List<FriendInfo> Friends { get => (((_fetchMask & FetchBits.FRIENDS) != 0) || (ValidateCache(FetchBits.FRIENDS).Result & FetchBits.FRIENDS) != 0) ? _friends : null; }
        public List<EntityProfileBody> FriendsProfiles { get => (((_fetchMask & FetchBits.FRIENDS_PROFILES) != 0) || (ValidateCache(FetchBits.FRIENDS_PROFILES).Result & FetchBits.FRIENDS_PROFILES) != 0) ? _friendsProfiles : null; }

        public string PublicDataJson { get => _publicData.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }
        public string InboxJson { get => _inbox.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }
        public string ChatJson { get => _chat.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}); }

        public PlayerContext(dynamic context)
        {
            _playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            _entityId = context.CallerEntityProfile.Entity.Id;
            _publicDataObjs = context.CallerEntityProfile.Objects;
        }

        public string PublicDataObjsJson 
        { 
            get 
            {
                BsonDocument doc = new BsonDocument()
                {
                    ["PublicProfileEx"] = _publicData,//.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson}),
                    ["ActiveInventory"] = _activeInventory//.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson})
                };
                return doc.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
            }
        }


        private void ValidateMongoDocIdsCache()
        {
            if (_mongoDocIds == null)
            {
                _mongoDocIds = BsonDocument.Parse(_publicDataObjs["DBIds"].EscapedDataObject);
            }
        }

        private async Task<PlayFabResult<GetEntityTokenResponse>> ValidateEntityTokenCache()
        {
            if (_entityToken == null)
            {
                var resultT = await Player.GetTitleEntityToken();
                _entityToken = resultT.Result.EntityToken;
                return resultT;
            }
            return null;
        }

        public async Task<uint> ValidateCache(uint fetchMask)
        {
            //PlayFabResult<GetObjectsResponse> publicDataT = null;
            BsonDocument inboxT = null;
            BsonDocument chatT = null;  
            PlayFabResult<GetFriendsListResult> friendsT = null;
            PlayFabResult<GetEntityProfilesResponse> friendsProfilesT = null;

            // Friends
            if ((fetchMask & FetchBits.FRIENDS) != 0)
            {
                friendsT = await Player.GetFriendsList(_playerId);
                SocialEdgeEnvironment.Log.LogInformation("Task fetch FRIENDS");
            }

            // Public data
            if ((fetchMask & FetchBits.PUBLIC_DATA) != 0)
            {
                _publicData = BsonDocument.Parse(UtilFunc.CleanupJsonString(_publicDataObjs["PublicProfileEx"].EscapedDataObject));
                SocialEdgeEnvironment.Log.LogInformation("Parse PUBLIC_DATA");
            }

            // Active inventory
            if ((fetchMask & FetchBits.ACTIVE_INVENTORY) != 0)
            {
                _activeInventory = BsonDocument.Parse(_publicDataObjs["ActiveInventory"].EscapedDataObject);
                SocialEdgeEnvironment.Log.LogInformation("Parse ACTIVE_INVENTORY");
            }

            // Inbox
            if ((fetchMask & FetchBits.INBOX) != 0)
            {
                ValidateMongoDocIdsCache();
                inboxT = await InboxModel.Get(_mongoDocIds["inbox"].ToString());
                SocialEdgeEnvironment.Log.LogInformation("Task fetch INBOX");
            }

            // Chat
            if ((fetchMask & FetchBits.CHAT) != 0)
            {
                ValidateMongoDocIdsCache();
                chatT = await SocialEdgeEnvironment.DataService.GetCollection("chat").FindOneById(_mongoDocIds["chat"].ToString());
                SocialEdgeEnvironment.Log.LogInformation("Task fetch CHAT");
            }

            // Friends Profiles
            if ((fetchMask & FetchBits.FRIENDS_PROFILES) != 0)
            {
                await ValidateEntityTokenCache();
                if (_friends == null && ((fetchMask & FetchBits.FRIENDS) != 0))
                {
                    friendsProfilesT = await Player.GetFriendProfiles( friendsT.Result.Friends, _entityToken);
                }
                else
                {
                    friendsProfilesT = await Player.GetFriendProfiles(_friends, _entityToken);
                }

                SocialEdgeEnvironment.Log.LogInformation("Task fetch FRIENDS_PROFILES");
            }

            // Process fetches
            // Public data
            if ((fetchMask & FetchBits.PUBLIC_DATA) != 0)
            {
                _fetchMask |= _publicData != null ? FetchBits.PUBLIC_DATA : 0;
            }

            // Active inventory
            if ((fetchMask & FetchBits.ACTIVE_INVENTORY) != 0)
            {
                _fetchMask |= _publicData != null ? FetchBits.ACTIVE_INVENTORY : 0;
            }            

            // Inbox
            if ((fetchMask & FetchBits.INBOX) != 0)
            {
                _fetchMask |=  inboxT != null ? FetchBits.INBOX : 0;
                if ((_fetchMask & FetchBits.INBOX) != 0)
                {
                    _inboxId = _mongoDocIds["inbox"].ToString();
                    _inbox = inboxT["inboxData"].AsBsonDocument;
                }
            }

            // Chat
            if ((fetchMask & FetchBits.CHAT) != 0)
            {
                _fetchMask |=  chatT != null ? FetchBits.CHAT : 0;
                if ((_fetchMask & FetchBits.CHAT) != 0)
                {
                    _chat = chatT;
                }
            }

            // Friends
            if ((fetchMask & FetchBits.FRIENDS) != 0)
            {
                _fetchMask |=  friendsT.Error == null ? FetchBits.FRIENDS : 0;
                if ((_fetchMask & FetchBits.FRIENDS) != 0)
                {
                    _friends = friendsT.Result.Friends;
                }
            }

            // Friends Profiles
            if ((fetchMask & FetchBits.FRIENDS_PROFILES) != 0)
            {
                _fetchMask |=  friendsProfilesT.Error == null ? FetchBits.FRIENDS_PROFILES : 0;
                if ((_fetchMask & FetchBits.FRIENDS_PROFILES) != 0)
                {
                    _friendsProfiles = friendsProfilesT.Result.Profiles;
                }
            }

            SocialEdgeEnvironment.Log.LogInformation("Task fetch Completed!");


            return _fetchMask;
        }
        
    }
}