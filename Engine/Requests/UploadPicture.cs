using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using SocialEdge.Server.Common.Utils;
using PlayFab.Samples;
using PlayFab.AdminModels;
using PlayFab;

namespace SocialEdge.Server.Requests
{
    public class UploadPicture
    {
        [FunctionName("UploadPicture")]
        public async Task<bool> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            bool uploaded = false; 
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            try
            {
                string key = args["key"].ToString();
                string contentType = args["contentType"].ToString();
                byte[] content = args["content"];
                if(!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(contentType))
                {
                    PlayFabResult<GetContentUploadUrlResult> uploadUrlResult = await GetUploadUrl(key,contentType);
                    if(uploadUrlResult.Error==null && !string.IsNullOrEmpty(uploadUrlResult.Result.URL))
                    {
                        uploaded = await PutFile(uploadUrlResult.Result.URL, content, contentType);
                    }
                }

                return uploaded;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<PlayFabResult<GetContentUploadUrlResult>> GetUploadUrl(string key, string contentType)
        {
            var request = new PlayFab.AdminModels.GetContentUploadUrlRequest
            {
                ContentType = contentType,
                Key = key
            };

            PlayFabResult<GetContentUploadUrlResult> result= await PlayFabAdminAPI.GetContentUploadUrlAsync(request);
            return result;
            
        }

        private async Task<bool> PutFile(string presignedUrl, byte[] content, string contentType)
        {
            try{
                using(HttpClient client = new HttpClient())
                {
                    HttpContent hc = new ByteArrayContent(content);
                    hc.Headers.ContentType= new MediaTypeHeaderValue("binary/octet-stream");
                
                    await client.PutAsync(new Uri(presignedUrl), hc);
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }
    }
}

