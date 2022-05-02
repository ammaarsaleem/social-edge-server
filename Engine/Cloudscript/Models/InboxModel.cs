
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
    }
    
}