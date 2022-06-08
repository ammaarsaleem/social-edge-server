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
using PlayFab.AdminModels;
using PlayFab;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Api;

namespace SocialEdgeSDK.Server.Requests
{
    public class GetSasUrl : FunctionContext
    {
        IDataService _dataService;
        public GetSasUrl(ITitleContext titleContext, IDataService dataService) { _dataService=dataService; Base(titleContext, dataService); }

        [FunctionName("GetSasUrl")]
        public String Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
               // InitContext<FunctionExecutionContext<dynamic>>(req, log);

                try
                {
                    string key = "profilepic7X3Z84";//Args["key"].ToString();
                    var blobStorage = _dataService.GetBlobStorage();

                    // test
                    var uri = blobStorage.GetServiceSasUriForBlob(key, 2);
                    var uriStr = uri.ToString();
                    return uriStr;

                }
                catch (Exception e)
                {
                    throw e;
                }

            }
    }
}
