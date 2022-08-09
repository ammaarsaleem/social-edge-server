/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using PlayFab.Samples;

namespace SocialEdgeSDK.Server.Requests
{
    public class GetDailyEvent: FunctionContext
    {
        public GetDailyEvent(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("GetDailyEvent")]
        public PlayerDataEvent Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            SocialEdgePlayer.PlayerEconomy.ProcessDailyEvent();
            CacheFlush();
            return SocialEdgePlayer.PlayerModel.Events;
        }
    }
}