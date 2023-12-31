/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PlayFab.Samples;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Models;
using MongoDB.Bson.Serialization;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.MessageService;

namespace SocialEdgeSDK.Server.Requests
{
    public class FriendsOpResult
    {
        public bool status;
        public string op;
        public string friendId;
        public string friendType;
        public int skip;
        public List<PlayerSearchDataModelDocument> searchList;
        public List<FriendInfo> friends;
        public List<PublicProfileEx> friendsProfilesEx;
    }

    public static class SubOpType
    {
        public const string REMOVE = "remove";
        public const string REMOVE_RECENT = "removeRecent";
    }

    public class FriendsSubOp
    {
        public List<string> friendIds;
        public string subOp;
    }

    public class FriendsOp : FunctionContext
    {
        public FriendsOp(ITitleContext titleContext, IDataService dataService, IMessageService messageService) { Base(titleContext, dataService, messageService); }

        [FunctionName("FriendsOp")]
        public FriendsOpResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            FriendsOpResult result = new FriendsOpResult();
            string friendId = data["friendId"] != null ? data["friendId"].ToString() : null;
            string op = result.op = data["op"].ToString();

            if (op == "initialize")
            {
                Dictionary<string, FriendData> friendsList = Friends.SyncFriendsList(SocialEdgePlayer);

                if (friendsList.Count > 0)
                {
                    result.friends = SocialEdgePlayer.Friends;
                    result.friendsProfilesEx = SocialEdgePlayer.FriendsProfilesEx;
                    Friends.NotifySync(SocialEdgePlayer, friendsList);
                }

                // Set fb id in player model info first so that it is available in the code following this.
                SocialEdgePlayer.PlayerModel.Info.fbId = SocialEdgePlayer.CombinedInfo.AccountInfo.FacebookInfo.FacebookId;
                PlayerSearch.Register(SocialEdgePlayer);

                // Note: This code is tied to a single entry Active Tournament.
                if (SocialEdgePlayer.PlayerModel.Tournament.activeTournaments.Count > 0)
                {
                    string tournamentId = SocialEdgePlayer.PlayerModel.Tournament.activeTournaments.ElementAt(0).Key;
                    var tournament = SocialEdgeTournament.TournamentModel.Get(tournamentId);
                    string collectionName = SocialEdgeTournament.TournamentLiveModel.Get(tournament.shortCode).collectionPrefix + tournament.tournamentCollectionIndex.ToString();
                    var entry = SocialEdgeTournament.TournamentEntryModel.Get(SocialEdgePlayer.PlayerDBId, collectionName);
                    entry.fbId = SocialEdgePlayer.PlayerModel.Info.fbId;
                    entry.displayName = SocialEdgePlayer.CombinedInfo.PlayerProfile.DisplayName;
                }

                result.status = true;
            }
            else if (op == "sync")
            {
                Friends.SyncFriendsList(SocialEdgePlayer);
                result.friends = SocialEdgePlayer.Friends;
                result.friendsProfilesEx = SocialEdgePlayer.FriendsProfilesEx;
                result.status = true;
            }
            else if (op == "add" || op == "addFavourite")
            {
                FriendData friendData = SocialEdgePlayer.PlayerModel.CreateFriendData();
                friendData._friendType = data["friendType"].ToString();
                SocialEdgePlayer.PlayerModel.DBOpAddFriend(friendId, friendData);
                result.friendId = friendId;
                result.friendType = data["friendType"].ToString();
                bool status = Friends.AddFriend(friendId, SocialEdgePlayer.PlayerId);
                result.status = true;
            }
            else if (op == "remove")
            {
                if (!string.IsNullOrEmpty(friendId))
                {
                    SocialEdgePlayer.PlayerModel.DBOpRemoveFriend(friendId);
                    result.friendId = friendId;
                    bool status = Friends.RemoveFriend(friendId, SocialEdgePlayer.PlayerId);
                    result.status = status;
                }

                if (data.ContainsKey("opJson") && data["opJson"] != null)
                {
                    string subOpData = data["opJson"].ToString();
                    FriendsSubOp friendsSubOp = BsonSerializer.Deserialize<FriendsSubOp>(subOpData);
                    if (friendsSubOp.subOp == SubOpType.REMOVE_RECENT)
                    {
                        foreach (var id in friendsSubOp.friendIds)
                        {
                            if (SocialEdgePlayer.PlayerModel.Friends.friends[id].friendType == "COMMNUNITY")
                            {
                                SocialEdgePlayer.PlayerModel.DBOpRemoveFriend(id);
                                Friends.RemoveFriend(id, SocialEdgePlayer.PlayerId);
                            }
                            else
                            {
                                SocialEdgePlayer.PlayerModel.Friends.friends[id].flagMask =
                                        SocialEdgePlayer.PlayerModel.Friends.friends[id].flagMask & ~FriendFlagMask.RECENT_PLAYED;
                            }
                        }
                    }
                    if (friendsSubOp.subOp == SubOpType.REMOVE)
                    {
                        foreach (var id in friendsSubOp.friendIds)
                        {
                            SocialEdgePlayer.PlayerModel.DBOpRemoveFriend(friendId);
                            Friends.RemoveFriend(friendId, SocialEdgePlayer.PlayerId);
                        }
                    }

                    result.status = true;
                }
            }
            else if (op == "block")
            {
                FriendInfo friend = SocialEdgePlayer.Friends.Find(s => s.FriendPlayFabId.Equals(friendId));
                bool status = Friends.RemoveFriend(friendId, SocialEdgePlayer.PlayerId);
                SocialEdgePlayer.PlayerModel.DBOpBlockFriend(friendId, friend.TitleDisplayName);
                result.friendId = friendId;
                result.status = status;
            }
            else if (op == "unblock")
            {
                SocialEdgePlayer.PlayerModel.DBOpUnblockFriend(friendId);
                result.friendId = friendId;
                result.status = true;
            }
            else if (op == "search")
            {
                string matchString = data["friendId"].ToString();
                int skip = int.Parse(data["skip"].ToString());
                int searchMaxPage = 10;

                List<string> excludeIds = new List<string>();
                excludeIds.Add(SocialEdgePlayer.PlayerDBId);
                foreach (var block in SocialEdgePlayer.PlayerModel.Blocked.blocked)
                    excludeIds.Add(Utils.DbIdFromPlayerId(block.Key));

                List<PlayerSearchDataModelDocument> list = PlayerSearch.Search(matchString, skip, searchMaxPage, excludeIds);
                result.searchList = list;
                result.status = true;
                result.skip = list.Count;
            }
            else if (op == "matchinvite")
            {
                string opponentId = data["opponentId"].ToString();
                string actionCode = data["actionCode"].ToString();
                string challengerDisplayName = data["challengerDisplayName"].ToString();

                Dictionary<string, string> msgDict = new Dictionary<string, string>()
                {
                    {"Challenge1", " wants a 1 minute game."},
                    {"Challenge3", " wants a 3 minute game."},
                    {"Challenge",  " wants a 5 minute game."},
                    {"Challenge10", " wants a 10 minute game."},
                    {"Challenge30", " wants a 30 minute game."}
                };
                string msg = msgDict.ContainsKey(actionCode) ? msgDict[actionCode] : string.Empty;

                if (msg != string.Empty)
                {
                    MatchInviteMessageData msgData = new MatchInviteMessageData();
                    msgData.creationTimestamp = Utils.UTCNow();
                    msgData.senderMiniProfile = SocialEdgePlayer.MiniProfile;
                    msgData.senderPlayerId = SocialEdgePlayer.PlayerId;
                    msgData.title = challengerDisplayName + msg;
                    msgData.body = "Make your move.";
                    msgData.actionCode = actionCode;

                    new SocialEdgeMessage(SocialEdgePlayer.PlayerId, msgData, nameof(MatchInviteMessageData), opponentId).Send();
                }
            }

            CacheFlush();
            return result;
        }
    }
}
