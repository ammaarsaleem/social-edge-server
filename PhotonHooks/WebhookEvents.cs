using System;
using PlayFab;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Net.Http.Formatting;
using SocialEdge.Server.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;

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
                
                var errorResponse = new { 
                    ResultCode = 1,
                    Error = error
                };

                // string errorResponse = "{ \"ResultCode\" : 1, \"Error\" : " + error + " }";
                return req.CreateResponse(
                    HttpStatusCode.OK,
                    errorResponse,
                    JsonMediaTypeFormatter.DefaultMediaType
                );
            }

            var okMsg = $"{req.RequestUri} - Room Created";
            log.LogInformation($"{okMsg} :: {JsonConvert.SerializeObject(body)}");
            
            var response = new { 
                    ResultCode = 0,
                    Message = "OK"
                };
            // string response = "{ \"ResultCode\" : 0, \"Message\" : \"OK\" }";
            return req.CreateResponse(
                HttpStatusCode.OK,
                response,
                JsonMediaTypeFormatter.DefaultMediaType
            );
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

            string response = "{ \"ResultCode\" : 0, \"Message\" : \"OK\" }";
            return req.CreateResponse(response);
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

            string response = "{ \"ResultCode\" : 0, \"Message\" : \"OK\" }";
            return req.CreateResponse(response);
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

            string response = "{ \"ResultCode\" : 0, \"Message\" : \"OK\" }";
            return req.CreateResponse(response);
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

            string response = "{ \"ResultCode\" : 0, \"Message\" : \"OK\" }";
            return req.CreateResponse(response);
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

            string response = "{ \"ResultCode\" : 0, \"Message\" : \"OK\" }";
            return req.CreateResponse(response);
        }
    }
}
