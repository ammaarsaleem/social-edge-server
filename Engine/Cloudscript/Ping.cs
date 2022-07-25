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
            try
            {
                PingResult result = new PingResult();
                result.clientSendTimestamp = (long)Args["data"]["clientSendTimestamp"].Value;
                result.serverReceiptTimestamp = Utils.UTCNow();

                // Dictionary<string, string> titleData = SocialEdge.TitleContext.TitleData.Data;
                // var testing = titleData["Testing"];
                // SocialEdge.Log.LogInformation("testing RESULT : " + testing.ToJson());

                return result;
            }
            catch (Exception e)
            {
                if(e is OperationCanceledException){
                     log.LogWarning("Function cancelled " + e.Message);
                      return new PingResult();
                }
                else{
                    throw e;
                }
            }
        }
    }
}
