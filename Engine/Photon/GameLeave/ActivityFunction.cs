using System;
using PlayFab;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SocialEdge.Server.Common.Models;
using SocialEdge.Server.Constants;
namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameLeave
    {
        [FunctionName(Constant.GAME_LEAVE_ENGINE_ACTIVITY)]
        public async Task<OkObjectResult> ActivityFunc(
            [ActivityTrigger] GameLeaveRequest body, ILogger log)
        {
            string message = string.Empty;
            List<string> activeChallenges = null;
            try
            {
                if (!body.IsInactive)
                {
                    string currentChallengeId = body.GameId;
                    string playerId = body.UserId;

                    var getPlayerDataRequest = new GetUserDataRequest
                    {
                        PlayFabId = playerId,
                        Keys = new List<string> { Constant.ACTIVE_CHALLENGES }
                    };
                    var playerDataResult = await PlayFabServerAPI.GetUserReadOnlyDataAsync(getPlayerDataRequest);
                    if (playerDataResult.Error == null)
                    {
                        activeChallenges = Utils.GetActiveChallenges(playerDataResult.Result);
                        Result removePlayerChallengeResult = await Utils.RemovePlayerChallenge(currentChallengeId, activeChallenges, playerId);
                        if (removePlayerChallengeResult.isSuccess)
                        {
                            if (!body.IsInactive)
                            {
                                log.LogInformation("Game leave activity function successful");
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
                        message = "Unable to get user readonly data";
                        log.LogInformation(message);
                    }
                }
                else
                {
                    return Utils.GetSuccessResponse();
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