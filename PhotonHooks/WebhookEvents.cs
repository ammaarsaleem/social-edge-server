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
using SocialEdge.Playfab;
using PlayFab.ServerModels;
using SocialEdge.Server.Constants;

namespace SocialEdge.Playfab.Photon
{
    public class GameCreate
    {
        [FunctionName("GameCreate")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            // Get request body
            GameCreateRequest body = await req.Content.ReadAsAsync<GameCreateRequest>();
            
            // Set name to query string or body data
            string message;
            if (!Utils.IsGameValid(body, out message))
            {
                string error = $"{req.RequestUri} - {message}";
                log.LogError(error);
                return req.CreateResponse(HttpStatusCode.BadRequest, error);
            }

            var okMsg = $"{req.RequestUri} - Room Created";
            log.LogInformation($"{okMsg} :: {JsonConvert.SerializeObject(body)}");
            return req.CreateResponse(HttpStatusCode.OK, okMsg);
        }
    }

    public class GameJoin
    {
        [FunctionName("GameJoin")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
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

    public class GameClose
    {
        [FunctionName("GameClose")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            return req.CreateResponse(HttpStatusCode.OK, "okMsg");
        }
    }

    public class GameEvent
    {
        [FunctionName("GameEvent")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            return req.CreateResponse(HttpStatusCode.OK, "okMsg");
        }
    }

    public class GameProperties
    {
        [FunctionName("GameProperties")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            return req.CreateResponse(HttpStatusCode.OK, "okMsg");
        }
    }

    public class GameLeave
    {
        [FunctionName("GameLeave")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            return req.CreateResponse(HttpStatusCode.OK, "okMsg");
        }
    }
}
