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
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class GetSasUrl : FunctionContext
    {
       // IDataService _dataService;
        public GetSasUrl(ITitleContext titleContext, IDataService dataService) {Base(titleContext, dataService); }

        [FunctionName("GetSasUrl")]
        public String Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                InitContext<FunctionExecutionContext<dynamic>>(req, log);

                var data = Args["data"];
                String op = data["op"].Value;
                String fileName  = data["fileName"].Value;

                try
                {
                    string containerName = Constants.Constant.CONTAINER_PLAYER_PROFILE;

                    if(op == "skinDownload"){
                        
                        containerName = Constants.Constant.CONTAINER_DLC;
                    }
                    else if(op == "picUpload"){

                        fileName = "pic-" + Guid.NewGuid().ToString();
                        SocialEdgePlayer.MiniProfile.UploadPicId = fileName;
                        Player.UpdatePlayerAvatarData(SocialEdgePlayer.PlayerId, SocialEdgePlayer.MiniProfile);
                    }
                 
                    var blobStorage = SocialEdge.DataService.GetBlobStorage(containerName);
                    var uri = blobStorage.GetServiceSasUriForBlob(fileName, 10);
                    var uriStr = uri.ToString();
                    
                    log.LogInformation("GetSasUrl : " + uriStr);
                    return uriStr;
                }
                catch (Exception e)
                {
                    throw new Exception($"An error occured : " + e.Message);
                }

            }
    }
}
