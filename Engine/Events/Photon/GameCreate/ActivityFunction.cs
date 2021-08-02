using PlayFab;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using PlayFab.ServerModels;
using SocialEdge.Server.Constants;
namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameCreate
    {
        /*photon waits for response*/

        /// <summary>
        /// Creates a shared group and room in cache
        /// Add challenge to the active challenges of the callee
        /// </summary>
        /// <param name="GameCreateRequest"></param>
        [FunctionName(Constant.GAME_CREATE_ENGINE_ACTIVITY)]
        public async Task<OkObjectResult> ActivityFunc(
            [ActivityTrigger] GameCreateRequest body, ILogger log)
        {
            string activeChallengesData = string.Empty;
            string message = string.Empty;
            List<string> activeChallenges = null;

            string currentChallengeId = body.GameId;
            string playerId = body.UserId;

            try
            {
                var createGroupRequest = new CreateSharedGroupRequest
                {
                    SharedGroupId = currentChallengeId
                };
                var getPlayerDataRequest = new GetUserDataRequest
                {
                    PlayFabId = playerId,
                    Keys = new List<string> { Constant.ACTIVE_CHALLENGES}
                };

                var createGroupTask = PlayFabServerAPI.CreateSharedGroupAsync(createGroupRequest);
                var playerDataResult = await PlayFabServerAPI.GetUserReadOnlyDataAsync(getPlayerDataRequest);

                if (playerDataResult.Error == null)
                {
                    activeChallenges = Utils.GetActiveChallenges(playerDataResult.Result);
                    var createGroupResult = await createGroupTask;
                    if (createGroupTask.IsCompletedSuccessfully && createGroupResult.Error == null)
                    {
                        log.LogInformation("group created with id: " + createGroupResult.Result.SharedGroupId);

                        var addPlayerChallengeResult =  await Utils.AddPlayerChallenge(currentChallengeId, activeChallenges, playerId);
                        if(addPlayerChallengeResult.isSuccess)
                        {
                            var addPlayerToCacheT =  _cache.AddPlayerToRoom(currentChallengeId,playerId);
                            if(addPlayerToCacheT.IsCompletedSuccessfully)
                            {
                                return Utils.GetSuccessResponse();
                            }
                            else
                            {
                                message = addPlayerToCacheT.Exception.Message;
                                log.LogInformation(message);
                            }
                        }
                        
                        else
                        {
                            message = addPlayerChallengeResult.error;
                            log.LogInformation(message);
                        }
                    }
                    else
                    {
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
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}