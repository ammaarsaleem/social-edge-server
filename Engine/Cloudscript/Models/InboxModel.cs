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
        public static async Task<InboxDataDocument> Get(string inboxId)
        {
            return !string.IsNullOrEmpty(inboxId) ? await SocialEdge.DataService.GetCollection<InboxDataDocument>("inbox").FindOneById(inboxId) : null;
        }

        public static bool Init(string inboxId)
        {
            var taskT = Set(inboxId, new Dictionary<string, InboxDataMessage>());
            taskT.Wait();
            return taskT.Result.ModifiedCount != 0;
        }
        public static async Task<DataService.UpdateResult> Set(string inboxId, Dictionary<string, InboxDataMessage> inbox)
        {  
            var collection = SocialEdge.DataService.GetCollection<InboxDataDocument>("inbox");
            var taskT = await collection.UpdateOneById<Dictionary<string, InboxDataMessage>>(inboxId, "InboxData", inbox, true);
            return taskT;
        }

        public static int Count(SocialEdgePlayerContext socialEdgePlayer)
        {
            var inbox = socialEdgePlayer.Inbox;
            if (inbox == null) 
                return 0;
            
            long now = Utils.UTCNow();
            int count = 0;
            foreach (var item in inbox)
            {
                InboxDataMessage message = item.Value;
                count += now >= message.startTime ? 1 : 0;
            }

            return count;
        }

        public static void Add(InboxDataMessage message, SocialEdgePlayerContext socialEdgePlayer)
        {
            socialEdgePlayer.Inbox.Add(message.msgId, message);
            socialEdgePlayer.SetDirtyBit(CachePlayerDataSegments.INBOX);
        }

        public static bool Update(string msgId, InboxDataMessage message, SocialEdgePlayerContext socialEdgePlayer)
        {
            var messages = socialEdgePlayer.Inbox;
            message.msgId = msgId;
            messages[msgId] = message;
            socialEdgePlayer.SetDirtyBit(CachePlayerDataSegments.INBOX);
            return true;
        }

        public static bool Del(string msgId, SocialEdgePlayerContext socialEdgePlayer)
        {
            bool isExists = socialEdgePlayer.Inbox.ContainsKey(msgId);
            if (isExists) 
            {
                socialEdgePlayer.Inbox.Remove(msgId);
                socialEdgePlayer.SetDirtyBit(CachePlayerDataSegments.INBOX);
            }
            return isExists;
        }

        public static string FindOne(string msgType, SocialEdgePlayerContext socialEdgePlayer)
        {
            Dictionary<string, InboxDataMessage> messages = socialEdgePlayer.Inbox;
            var it = messages.GetEnumerator();
            while (it.MoveNext() && !(messages[it.Current.Key].type == msgType));
            return it.Current.Key != null ? messages[it.Current.Key].msgId.ToString() : null;
        }

        public static List<string> FindAll(string msgType, SocialEdgePlayerContext socialEdgePlayer)
        {
            Dictionary<string, InboxDataMessage> messages = socialEdgePlayer.Inbox;
            List<string> list = new List<string>();
            var it = messages.GetEnumerator();
            while (it.MoveNext())
            {
                if (messages[it.Current.Key].type == msgType)
                    list.Add(it.Current.Key.ToString());
            }

            return list;
        }
    }
}