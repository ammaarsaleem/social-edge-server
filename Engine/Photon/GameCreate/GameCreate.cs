using System;
using PlayFab;
using System.Net.Http;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Common.Models;
using SocialEdge.Server.Constants;
namespace SocialEdge.Playfab.Photon.Events
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
            SocialEdgeEnvironment.Init(req);

            string activeChallengesData = string.Empty;
            string message = string.Empty;
            List<string> activeChallenges = null;

            GameCreateRequest body = await req.Content.ReadAsAsync<GameCreateRequest>();

            if (!Utils.IsGameValid(body.GameId, body.UserId, out message))
            {
                message = "game is not valid";
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }

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
                        Result addPlayerChallengeResult = await Utils.AddPlayerChallenge(currentChallengeId, activeChallenges, playerId);
                        if (addPlayerChallengeResult.isSuccess)
                        {
                            // log.LogInformation("player added to group and internal data updated");
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