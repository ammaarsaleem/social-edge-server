/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Context;

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

        public static bool IncreasePublicStats(string friendId, int resultCode, SocialEdgePlayerContext socialEdgePlayer)
        {
            const int RESULT_CODE_WIN = 0;
            const int RESULT_CODE_LOSE = 1;
            const int RESULT_CODE_DRAW = 2;
            const int RESULT_CODE_FRIENDLY = 3;

            const int WIN_IDX = 0;
            const int LOSE_IDX = 1;
            const int DRAW_IDX = 2;
            const int FRIENDLY_IDX = 3;

            const int TAG_STATS_IX = 3;

            FriendInfo friend = socialEdgePlayer.Friends.Find(s => s.FriendPlayFabId.Equals(friendId));
            string[] stats = friend.Tags[TAG_STATS_IX].Split(',');

            // win
            if (resultCode == RESULT_CODE_WIN)
            {
                string update = (Int32.Parse(stats[WIN_IDX]) + 1).ToString();
                friend.Tags[TAG_STATS_IX] =  update + "," + stats[LOSE_IDX] + "," + stats[DRAW_IDX] + "," + stats[FRIENDLY_IDX];
            }
            // lose
            else if (resultCode == RESULT_CODE_LOSE)
            {
                string update = (Int32.Parse(stats[LOSE_IDX]) + 1).ToString();
                friend.Tags[TAG_STATS_IX] = stats[WIN_IDX] + "," + update + "," + stats[DRAW_IDX] + "," + stats[FRIENDLY_IDX];
            }
            // draw
            else if (resultCode == RESULT_CODE_DRAW)
            {
                string update = (Int32.Parse(stats[DRAW_IDX]) + 1).ToString();
                friend.Tags[TAG_STATS_IX] = stats[WIN_IDX] + "," + stats[LOSE_IDX] + "," + update + "," + stats[FRIENDLY_IDX];
            }
            // friendly
            else if (resultCode == RESULT_CODE_FRIENDLY)
            {
                string update = (Int32.Parse(stats[FRIENDLY_IDX]) + 1).ToString();
                friend.Tags[TAG_STATS_IX] = stats[WIN_IDX] + "," + stats[LOSE_IDX] + "," + stats[DRAW_IDX] + "," + update;
            }
            
            return Friends.SetFriendTags(friendId, friend.Tags, socialEdgePlayer.PlayerId);
        }
    }
}