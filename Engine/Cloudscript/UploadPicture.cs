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
    public class UploadPicture : FunctionContext
    {
         public UploadPicture(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("UploadPicture")]
        public async Task<bool> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            bool uploaded = false;
            InitContext<FunctionExecutionContext<dynamic>>(req, log);

            try
            {
                string key = Args["key"].ToString();
                string contentType = Args["contentType"].ToString();
                byte[] content = Args["content"].ToObject<byte[]>();
                if(!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(contentType))
                {
                    var blobStorage = SocialEdge.DataService.GetBlobStorage(Constants.Constant.CONTAINER_PLAYER_PROFILE);
                    await blobStorage.Save(key, content);

                    // test
                    var uri = blobStorage.GetServiceSasUriForBlob(key, 2);
                    var uriStr = uri.ToString();
                }


                return uploaded;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

