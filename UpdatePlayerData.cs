
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using System.Net;
using SocialEdge.Server.Common.Utils;

namespace SocialEdge.Server.Requests
{
    public static class UpdatePlayerData
    {
        [FunctionName("UpdatePlayerData")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
