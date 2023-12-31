/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.Samples;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;

namespace SocialEdgeSDK.Server.Requests
{
    public class InboxOpResult
    {
        public int count;
        public string inbox;
        public Dictionary<string, int> collect;
        public string messageId;
    }

    public class InboxOp : FunctionContext
    {
        public InboxOp(ITitleContext titleContext) { Base(titleContext); }

        [FunctionName("InboxOp")]
        public InboxOpResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            var op = data["op"];

            var result = new InboxOpResult();

            if (op == "get")
            {
                result.inbox = SocialEdgePlayer.InboxJson;
            }
            else if (op == "collect")
            {
                result.messageId = data["messageId"].ToString();
                result.collect = Inbox.Collect(data["messageId"].ToString(), SocialEdgePlayer);
            }

            result.count = InboxModel.Count(SocialEdgePlayer);
            SocialEdgePlayer.CacheFlush();

            return result;
        }
    }
}

