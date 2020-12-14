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
using Microsoft.AspNetCore.Mvc;

namespace SocialEdge.Playfab.Photon
{
    public class GameCreate
    {
        [FunctionName("GameCreate")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            // Get request body
            GameCreateRequest body = await req.Content.ReadAsAsync<GameCreateRequest>();

            // Set name to query string or body data
            string message;
            if (!Utils.IsGameValid(body, out message))
            {
                var errorResponse = new { 
                    ResultCode = 1,
                    Error = message
                };

                return new OkObjectResult(errorResponse);
            }

            var okMsg = $"{req.RequestUri} - Room Created";
            log.LogInformation($"{okMsg} :: {JsonConvert.SerializeObject(body)}");
            
            var response = new { 
                ResultCode = 0,
                Message = "Success"
            };
            
            return new OkObjectResult(response);
        }
    }

    public class GameJoin
    {
        [FunctionName("GameJoin")]
        public async Task<OkObjectResult> Run(
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

            var response = new { 
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }
    }

    public class GameClose
    {
        [FunctionName("GameClose")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            // Get request body
            GameCloseRequest body = await req.Content.ReadAsAsync<GameCloseRequest>();
    
            // Set name to query string or body data
            string message;
            var okMsg = $"{req.RequestUri} - Closed Game - {body.GameId}";
            if (!Utils.IsGameValid(body, out message))
            {
                var errorResponse = new { 
                    ResultCode = 1,
                    Error = message
                };

                return new OkObjectResult(errorResponse);
            }

            var state = (string)JsonConvert.SerializeObject(body.State);
            log.LogInformation(okMsg + " - State: " + state);
            
            var response = new { 
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }
    }

    public class GameEvent
    {
        [FunctionName("GameEvent")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            var response = new { 
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }
    }

    public class GameProperties
    {
        [FunctionName("GameProperties")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            var response = new { 
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }
    }

    public class GameLeave
    {
        [FunctionName("GameLeave")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

            var response = new { 
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }
    }
}
