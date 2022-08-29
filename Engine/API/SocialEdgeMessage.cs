/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Api
{
    public class MatchInviteMessageData
    {
        public string title; 
        public string body; 
        public string senderPlayerId; 
        public string actionCode;
        public PlayerMiniProfileData senderMiniProfile;
        public long creationTimestamp;
    }

    public class SocialEdgeMessage
    {
        public string msgType;
        public string senderPlayerId;
        public string msgDataType;
        public string msgData; 

        private List<string> toPlayerIds;

        public SocialEdgeMessage(string senderPlayerId, object msgData, string msgDataType, params string[] toPlayerIds)
        {
            this.msgType = "UserMessage";
            this.senderPlayerId = senderPlayerId;
            this.toPlayerIds = toPlayerIds.ToList();
            this.msgData = msgData.ToJson();
            this.msgDataType = msgDataType;
        }

        public void Send()
        {
            if (toPlayerIds.Count == 1)
                SocialEdge.MessageService.Send(toPlayerIds[0], this);
            else
                SocialEdge.MessageService.Send(toPlayerIds, this);
        }
    }
}
