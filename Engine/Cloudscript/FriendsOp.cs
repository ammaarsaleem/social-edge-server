/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
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
        public FriendsOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("FriendsOp")]
        public FriendsOpResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            FriendsOpResult result = new FriendsOpResult();
            string friendId = data["friendId"] != null ? data["friendId"].ToString() : null;
            string op = result.op = data["op"].ToString();

            if (op == "add" || op == "addFavourite")
            {
                FriendData friendData = SocialEdgePlayer.PlayerModel.CreateFriendData();
                friendData._friendType = data["friendType"].ToString();
                SocialEdgePlayer.PlayerModel.AddFriend(friendId, friendData);
                result.friendId = friendId;
                result.friendType = data["friendType"].ToString();
                bool status = Friends.AddFriend(friendId, SocialEdgePlayer.PlayerId);
                result.status = true;
            }
            else if (op == "remove")
            {
                if (!string.IsNullOrEmpty(friendId))
                {
                    SocialEdgePlayer.PlayerModel.RemoveFriend(friendId);
                    result.friendId = friendId;
                    bool status = Friends.RemoveFriend(friendId, SocialEdgePlayer.PlayerId);
                    result.status = status;
                }

                if (data.ContainsKey("opJson"))
                {
                    string subOpData = data["opJson"].ToString();
                    FriendsSubOp friendsSubOp = BsonSerializer.Deserialize<FriendsSubOp>(subOpData);
                    if (friendsSubOp.subOp == SubOpType.REMOVE_RECENT)
                    {
                        foreach (var id in friendsSubOp.friendIds) 
                        {
                            if (SocialEdgePlayer.PlayerModel.Friends.friends[id].friendType == "COMMNUNITY") 
                            {
                                SocialEdgePlayer.PlayerModel.RemoveFriend(id);
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
                            SocialEdgePlayer.PlayerModel.RemoveFriend(friendId);
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
                SocialEdgePlayer.PlayerModel.BlockFriend(friendId, friend.TitleDisplayName);
                result.friendId = friendId;
                result.status = status;
            }
            else if (op == "unblock")
            {
                SocialEdgePlayer.PlayerModel.UnblockFriend(friendId);
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
                foreach(var block in SocialEdgePlayer.PlayerModel.Blocked.blocked)
                    excludeIds.Add(Utils.DbIdFromPlayerId(block.Key));

                List<PlayerSearchDataModelDocument> list = PlayerSearch.Search(matchString, skip, searchMaxPage, excludeIds);
                result.searchList = list;
                result.status = true;
                result.skip = list.Count;
            }

            CacheFlush();
            return result;
        }
    }
}
