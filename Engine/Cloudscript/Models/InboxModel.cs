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
    public static class InboxModel
    {
        public static async Task<BsonDocument> Get(string inboxId)
        {
            return await SocialEdge.DataService.GetCollection("inbox").FindOneById(inboxId);
        }

        public static async Task<DataService.UpdateResult> Set(string inboxId, BsonDocument inbox)
        {  
            BsonDocument inboxData = new BsonDocument() { ["inboxData"] = inbox };
            return await SocialEdge.DataService.GetCollection("inbox").ReplaceOneById(inboxId, inboxData, true);
        }

        public static int Count(BsonDocument inbox)
        {
            long now = Utils.UTCNow();
            int count = 0;
            var messages = inbox["messages"].AsBsonDocument;
            foreach (string key in messages.Names)
            {
                var msg = messages[key].AsBsonDocument;
                //SocialEdgeEnvironment.Log.LogInformation(msg.ToString());
                count += (msg["startTime"] == null || now >= msg["startTime"]) ? 1 : 0;
            }

            return count;
        }

        public static void Add(BsonDocument inbox, BsonDocument message)
        {
            var messages = inbox["messages"].AsBsonDocument;
            BsonDocument msg = new BsonDocument() { [message["id"].ToString()] = message };
            messages.AddRange(msg);
        }

        public static bool Update(BsonDocument inbox, string msgId, BsonDocument message)
        {
            var messages = inbox["messages"].AsBsonDocument;
            message["id"] = msgId;
            messages[msgId] = message;
            return true;
        }

        public static bool Del(BsonDocument inbox, string msgId)
        {
            var messages = inbox["messages"].AsBsonDocument;
            bool isExists = messages.GetValue(msgId, null) != null;
            if (isExists) messages.Remove(msgId);
            return isExists;
        }

        public static string FindOne(BsonDocument inbox, string msgType)
        {
            var messages = inbox["messages"].AsBsonDocument;
            var it = messages.GetEnumerator();
            while (it.MoveNext() && !(messages[it.Current.Name]["type"] == msgType));
            return it.Current.Name != null ? messages[it.Current.Name]["id"].ToString() : null;
        }

        public static List<string> FindAll(BsonDocument inbox, string msgType)
        {
            List<string> list = new List<string>();
            var messages = inbox["messages"].AsBsonDocument;
            var it = messages.GetEnumerator();
            while (it.MoveNext())
            {
                if (messages[it.Current.Name]["type"] == msgType)
                {
                    list.Add(it.Current.Name.ToString());
                }
            }

            return list;
        }
    }
}