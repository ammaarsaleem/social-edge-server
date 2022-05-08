/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using System.Net.Http;
using Newtonsoft.Json;
using SocialEdgeSDK.Server.Context;
using PlayFab.Samples;
using PlayFab;

namespace SocialEdgeSDK.Server.Requests
{
    public class DownloadPicture
    {
        [FunctionName("DownloadPicture")]
        public async Task<byte[]> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            byte[] profilePic = null;
            SocialEdge.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            try
            {
                string key = args["key"].ToString();
                string contentType = args["contentType"].ToString();
                byte[] content = args["content"];
                if(!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(contentType))
                {
                    PlayFabResult<GetContentDownloadUrlResult> downloadUrlResult = await GetDownloadUrl(key);
                    if(downloadUrlResult.Error==null && !string.IsNullOrEmpty(downloadUrlResult.Result.URL))
                    {
                        profilePic = await GetFile(downloadUrlResult.Result.URL);
                    }
                }

                return profilePic;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<PlayFabResult<GetContentDownloadUrlResult>> GetDownloadUrl(string key)
        {
            var request = new GetContentDownloadUrlRequest
            {
                Key = key,
                ThruCDN = true
            };

            PlayFabResult<GetContentDownloadUrlResult> result= await PlayFabServerAPI.GetContentDownloadUrlAsync(request);
            return result;
            
        }

        private async Task<byte[]> GetFile(string presignedUrl)
        {
            try{
                using(HttpClient client = new HttpClient())
                {
                    
                    HttpResponseMessage result = await client.GetAsync(presignedUrl);
                    byte[] pic = await result.Content.ReadAsByteArrayAsync();

                    return pic;
                }
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}

