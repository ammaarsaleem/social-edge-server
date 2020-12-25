using PlayFab;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Collections.Generic;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Common.Models;
using PlayFab.ServerModels;
using SocialEdge.Server.Constants;
namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameJoin
    {
        [FunctionName("GameJoinEngineActivity")]
        public async Task<OkObjectResult> ActivityFunc(
            [ActivityTrigger] GameLeaveRequest body, ILogger log)
        {
            string message = string.Empty;
            List<string> activeChallenges = null;

            log.LogInformation("Game join activity");
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

                    Result addPlayerChallengeResult = await Utils.AddPlayerChallenge(currentChallengeId, activeChallenges, playerId);
                    if (addPlayerChallengeResult.isSuccess)
                    {
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