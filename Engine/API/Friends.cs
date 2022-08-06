/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using PlayFab;
using PlayFab.ServerModels;

namespace SocialEdgeSDK.Server.Api
{
    public static class Friends
    {
        public static bool AddFriend(string friendId, string state, string friendType, string playerId)
        {
            bool status = false;

            var requestAccountInfo = new GetUserAccountInfoRequest();
            requestAccountInfo.PlayFabId = friendId;
            var requestAccountInfoT = PlayFabServerAPI.GetUserAccountInfoAsync(requestAccountInfo);
            requestAccountInfoT.Wait();
            status = requestAccountInfoT.Result.Error == null;

            if (status == false)
                return status;

            var playertTitleAccountId = requestAccountInfoT.Result.Result.UserInfo.TitleInfo.TitlePlayerAccount.Id;

            var requestAddFriend = new AddFriendRequest();
            requestAddFriend.FriendPlayFabId = friendId;
            requestAddFriend.PlayFabId = playerId;
            var requestAddFriendT = PlayFabServerAPI.AddFriendAsync(requestAddFriend);
            requestAddFriendT.Wait();
            status = requestAddFriendT.Result.Error == null;

            if (status == false)
                return status;

            var requestSetFriendTag = new SetFriendTagsRequest();
            requestSetFriendTag.FriendPlayFabId = friendId;
            requestSetFriendTag.PlayFabId = playerId;
            requestSetFriendTag.Tags = new List<string>() { state, playertTitleAccountId, friendType, "0,0,0,0" };
            var requestSetFriendTagT = PlayFabServerAPI.SetFriendTagsAsync(requestSetFriendTag);
            requestSetFriendTagT.Wait();
            status = requestSetFriendTagT.Result.Error == null;
            // Remove this friend if failed to write tags
            if (status == false)
            {
                RemoveFriend(friendId, playerId);
            }

            return status;
        }

        public static bool RemoveFriend(string friendId, string playerId)
        {
            var request = new RemoveFriendRequest();
            request.FriendPlayFabId = friendId;
            request.PlayFabId = playerId;
            var requestT = PlayFabServerAPI.RemoveFriendAsync(request);
            requestT.Wait();
            return requestT.Result.Error == null;
        }

        public static bool SetFriendTags(string friendId, List<string> tags, string playerId)
        {
            var request = new SetFriendTagsRequest();
            request.FriendPlayFabId = friendId;
            request.PlayFabId = playerId;
            request.Tags = tags;
            var requestT = PlayFabServerAPI.SetFriendTagsAsync(request);
            requestT.Wait();
            return requestT.Result.Error == null;
        }
    }
}