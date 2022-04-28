using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using PlayFab.Samples;
using System.Net;
using SocialEdge.Server.Common.Utils;
using System.Collections.Generic;
using PlayFab.DataModels;
using SocialEdge.Server.Api;
using PlayFab;

namespace SocialEdge.Server.Requests
{
    public static class ClaimReward
    {
        [FunctionName("ClaimReward")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            var data = args["data"];

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
