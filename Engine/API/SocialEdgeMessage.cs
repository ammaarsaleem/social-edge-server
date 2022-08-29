/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using MongoDB.Bson;
using MongoDB.Bson.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PlayFab.ProfilesModels;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;
using PlayFab.Samples;
using SocialEdgeSDK.Server.MessageService;

namespace SocialEdgeSDK.Server.Context
{
    public class SocialEdgeMessage
    {
        public string msgType;
        public string senderPlayerId;
        public object msgData; 

        private List<string> toPlayerIds;

        public SocialEdgeMessage(string senderPlayerId, object msgData, params string[] toPlayerIds)
        {
            this.msgType = "UserMessage";
            this.senderPlayerId = senderPlayerId;
            this.toPlayerIds = toPlayerIds.ToList();
            this.msgData = msgData;
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
