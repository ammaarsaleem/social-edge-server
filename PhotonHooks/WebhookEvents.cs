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
            
            if(!Utils.IsGameValid(body, out message))
            {
                message = "game is not valid";
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }

            string currentChallengeId = body.GameId;
            string playerId = body.UserId;

            var createGroupRequest = new CreateSharedGroupRequest{
                SharedGroupId = currentChallengeId
            };            
            var getPlayerDataRequest = new GetUserDataRequest{
                PlayFabId = playerId,
                Keys = new List<string>{"activeChallenges"}
            };

            var createGroupTask = PlayFabServerAPI.CreateSharedGroupAsync(createGroupRequest);
            var playerDataResult= await PlayFabServerAPI.GetUserInternalDataAsync(getPlayerDataRequest);        
            
            if(playerDataResult.Error==null)
            {
                log.LogInformation("group created with id: " + createGroupTask.Result.Result.SharedGroupId);
                log.LogInformation("player data fetched");

                activeChallenges = UtilMethods.GetActiveChallenges(playerDataResult.Result);
                var createGroupResult = await createGroupTask;
                if(createGroupResult.Error==null)
                {
                    Tuple<bool,string> addPlayerChallengeResult  = await UtilMethods.AddPlayerChallenge(currentChallengeId,activeChallenges,playerId);
                    bool isChallengedAdded = addPlayerChallengeResult.Item1;
                    if(isChallengedAdded)
                    {
                        return Utils.GetSuccessResponse();
                    }
                    else
                    {
                        message = addPlayerChallengeResult.Item2;
                        log.LogInformation(message);
                    }
                }
                else{
                    message = "group could not be created";
                    log.LogInformation(message);
                }
            }
            else
            {
                message = "unable to get player data";
                log.LogInformation(message);
            }

            return Utils.GetErrorResponse(message);
        }
    }


    public class GameJoin
    {
        [FunctionName("GameJoin")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
            RequestUtil.Init(req);

            string activeChallengesData = string.Empty;
            string message = string.Empty;
            List<string> activeChallenges = null;
            GameCreateRequest body = await req.Content.ReadAsAsync<GameCreateRequest>();
            
            var msg = $"{req.RequestUri} - Room Joined";
            log.LogInformation($"{msg} :: {JsonConvert.SerializeObject(body)}");
            if(Utils.IsGameValid(body, out message))
            {
                string currentChallengeId = body.GameId;
                string playerId = body.UserId;
                
                var getPlayerDataRequest = new GetUserDataRequest{
                    PlayFabId = playerId,
                    Keys = new List<string>{"activeChallenges"}
                };
                var playerDataResult= await PlayFabServerAPI.GetUserInternalDataAsync(getPlayerDataRequest);
                
                if(playerDataResult.Error==null)
                {
                    log.LogInformation("player data fetched");

                    activeChallenges = UtilMethods.GetActiveChallenges(playerDataResult.Result);
                    if (!activeChallenges.Any(s => s.Equals(currentChallengeId)))
                    {
                        activeChallenges.Add(currentChallengeId);
                        var addPlayerToGroupResult =  await UtilMethods.AddToGroup(playerId,currentChallengeId);
                        var isPlayerAddedToGroup = addPlayerToGroupResult.Item1;
                        if(isPlayerAddedToGroup)
                        {
                            var updatePlayerDataResult = await UtilMethods.UpdateUserData(activeChallenges, playerId);
                            bool isPlayerDataUpdated = updatePlayerDataResult.Item1;
                            if(isPlayerDataUpdated)
                            {
                                log.LogInformation("Internal data successfully updated");
                                return Utils.GetSuccessResponse();
                            }

                            else
                            {
                                message = updatePlayerDataResult.Item2;
                                log.LogInformation(message);
                            }
                        }

                        else
                        {
                            message = addPlayerToGroupResult.Item2;
                            log.LogInformation(message);
                        }
                    }

                    else
                    {
                        message = "this challenge is already in active challenges list";
                        log.LogInformation(message);
                    }
                }
                else
                {
                    message = "Unable to get user internal data";
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

    public static class UtilMethods
    {

        public static async Task<Tuple<bool,string>> AddPlayerChallenge(string currentChallengeId, List<string> activeChallenges, string playerId)
        {
            string error = string.Empty;
            bool isPlayerDataUpdated = false;
            if (!activeChallenges.Any(s => s.Equals(currentChallengeId)))
            {
                activeChallenges.Add(currentChallengeId);
                var addPlayerToGroupResult =  await AddToGroup(playerId,currentChallengeId);
                var isPlayerAddedToGroup = addPlayerToGroupResult.Item1;
                if(isPlayerAddedToGroup)
                {
                    var updatePlayerDataResult = await UpdateUserData(activeChallenges, playerId);
                    isPlayerDataUpdated = updatePlayerDataResult.Item1;
                    if(isPlayerDataUpdated)
                    {
                        // log.LogInformation("Internal data successfully updated");
                        return Tuple.Create(isPlayerDataUpdated,error);
                    }

                    else
                    {
                        error = updatePlayerDataResult.Item2;
                        // log.LogInformation(error);
                    }
                }

                else
                {
                    error = addPlayerToGroupResult.Item2;
                    // log.LogInformation(error);
                }
            }

            else
            {
                error = "this challenge is already in active challenges list";
                // log.LogInformation(error);
            }

            return Tuple.Create(isPlayerDataUpdated,error);
        }

        public static List<string> GetActiveChallenges(GetUserDataResult playerData)
        {
            List<string> activeChallenges = new List<string>();
            string activeChallengesData = string.Empty;
            if (playerData.Data.ContainsKey("activeChallenges"))
            {
                activeChallengesData = playerData.Data["activeChallenges"].Value;
            }

            if (!string.IsNullOrEmpty(activeChallengesData))
            {
                activeChallenges = JsonConvert.DeserializeObject<List<string>>(activeChallengesData);
            }

            return activeChallenges;
        }

        public static async Task<Tuple<bool,string>> UpdateUserData(List<string> activeChallenges, string playerId)
        {
            string errorMessage = string.Empty;
            bool hasUpdated=false;
            string activeChallengesJson = JsonConvert.SerializeObject(activeChallenges);
            var activeChallengesDict = new Dictionary<string, string>();
            activeChallengesDict.Add("activeChallenges", activeChallengesJson);

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

        public static async Task<Tuple<bool,string>> AddToGroup(string playerId, string groupId)
        {
            string error = string.Empty;
            bool addedToGroup = false;
            var request = new AddSharedGroupMembersRequest
            {
                PlayFabIds = new List<string>{playerId},
                SharedGroupId = groupId
            };

            var result = await PlayFabServerAPI.AddSharedGroupMembersAsync(request);
            if(result.Error==null)
                addedToGroup = true; 
            else
                error = result.Error.ErrorMessage;

            return new Tuple<bool, string>(addedToGroup,error);
        }
    }
}
