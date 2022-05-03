
using SocialEdge.Server.Common.Utils;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace SocialEdge.Server.Models
{
    public static class InboxModel
    {
        public static async Task<BsonDocument> Get(string inboxId)
        {
            return await SocialEdgeEnvironment.DataService.GetCollection("inbox").FindOneById(inboxId);
        }

        public static async Task<SocialEdge.Server.DataService.UpdateResult> Set(string inboxId, BsonDocument inbox)
        {  
            return await SocialEdgeEnvironment.DataService.GetCollection("inbox").ReplaceOneById(inboxId, inbox, true);
        }

        public static int Count(BsonDocument inbox)
        {
            long now = UtilFunc.UTCNow();
            int count = 0;
            var messages = inbox["inboxData"]["messages"].AsBsonDocument;
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
            var messages = inbox["inboxData"]["messages"].AsBsonDocument;
            BsonDocument msg = new BsonDocument() { [message["id"].ToString()] = message };
            messages.AddRange(msg);
        }

        public static bool Update(BsonDocument inbox, string msgId, BsonDocument message)
        {
            var messages = inbox["inboxData"]["messages"].AsBsonDocument;
            message["id"] = msgId;
            messages[msgId] = message;
            return true;
        }

        public static bool Del(BsonDocument inbox, string msgId)
        {
            var messages = inbox["inboxData"]["messages"].AsBsonDocument;
            bool isExists = messages.GetValue(msgId, null) != null;
            if (isExists) messages.Remove(msgId);
            return isExists;
        }
    }
}