/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Collections.Generic;

namespace SocialEdgeSDK.Server.Models
{
    public static class ChatModel
    {
        public static async Task<BsonDocument> Get(string chatId)
        {
            return await SocialEdge.DataService.GetCollection("chat").FindOneById(chatId);
        }

        public static async Task<DataService.UpdateResult> Set(string chatId, BsonDocument chat)
        {  
            BsonDocument inboxData = new BsonDocument() { ["ChatData"] = chat };
            return await SocialEdge.DataService.GetCollection("chat").ReplaceOneById(chatId, inboxData, true);
        }
   }
}