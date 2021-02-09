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
using System.Linq;
namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameJoin
    {
        /// <summary>
        /// Adds player to shared group and room in cache
        /// Add challenge to the active challenges of the callee
        /// </summary>
        /// <param name="GameLeaveRequest"></param>
        [FunctionName(Constant.GAME_JOIN_ENGINE_ACTIVITY)]
        public async Task<OkObjectResult> ActivityFunc(
            [ActivityTrigger] GameLeaveRequest body, ILogger log)
        {
            string message = string.Empty;
            List<string> activeChallenges = null;

            log.LogInformation("Game join activity function started");
            string currentChallengeId = body.GameId;
            string playerId = body.UserId;

            var getPlayerDataRequest = new GetUserDataRequest
            {
                PlayFabId = playerId,
                Keys = new List<string> { Constant.ACTIVE_CHALLENGES }
            };

            try
            {
                var playerDataResult = await PlayFabServerAPI.GetUserReadOnlyDataAsync(getPlayerDataRequest);

                if (playerDataResult.Error == null)
                {
                    activeChallenges = Utils.GetActiveChallenges(playerDataResult.Result);
                    if (!activeChallenges.Any(s => s.Equals(currentChallengeId)))
                    {
                        Task addPlayerChallengeT =  Utils.AddPlayerChallenge(currentChallengeId, activeChallenges, playerId);
                        Task addPlayerToCacheT =  _cache.AddPlayerToRoom(currentChallengeId,playerId);
                        Task t = Task.WhenAll(addPlayerChallengeT,addPlayerToCacheT);
                        await t;


                        if (t.IsCompletedSuccessfully)//addPlayerChallengeResult.isSuccess)
                        {
                            // log.LogInformation("player added to group and internal data updated");
                            return Utils.GetSuccessResponse();
                        }
                        else
                        {
                            message = t.Exception.Message;
                            log.LogInformation(message);
                        }
                    }
                    else
                    {
                        log.LogInformation(playerDataResult.Error.ErrorMessage);
                         message = "this challenge is already in active challenges list";
                         log.LogInformation(message);
                    }
                }
                else
                {
                    log.LogInformation(playerDataResult.Error.ErrorMessage);
                    message = "Unable to get user readonly data";
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