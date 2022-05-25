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
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class Ping : FunctionContext
    {
        public Ping(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("Ping")]
        public async Task<PingResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            // log.LogInformation("C# HTTP trigger function processed a request.");
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            PingResult result = new PingResult();
            result.clientSendTimestamp = (long)Args["clientSendTimestamp"].Value;
            result.serverReceiptTimestamp = Utils.UTCNow();
            return result;
        }
    }
}
