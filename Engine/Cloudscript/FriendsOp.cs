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

    public class FriendsOp : FunctionContext
    {
        public FriendsOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("FriendsOp")]
        public async Task<FriendsOpResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            FriendsOpResult result = new FriendsOpResult();
            string friendId = data["friendId"].ToString().ToLower();
            string op = result.op = data["op"].ToString();

            if (op == "add" || op == "addFavourite")
            {
                if (!SocialEdgePlayer.PlayerModel.Friends.friends.ContainsKey(friendId))
                {
                    bool status = Friends.AddFriend(friendId, data["friendState"].ToString(), data["friendType"].ToString(), SocialEdgePlayer.PlayerId);
                    if (status == true)
                    {
                        if (!SocialEdgePlayer.PlayerModel.Friends.friends.ContainsKey(friendId))
                        {
                            FriendData friendData = SocialEdgePlayer.PlayerModel.Friends.CreateFriendData();
                            SocialEdgePlayer.PlayerModel.Friends.Add(friendId, friendData);
                        }

                        result.friendId = friendId;
                        result.friendType = data["friendType"].ToString();
                        result.status = true;
                    }
                }
            }
            else if (op == "remove")
            {
                bool status = Friends.RemoveFriend(friendId, SocialEdgePlayer.PlayerId);
                if (status == true)
                {
                    if (SocialEdgePlayer.PlayerModel.Friends.friends.ContainsKey(friendId))
                    {
                        SocialEdgePlayer.PlayerModel.Friends.Remove(friendId);
                    }

                    result.friendId = friendId;
                    result.status = true;
                }
            }
            else if (op == "setstate")
            {
                FriendInfo friend = SocialEdgePlayer.Friends.Find(s => s.FriendPlayFabId.Equals(friendId));
                friend.Tags[0] = data["friendState"].ToString();
                result.status = Friends.SetFriendTags(friendId, friend.Tags, SocialEdgePlayer.PlayerId);
            }
            else if (op == "setfriendtype")
            {
                FriendInfo friend = SocialEdgePlayer.Friends.Find(s => s.FriendPlayFabId.Equals(friendId));
                friend.Tags[2] = data["friendType"].ToString();
                result.status = Friends.SetFriendTags(friendId, friend.Tags, SocialEdgePlayer.PlayerId);
            }
            else if (op == "search")
            {
                string matchString = data["friendId"].ToString();
                int skip = int.Parse(data["skip"].ToString());
                int searchMaxPage = 10;

                List<PlayerSearchDataModelDocument> list = PlayerSearch.Search(matchString, skip, searchMaxPage);
                result.searchList = list;
                result.status = true;
                result.skip = list.Count;
            }

            CacheFlush();
            return result;

        }
    }
}
