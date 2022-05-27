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
            return !string.IsNullOrEmpty(inboxId) ? await SocialEdge.DataService.GetCollection("inbox").FindOneById(inboxId) : null;
        }

        public static string Init()
        {
            var collection = SocialEdge.DataService.GetCollection("inbox");
            BsonDocument container = new BsonDocument() { ["inboxData"] = new BsonDocument() { ["messages"] = new BsonDocument(){} } };
            collection.InsertOne(container);
            return container.Contains("_id") ? container["_id"].ToString() : null;
        }

        public static async Task<DataService.UpdateResult> Set(string inboxId, BsonDocument inbox)
        {  
            BsonDocument inboxData = new BsonDocument() { ["inboxData"] = inbox };
            return await SocialEdge.DataService.GetCollection("inbox").ReplaceOneById(inboxId, inboxData, true);
        }

        public static int Count(SocialEdgePlayerContext socialEdgePlayer)
        {
            var inbox = socialEdgePlayer.Inbox;
            if (inbox == null) return 0;
            
            long now = Utils.UTCNow();
            int count = 0;
            var messages = inbox["messages"].AsBsonDocument;
            foreach (string key in messages.Names)
            {
                var msg = messages[key].AsBsonDocument;
                //SocialEdgeEnvironment.Log.LogInformation(msg.ToString());
                count += (msg.Contains("startTime") == false || now >= msg["startTime"]) ? 1 : 0;
            }

            return count;
        }

        public static void Add(BsonDocument message, SocialEdgePlayerContext socialEdgePlayer)
        {
            var messages = socialEdgePlayer.Inbox["messages"].AsBsonDocument;
            BsonDocument msg = new BsonDocument() { [message["id"].ToString()] = message };
            messages.AddRange(msg);
            socialEdgePlayer.SetDirtyBit(CacheSegment.INBOX);
        }

        public static bool Update(string msgId, BsonDocument message, SocialEdgePlayerContext socialEdgePlayer)
        {
            var messages = socialEdgePlayer.Inbox["messages"].AsBsonDocument;
            message["id"] = msgId;
            messages[msgId] = message;
            socialEdgePlayer.SetDirtyBit(CacheSegment.INBOX);
            return true;
        }

        public static bool Del(string msgId, SocialEdgePlayerContext socialEdgePlayer)
        {
            var messages = socialEdgePlayer.Inbox["messages"].AsBsonDocument;
            bool isExists = messages.GetValue(msgId, null) != null;
            if (isExists) 
            {
                messages.Remove(msgId);
                socialEdgePlayer.SetDirtyBit(CacheSegment.INBOX);
            }
            return isExists;
        }

        public static string FindOne(string msgType, SocialEdgePlayerContext socialEdgePlayer)
        {
            var messages = socialEdgePlayer.Inbox["messages"].AsBsonDocument;
            var it = messages.GetEnumerator();
            while (it.MoveNext() && !(messages[it.Current.Name]["type"] == msgType));
            return it.Current.Name != null ? messages[it.Current.Name]["id"].ToString() : null;
        }

        public static List<string> FindAll(string msgType, SocialEdgePlayerContext socialEdgePlayer)
        {
            var messages = socialEdgePlayer.Inbox["messages"].AsBsonDocument;
            List<string> list = new List<string>();
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