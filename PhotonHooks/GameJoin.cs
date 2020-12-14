using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocialEdge.Server.Util;
using System.Net.Http;
using PlayFab.Samples;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdge.Server.Constants;

namespace SocialEdge.Playfab
{


    // {
    //   "authLevel": "function",
    //   "name": "req",
    //   "type": "httpTrigger",
    //   "direction": "in",
    //   "methods": [ "post" ],
    //   "route": "{appId}/GameJoin/{id:int?}"

    // },
    // {
    //   "name": "res",
    //   "type": "http",
    //   "direction": "out"
    // }




    public class GameJoin
    {
        [FunctionName("GameJoin")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            // [HttpTrigger(AuthorizationLevel.Function, "post")]
            // HttpRequestMessage req, ILogger log)
            HttpRequestMessage req, ILogger log)
        {

            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            // Get request body
            GameLeaveRequest body = await req.Content.ReadAsAsync<GameLeaveRequest>();

            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
            WriteTitleEventRequest titleEventRequest = new WriteTitleEventRequest();
            titleEventRequest.EventName = "room_joined";
            titleEventRequest.Body = new System.Collections.Generic.Dictionary<string, object> {
                { "RequestBody", JsonConvert.SerializeObject(body) }
            };
            await PlayFabServerAPI.WriteTitleEventAsync(titleEventRequest);

            var okMsg = $"{req.RequestUri} - Recieved Game Join Request";
            log.LogInformation(okMsg);

            return req.CreateResponse(HttpStatusCode.OK, okMsg);
        }
    }

    public class GameCreate
    {
        [FunctionName("GameCreate")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            // [HttpTrigger(AuthorizationLevel.Function, "post")]
            // HttpRequestMessage req, ILogger log)
            HttpRequestMessage req, ILogger log)
        {

            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");
        }
    }

    public class GameClose
    {
        [FunctionName("GameClose")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            // [HttpTrigger(AuthorizationLevel.Function, "post")]
            // HttpRequestMessage req, ILogger log)
            HttpRequestMessage req, ILogger log)
        {

            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");
        }
    }

    public class GameEvent
    {
        [FunctionName("GameEvent")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            // [HttpTrigger(AuthorizationLevel.Function, "post")]
            // HttpRequestMessage req, ILogger log)
            HttpRequestMessage req, ILogger log)
        {

            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");
        }
    }

    public class GameProperties
    {
        [FunctionName("GameProperties")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            // [HttpTrigger(AuthorizationLevel.Function, "post")]
            // HttpRequestMessage req, ILogger log)
            HttpRequestMessage req, ILogger log)
        {

            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");
        }
    }

    public class GameLeave
    {
        [FunctionName("GameLeave")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            // [HttpTrigger(AuthorizationLevel.Function, "post")]
            // HttpRequestMessage req, ILogger log)
            HttpRequestMessage req, ILogger log)
        {

            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");
        }
    }
}

