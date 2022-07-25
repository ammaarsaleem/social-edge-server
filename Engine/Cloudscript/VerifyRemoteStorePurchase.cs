/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class VerifyRemoteStorePurchase : FunctionContext
    {
       // IDataService _dataService;
        public VerifyRemoteStorePurchase(ITitleContext titleContext, IDataService dataService) {Base(titleContext, dataService); }

        [FunctionName("VerifyRemoteStorePurchase")]
        public RemotePurchaseResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                InitContext<FunctionExecutionContext<dynamic>>(req, log);
                //var sharedSecret = "39c303891fcf4e9ca7f00f144e78d1e0";
                var data = Args["data"];
                string  remoteProductId = data["itemId"].Value;
                String instanceId = data["instanceId"].Value;
                double expiryTime = data["expiryTimeStamp"].Value;
                String subscriptionType = data["subscriptionType"].Value;

                try
                {
                     RemotePurchaseResult result = new RemotePurchaseResult();
                     PlayFab.ServerModels.CatalogItem purchaseItem =  SocialEdge.TitleContext.GetCatalogItem(remoteProductId);
                     if(purchaseItem == null)
                     {
                             result.responseCode = 1;
                             result.responseMessage = "Item Not found :" + remoteProductId;
                             return result;
                     }

                   log.LogInformation("VerifyRemoteStorePurchase : called " + result.ToString());
                    return result;
                }
                catch (Exception e)
                {
                    throw e;
                }

            }
    }
}
