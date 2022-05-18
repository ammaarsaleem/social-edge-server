/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using PlayFab.Samples;

namespace SocialEdgeSDK.Server.Requests
{
    public class FriendsOpResult
    {
        bool status;
    }

    public class FriendsOp : FunctionContext
    {
        public FriendsOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("FriendsOp")]
        public async Task<FriendsOpResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            BsonDocument args = BsonDocument.Parse(Args);

            FriendsOpResult friendsOpResult = new FriendsOpResult();

            SocialEdgePlayer.CacheFlush();
            return friendsOpResult;
        }
    }
}
