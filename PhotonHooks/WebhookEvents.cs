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
                log.LogInformation("player data fetched");

                activeChallenges = UtilMethods.GetActiveChallenges(playerDataResult.Result);
                var createGroupResult = await createGroupTask;
                if(createGroupResult.Error==null)
                {
                    log.LogInformation("group created with id: " + createGroupTask.Result.Result.SharedGroupId);
                    Result addPlayerChallengeResult  = await UtilMethods.AddPlayerChallenge(currentChallengeId,activeChallenges,playerId);
                    if(addPlayerChallengeResult.isSuccess)
                    {
                        log.LogInformation("player added to group and internal data updated");
                        return Utils.GetSuccessResponse();
                    }
                    else
                    {
                        message = addPlayerChallengeResult.error;
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
            string message = string.Empty;
            List<string> activeChallenges = null;
            GameCreateRequest body = await req.Content.ReadAsAsync<GameCreateRequest>();
            
            var msg = $"{req.RequestUri} - Room Joined";
            log.LogInformation($"{msg} :: {JsonConvert.SerializeObject(body)}");
            if(!Utils.IsGameValid(body,out message))
            {
                message = "Game is not valid";
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }
                
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
                
                Result addPlayerChallengeResult  = await UtilMethods.AddPlayerChallenge(currentChallengeId,activeChallenges,playerId);
                if(addPlayerChallengeResult.isSuccess)
                {
                    log.LogInformation("player added to group and internal data updated");
                    return Utils.GetSuccessResponse();
                }
                else
                {
                    message = addPlayerChallengeResult.error;
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
    }

    public class GameLeave
    {
        [FunctionName("GameLeave")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] 
            HttpRequestMessage req, ILogger log)
        {
               RequestUtil.Init(req);
            string message = string.Empty;
            List<string> activeChallenges = null;
            GameLeaveRequest body = await req.Content.ReadAsAsync<GameLeaveRequest>();
            
            var msg = $"{req.RequestUri} - Room Joined";
            log.LogInformation($"{msg} :: {JsonConvert.SerializeObject(body)}");
            if(!Utils.IsGameValid(body,out message))
            {
                message = "Game is not valid";
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }
            if(!body.IsInactive)
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
                    
                    Result removePlayerChallengeResult  = await UtilMethods.RemovePlayerChallenge(currentChallengeId,activeChallenges,playerId);
                    if(removePlayerChallengeResult.isSuccess)
                    {
                        log.LogInformation("player removed from group and internal data updated");
                        if(!body.IsInactive)
                        {
                            return Utils.GetSuccessResponse();
                        }
                    }
                    else
                    {
                        message = removePlayerChallengeResult.error;
                        log.LogInformation(message);
                    }
                }
                else
                {
                    message = "Unable to get user internal data";
                    log.LogInformation(message);
                }
            }
            else
            {
                return Utils.GetSuccessResponse();
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
            log.LogInformation("method begin");
            RequestUtil.Init(req);
            string message = string.Empty;
            GameCloseRequest body = await req.Content.ReadAsAsync<GameCloseRequest>();
            
            if (!Utils.IsGameValid(body, out message))
            {
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }
            log.LogInformation("before getting state");
            dynamic gameState = body.State;
            string currentChallengeId = body.GameId;
            log.LogInformation(currentChallengeId);
            if(gameState!=null && gameState["ActorsList"]!=null)
            {
                log.LogInformation("before getting actor list");
                var actorsData = gameState["ActorList"];
                List<Actor> players = JsonConvert.DeserializeObject<List<Actor>>(actorsData.ToString());
                List<string> ids = players.Select(a => a.UserId).ToList();
                log.LogInformation("ids: " + ids.ToString());
                List<Task> getDataTasks = new List<Task>(); 
                foreach (var id in ids)
                {
                    var getPlayerDataRequest = new GetUserDataRequest
                    {
                        PlayFabId = id,
                        Keys = new List<string> { "activeChallenges" }
                    };
                    log.LogInformation("before calling get user intetnal data");
                    Task<PlayFabResult<GetUserDataResult>> playerDataTask = PlayFabServerAPI.GetUserInternalDataAsync(getPlayerDataRequest);
                    getDataTasks.Add(playerDataTask);
                    
                }

                Task getT = Task.WhenAll(getDataTasks);
                await getT;

                log.LogInformation("all tasks awaited");
                if (getT.Status == TaskStatus.RanToCompletion)
                {
                    log.LogInformation("all get tasks ran to completion");
                    Result updateResult = await UtilMethods.UpdateUsersData(currentChallengeId, getDataTasks);
                    if (updateResult.isSuccess)
                    {
                        var deleteGroupRequest = new DeleteSharedGroupRequest
                        {    
                            SharedGroupId = currentChallengeId
                        };

                        var deleteGroupResult = await PlayFabServerAPI.DeleteSharedGroupAsync(deleteGroupRequest);
                        if(deleteGroupResult.Error==null)
                        {
                            log.LogInformation("group deleted successfully");
                            return Utils.GetSuccessResponse();
                        }
                        else
                        {
                            message = "Group: " +currentChallengeId +" not deleted";
                        }
                    }
                    else
                    {
                        message = "Player(s) active challenge not removed";
                        log.LogInformation(message);
                    }
                }
                else{
                message = "Unable to get player(s) data";
                log.LogInformation(message);
               }
            }
            return Utils.GetErrorResponse("Invalid game or players do not exist");
    }

        // private static List<Task> GetPlayersActiveChallenges(List<string> ids)
        // {
        //     List<Task> getDataTasks = new List<Task>(); ;
        //     foreach (var id in ids)
        //     {
        //         var getPlayerDataRequest = new GetUserDataRequest
        //         {
        //             PlayFabId = id,
        //             Keys = new List<string> { "activeChallenges" }
        //         };
        //         Task<PlayFabResult<GetUserDataResult>> playerDataTask = PlayFabServerAPI.GetUserInternalDataAsync(getPlayerDataRequest);
        //         getDataTasks.Add(playerDataTask);
        //         Task getT = Task.WhenAll(getDataTasks);
        //         await getT;
        //     }

        //     return getDataTasks;
        // }

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

   

    public static class UtilMethods
    {

        public static async Task<Result> AddPlayerChallenge(string currentChallengeId, List<string> activeChallenges, string playerId)
        {
            string error = string.Empty;
            bool isPlayerDataUpdated = false;
            if (!activeChallenges.Any(s => s.Equals(currentChallengeId)))
            {
                activeChallenges.Add(currentChallengeId);
                var addPlayerToGroupResult =  await AddToGroup(playerId,currentChallengeId);
                var isPlayerAddedToGroup = addPlayerToGroupResult.isSuccess;
                if(isPlayerAddedToGroup)
                {
                    var updatePlayerDataResult = await UpdateUserData(activeChallenges, playerId);
                    isPlayerDataUpdated = updatePlayerDataResult.isSuccess;
                    if(isPlayerDataUpdated)
                    {                    
                        return new Result(isPlayerDataUpdated,error);   
                        // return Tuple.Create(isPlayerDataUpdated,error);
                    }
                    else
                    {
                        error = updatePlayerDataResult.error;
                    }
                }
                else
                {
                    error = addPlayerToGroupResult.error;
                }
            }
            else
            {
                error = "this challenge is already in active challenges list";
            }
            return new Result(isPlayerDataUpdated,error);   
            // return Tuple.Create(isPlayerDataUpdated,error);
        }

        public static async Task<Result> RemovePlayerChallenge(string currentChallengeId, List<string> activeChallenges, string playerId)
        {
            string error = string.Empty;
            bool isPlayerDataUpdated = false;
            if (activeChallenges.Any(s => s.Equals(currentChallengeId)))
            {
                activeChallenges.RemoveAll(c=>c.Equals(currentChallengeId));
                var removePlayerFromGroupResult =  await RemoveFromGroup(playerId,currentChallengeId);
                var isPlayerRemovedFromGroup = removePlayerFromGroupResult.isSuccess;
                if(isPlayerRemovedFromGroup)
                {
                    var updatePlayerDataResult = await UpdateUserData(activeChallenges, playerId);
                    isPlayerDataUpdated = updatePlayerDataResult.isSuccess;
                    if(isPlayerDataUpdated)
                    {              
                        return new Result(isPlayerDataUpdated,error);      
                        // return Tuple.Create(isPlayerDataUpdated,error);
                    }
                    else
                    {
                        error = updatePlayerDataResult.error;
                    }
                }
                else
                {
                    error = removePlayerFromGroupResult.error;
                }
            }
            else
            {
                error = "this challenge is not in active challenges list";
            }

            return new Result(isPlayerDataUpdated,error);
            // return Tuple.Create(isPlayerDataUpdated,error);
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

        public static async Task<Result> UpdateUserData(List<string> activeChallenges, string playerId)
        {
            string error = string.Empty;
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
                 error = updateDataResult.Error.ErrorMessage;
             }

            //  return new Tuple<bool, string>(hasUpdated,errorMessage);
             return new Result(hasUpdated,error);
        }

        public static async Task<Result> AddToGroup(string playerId, string groupId)
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

            // return new Tuple<bool, string>(addedToGroup,error);
            return new Result(addedToGroup,error);
        }

        public static async Task<Result> RemoveFromGroup(string playerId, string groupId)
        {
            string error = string.Empty;
            bool removedFromGroup = false;
            var request = new RemoveSharedGroupMembersRequest
            {
                PlayFabIds = new List<string>{playerId},
                SharedGroupId = groupId
            };

            var result = await PlayFabServerAPI.RemoveSharedGroupMembersAsync(request);
            if(result.Error==null)
                removedFromGroup = true; 
            else
                error = result.Error.ErrorMessage;

            //return new Tuple<bool, string>(addedToGroup,error);
            return new Result(removedFromGroup,error);
        }

        public static async Task<Result> UpdateUsersData(string currentChallengeId, List<Task> getDataTasks)
        {
            List<Task> updateDataTasks = new List<Task>();
            bool isCompleted = false;
            string error = string.Empty;
            foreach (Task<PlayFabResult<GetUserDataResult>> playerDataTask in getDataTasks)
            {
                List<string> activeChallenges = UtilMethods.GetActiveChallenges(playerDataTask.Result.Result);
                activeChallenges.RemoveAll(c => c.Equals(currentChallengeId));

                string activeChallengesJson = JsonConvert.SerializeObject(activeChallenges);
                Dictionary<string, string> activeChallengesDict = new Dictionary<string, string>();
                activeChallengesDict.Add("activeChallenges", activeChallengesJson);

                var request = new UpdateUserInternalDataRequest
                {
                    Data = activeChallengesDict,
                    PlayFabId = playerDataTask.Result.Result.PlayFabId
                };

                Task<PlayFabResult<UpdateUserDataResult>> updateTask = PlayFabServerAPI.UpdateUserInternalDataAsync(request);
                updateDataTasks.Add(updateTask);
            }

            Task updateT = Task.WhenAll(updateDataTasks);
            await updateT;
            if(updateT.Status == TaskStatus.RanToCompletion)
            {
                isCompleted = true;
            }
            else 
            {
                error = updateT.Status.ToString();
            }

            return new Result(isCompleted,error);//Tuple.Create(isCompleted,error);
        }
    }

    public class Result
    {
        public bool isSuccess = false;
        public string error = string.Empty;
        public Result(bool success, string err)
        {
            isSuccess = success;
            error = err;
        }
    }

}
