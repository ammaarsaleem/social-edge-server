/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SocialEdgeSDK.Server.Requests;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.DataService;
using PlayFab.Samples;

namespace SocialEdgeSDK.Playfab
{
    public class Playstream_OnPlayerCreated : FunctionContext
    {
        ITitleContext TITLECONTEXT;
        IDataService DATASERVICE;

        public Playstream_OnPlayerCreated(ITitleContext titleContext, IDataService dataService) { 
            Base(titleContext, dataService); 
            TITLECONTEXT = titleContext;
            DATASERVICE = dataService;
            }

        [FunctionName("Playstream_OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            // InitContext<PlayerPlayStreamFunctionExecutionContext<dynamic>>(req, log);
            // Player.NewPlayerInit(SocialEdgePlayer);
            // SocialEdgePlayer.CacheFlush();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
