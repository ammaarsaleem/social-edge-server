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

namespace SocialEdgeSDK.Playfab
{
    public class Playstream_OnPlayerCreated : FunctionContext
    {
        [FunctionName("Playstream_OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext(req, log);
            Player.NewPlayerInit(SocialEdgePlayer.PlayerId, SocialEdgePlayer.EntityToken, SocialEdgePlayer.EntityId);
            SocialEdge.Log.LogInformation("New Player Init: " + SocialEdgePlayer.PlayerId);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
