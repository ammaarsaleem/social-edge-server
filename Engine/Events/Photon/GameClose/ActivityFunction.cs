using PlayFab;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using SocialEdgeSDK.Server.Common.Models;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Constants;
using Newtonsoft.Json;
using System.Linq;
namespace SocialEdgeSDK.Playfab.Photon.Events
{
    public partial class GameClose
    {
        /// <summary>
        /// Removes challenge from active challenges of players that are still in the room
        /// Deletes shared group from playfab
        /// deletes room from cache
        /// </summary>
        /// <param name="GameCloseRequest"></param>
        [FunctionName(Constant.GAME_CLOSE_ENGINE_ACTIVITY)]
        public async Task<OkObjectResult> ActivityFunc(
            [ActivityTrigger] GameCloseRequest body, ILogger log)
        {
            string message = string.Empty;
            dynamic gameState = body.State;
            string currentChallengeId = body.GameId;

            try
            {
                if (gameState != null)
                {
                    var actorsData = gameState["ActorList"];
                    List<Actor> players = JsonConvert.DeserializeObject<List<Actor>>(actorsData.ToString());
                    List<string> ids = players.Select(a => a.UserId).ToList();
                    List<Task> getDataTasks = new List<Task>();
                    foreach (var id in ids)
                    {
                        var getPlayerDataRequest = new GetUserDataRequest
                        {
                            PlayFabId = id,
                            Keys = new List<string> { Constant.ACTIVE_CHALLENGES }
                        };
                        Task<PlayFabResult<GetUserDataResult>> playerDataTask = PlayFabServerAPI.GetUserReadOnlyDataAsync(getPlayerDataRequest);
                        getDataTasks.Add(playerDataTask);
                    }

                    Task getT = Task.WhenAll(getDataTasks);
                    await getT;

                    if (getT.Status == TaskStatus.RanToCompletion)
                    {
                        Result updateResult = await Utils.UpdateUsersChallenges(currentChallengeId, getDataTasks);
                        if (updateResult.isSuccess)
                        {
                            var deleteGroupRequest = new DeleteSharedGroupRequest
                            {
                                SharedGroupId = currentChallengeId
                            };

                            // var deleteGroupResult = await PlayFabServerAPI.DeleteSharedGroupAsync(deleteGroupRequest);
                            
                            var deleteGroupT = PlayFabServerAPI.DeleteSharedGroupAsync(deleteGroupRequest);
                            Task deleteRoomT = _cache.DeleteRoom(currentChallengeId);
                            Task t = Task.WhenAll(deleteGroupT,deleteRoomT);
                            await t;
                            
                            if (t.IsCompletedSuccessfully)
                            {
                                return Utils.GetSuccessResponse();
                            }
                            else
                            {
                                message = "Group: " + currentChallengeId + " not deleted. " + t.Exception.Message;
                            }
                        }
                        else
                        {
                            message = "Player(s) active challenge not removed";
                            log.LogInformation(message);
                        }
                    }
                    else
                    {
                        message = "Unable to get player(s) data";
                        log.LogInformation(message);
                    }
                }
                return Utils.GetErrorResponse("Invalid game or players do not exist");
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}