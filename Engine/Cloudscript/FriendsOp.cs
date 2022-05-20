/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PlayFab.Samples;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Api;

namespace SocialEdgeSDK.Server.Requests
{
    public class FriendsOpResult
    {
        public bool status;
    }

    public class FriendsOp : FunctionContext
    {
        public FriendsOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("FriendsOp")]
        public async Task<FriendsOpResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            FriendsOpResult friendsOpResult = new FriendsOpResult();
            string friendId = data["friendId"].ToString();
            var op = data["op"];

            if (op == "add")
            {
                friendsOpResult.status = Friends.AddFriend(friendId, data["friendState"].ToString(), data["friendType"].ToString(), SocialEdgePlayer.PlayerId);
            }
            else if (op == "remove")
            {
                friendsOpResult.status = Friends.RemoveFriend(friendId, SocialEdgePlayer.PlayerId);
            }
            else if (op == "setstate")
            {
                FriendInfo friend = SocialEdgePlayer.Friends.Find(s => s.FriendPlayFabId.Equals(friendId));
                friend.Tags[0] = data["friendState"].ToString();
                friendsOpResult.status = Friends.SetFriendTags(friendId, friend.Tags, SocialEdgePlayer.PlayerId);
            }
            else if (op == "setfriendtype")
            {
                FriendInfo friend = SocialEdgePlayer.Friends.Find(s => s.FriendPlayFabId.Equals(friendId));
                friend.Tags[2] = data["friendType"].ToString();
                friendsOpResult.status = Friends.SetFriendTags(friendId, friend.Tags, SocialEdgePlayer.PlayerId);
            }

            SocialEdgePlayer.CacheFlush();
            return friendsOpResult;

        }
    }
}
