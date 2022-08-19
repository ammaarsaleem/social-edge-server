/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Api
{
    public static class FriendFlagMask
    {
        public const int RECENT_PLAYED = 0x1;
    }

    public static class Friends
    {
        public static bool AddFriend(string friendId, string playerId)
        {
            bool status = false;
            var requestAddFriend = new AddFriendRequest();
            requestAddFriend.FriendPlayFabId = friendId;
            requestAddFriend.PlayFabId = playerId;
            var requestAddFriendT = PlayFabServerAPI.AddFriendAsync(requestAddFriend);
            requestAddFriendT.Wait();
            status = requestAddFriendT.Result.Error == null;
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

        public static FriendData UpdateFriendsMatchTimestamp(string friendId, SocialEdgePlayerContext socialEdgePlayer)
        {
            if (!socialEdgePlayer.PlayerModel.Friends.friends.ContainsKey(friendId))
                return null;

            FriendData friendData = socialEdgePlayer.PlayerModel.Friends.friends[friendId];
            friendData.lastMatchTimestamp = Utils.UTCNow();
            friendData.flagMask = friendData.flagMask | FriendFlagMask.RECENT_PLAYED;
            return friendData;
        }
    }
}