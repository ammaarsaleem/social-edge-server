/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using MongoDB.Bson;
using MongoDB.Bson.IO;
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
    public class Message
    {
        public string msgType;
        public string senderUserId;
        public object msgData;   
    }

    public class SocialEdgeMessageContext
    {
        private string _playerId;
        private List<string> _toPlayerIds;

        public SocialEdgeMessageContext(string playerId)
        {
            _playerId = playerId;
            _toPlayerIds = new List<string>();
        }

        public Message Create()
        {
            Message message = new Message();
            message.msgType = "UserMessage";
            message.senderUserId = _playerId;
            return message;
        }

        public void SetToPlayerIds(List<string> toPlayerIds)
        {
            _toPlayerIds = toPlayerIds;
        }

        public void Send(Message message)
        {
            if (_toPlayerIds.Count == 1)
                SocialEdge.MessageService.Send(_toPlayerIds[0], message);
            else
                SocialEdge.MessageService.Send(_toPlayerIds, message);
        }
    }
}
