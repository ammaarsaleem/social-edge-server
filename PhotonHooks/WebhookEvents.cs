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
using System.Collections.Generic;
using SocialEdge.Server.Util;
using System.Linq;
namespace SocialEdge.Playfab.Photon
{
    public class GameCreate
    {
        /*photon waits for response*/
        [FunctionName("GameCreate")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            // call util
            RequestUtil.Init(req);

            string activeChallengesData = string.Empty;
            string message = string.Empty;
            List<string> activeChallenges = null;

            GameCreateRequest body = await req.Content.ReadAsAsync<GameCreateRequest>();
            
            var okMsg = $"{req.RequestUri} - Room Created";
            log.LogInformation($"{okMsg} :: {JsonConvert.SerializeObject(body)}");
            
            if(Utils.IsGameValid(body, out message))
            {
                string currentChallengeId = body.GameId;
                string playerId = body.UserId;

                //create shared group 
                var createGroupRequest = new CreateSharedGroupRequest{
                    SharedGroupId = currentChallengeId
                };  
                
                var getPlayerDataRequest = new GetUserDataRequest{
                    PlayFabId = playerId,
                    Keys = new List<string>{"activeChallenges"}
                };

                var playerDataTask =  PlayFabServerAPI.GetUserInternalDataAsync(getPlayerDataRequest);
                var groupTask =  PlayFabServerAPI.CreateSharedGroupAsync(createGroupRequest);
                await Task.WhenAll(playerDataTask,groupTask);

                //update user internal data
                if(groupTask.IsCompletedSuccessfully && playerDataTask.IsCompletedSuccessfully)
                {   
                    log.LogInformation("group created with id: " + groupTask.Result.Result.SharedGroupId);
                    log.LogInformation("player data fetched");
                    if(playerDataTask.Result.Result.Data.ContainsKey("activeChallenges"))
                    {
                        activeChallengesData = playerDataTask.Result.Result.Data["activeChallenges"].Value;
                    }
                    
                    if(!string.IsNullOrEmpty(activeChallengesData)){
                        activeChallenges = JsonConvert.DeserializeObject<List<string>>(activeChallengesData);
                    }
                    else{
                        activeChallenges = new List<string>();
                    }

                    //If current challenge not already added
                    if(!activeChallenges.Any(s=>s.Equals(currentChallengeId)))
                    {
                        activeChallenges.Add(currentChallengeId);
                        Tuple<bool,string> result = await UpdateUserData(activeChallenges,  playerId);
                        var success = result.Item1;
                       
                        if (success)
                        {
                            log.LogInformation("Internal data successfully updated");
                            return Utils.GetSuccessResponse();
                        }
                        else
                        {
                            message = result.Item2;
                            log.LogInformation(message);
                        }
                    }

                    else
                    {
                        message = "this challenge is already active";
                        log.LogInformation(message);
                    }
                }
                else{
                    message = "GroupError: " + groupTask.Result.Error.ErrorMessage + 
                                ", getPlayerDataError: "+ playerDataTask.Result.Error.ErrorMessage;
                    
                    log.LogInformation(message);
                }

                return Utils.GetErrorResponse(message);
            }
            else{
                message = "Game is not valid";
                log.LogInformation(message);
            }

            return Utils.GetErrorResponse(message);
        }

        private async Task<Tuple<bool,string>> UpdateUserData(List<string> activeChallenges, string playerId)
        {
            string errorMessage = string.Empty;
            bool hasUpdated=false;
            string activeChallengesJson = JsonConvert.SerializeObject(activeChallenges);
            var activeChallengesDict = new Dictionary<string, string>();
            activeChallengesDict["activeChallenges"] = activeChallengesJson;

            var updateDataRequest = new UpdateUserInternalDataRequest
            {
                PlayFabId = playerId,
                Data = activeChallengesDict
            };

             var updateDataResult = await PlayFabServerAPI.UpdateUserInternalDataAsync(updateDataRequest);
             if(updateDataResult.Error==null)
             {
                hasUpdated = true;
             }
             else{
                 errorMessage = updateDataResult.Error.ErrorMessage;
             }

             return new Tuple<bool, string>(hasUpdated,errorMessage);
        }
    }

    public class GameJoin
    {
        [FunctionName("GameJoin")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            // Get request body
            GameLeaveRequest body = await req.Content.ReadAsAsync<GameLeaveRequest>();

            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
            // Event Log for testing. Remove this in production
            WriteTitleEventRequest titleEventRequest = new WriteTitleEventRequest();
            titleEventRequest.EventName = "room_joined";
            titleEventRequest.Body = new System.Collections.Generic.Dictionary<string, object> {
                { "RequestBody", JsonConvert.SerializeObject(body) }
            };
            await PlayFabServerAPI.WriteTitleEventAsync(titleEventRequest);

            // Logs for testing. Remove this in production
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
    
            // Request data validity check
            string message;
            if (!Utils.IsGameValid(body, out message))
            {
                var errorResponse = new { 
                    ResultCode = 1,
                    Error = message
                };

                return new OkObjectResult(errorResponse);
            }

            // Logs for testing. Remove this in production
            var okMsg = $"{req.RequestUri} - Closed Game - {body.GameId}";
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
            // Get request body
            GameEventRequest body = await req.Content.ReadAsAsync<GameEventRequest>();

            // Logs for testing. Remove this in production
            var okMsg = $"{req.RequestUri} - Recieved Game Event";
            log.LogInformation(okMsg);

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
            // Get request body
            GamePropertiesRequest body = await req.Content.ReadAsAsync<GamePropertiesRequest>();

            // Request data validity check
            string message;
            if (!Utils.IsGameValid(body, out message))
            {
                var errorResponse = new { 
                    ResultCode = 1,
                    Error = message
                };

                return new OkObjectResult(errorResponse);
            }
    
            if(body.State != null)
            { 
                var state = (string)JsonConvert.SerializeObject(body.State);
                
                var properties = body.Properties;
                //// Example of how to get data from properties
                // object actorNrNext = null;
                // properties?.TryGetValue("turn", out actorNrNext);
            }

            // Logs for testing. Remove this in production
            var okMsg = $"{req.RequestUri} - Uploaded Game Properties";
            log.LogInformation(okMsg);

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
            // Get request body
            GameLeaveRequest body = await req.Content.ReadAsAsync<GameLeaveRequest>();
    
            // Request data validity check
            string message;
            if (!Utils.IsGameValid(body, out message))
            {
                var errorResponse = new { 
                    ResultCode = 1,
                    Error = message
                };

                return new OkObjectResult(errorResponse);
            }

            if (body.IsInactive)
            {
                // Set to inactive in shared data here
            }
            else
            {
                // Remove from shared data here
            }

            // Logs for testing. Remove this in production
            var okMsg = $"{req.RequestUri} - {body.UserId} left {body.GameId}";
            log.LogInformation(okMsg);

            var response = new { 
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }
    }
}
