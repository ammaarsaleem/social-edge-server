using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using PlayFab.Samples;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdge.Playfab.Models;
using SocialEdge.Server.Constants;
using SocialEdge.Server.Util;
using Newtonsoft.Json;
using System.Net;
using System.Text;
namespace SocialEdge.Playfab
{
    public class GetInitData
    {
        [FunctionName("GetInitData")]
        public async Task<Dictionary<string,object>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string playerId = string.Empty;
            
            var context = await Util.Init(req);
            dynamic args = context.FunctionArgument;
            
            
            if(args!=null)
            {
                playerId = args["playerId"];
            }
            try
            {
                Dictionary<string,object> dict = new Dictionary<string, object>();
                var playerDataRequest = CreatePlayerDataRequest(context.CallerEntityProfile.Lineage.MasterPlayerAccountId, playerId);
                var titleNewsRequest = PlayFabServerAPI.GetTitleNewsAsync(new GetTitleNewsRequest());
                var playerDataTask =  PlayFabServerAPI.GetPlayerCombinedInfoAsync(playerDataRequest);
                List<Task> tasks = new List<Task> { playerDataTask, titleNewsRequest };
                await Task.WhenAll(tasks);

                var result = CreateResult(tasks, playerDataTask, context.CallerEntityProfile.Objects);
                var abc = JsonConvert.SerializeObject(result);
                dict["player"] = result.playerData;
                dict["playerCustomSettings"]  = context.CallerEntityProfile.Objects;
                ErrorData err = new ErrorData{error = "error",errors="errors"};
                dict["Error"] = err;
                return dict;
                // return result.playerData;

                // return req.CreateResponse(HttpStatusCode.OK, json, "application/json");
                // return new HttpResponseMessage(HttpStatusCode.OK) 
                // {
                //     Content = new StringContent(abc, System.Text.Encoding.UTF8, "application/json")
                // };

            }

            catch (Exception e)
            {
                throw e;
            }


        }

        private InitDataResult CreateResult(List<Task> tasks,
                                                Task<PlayFabResult<GetPlayerCombinedInfoResult>> playerDataTask,
                                                Dictionary<string, PlayFab.ProfilesModels.EntityDataObject> playerSettings)
        {
            InitDataResult result = new InitDataResult();
            if (playerDataTask.IsCompletedSuccessfully && playerDataTask.Result.Error == null)
            {
                result.playerData = playerDataTask.Result.Result.InfoResultPayload;
                    // playerSettings = playerSettings
                // };
            }

            else if (playerDataTask.IsCompletedSuccessfully && playerDataTask.Result.Error != null)
            {
                result.error = new
                {
                    playerReqError = playerDataTask.Result.Error.Error
                };
            }

            else
            {
                result.error = new
                {
                    playerDataTask.Exception.InnerException
                };
            }

            return result;
        }

        private GetPlayerCombinedInfoRequest CreatePlayerDataRequest(string titlePlayerAccountId, string playerId)
        {
            var playerDataRequest = new GetPlayerCombinedInfoRequest();
            if (string.IsNullOrEmpty(playerId) || playerId == titlePlayerAccountId)
            {
                playerDataRequest.InfoRequestParameters = SetRequestParams();
                playerDataRequest.PlayFabId = titlePlayerAccountId;
            }
            else
            {
                playerDataRequest.InfoRequestParameters = SetRequestParamsForOtherPlayer();
            }

            return playerDataRequest;
        }
        private GetPlayerCombinedInfoRequestParams SetRequestParams()
        {
            var infoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true,
                GetUserAccountInfo = true,
                GetPlayerProfile = true,
                GetUserInventory = true,
                GetPlayerStatistics = true,
                GetUserReadOnlyData = true,
                GetTitleData = true,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowAvatarUrl = true,
                    ShowLinkedAccounts = true,
                    ShowBannedUntil = true,
                    ShowCreated = true,
                    ShowDisplayName = true,
                    ShowLastLogin = true,
                    ShowLocations = true,
                    ShowTotalValueToDateInUsd = true,
                    ShowOrigination = true
                },
                
                
            };

            return infoRequestParameters;
        }

        private GetPlayerCombinedInfoRequestParams SetRequestParamsForOtherPlayer()
        {
            var infoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true,
                GetPlayerProfile = true,
                GetPlayerStatistics = true,
                GetTitleData = true,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowAvatarUrl = true,
                    ShowBannedUntil = true,
                    ShowDisplayName = true,
                    ShowLastLogin = true,
                    ShowLocations = true
                }
            };

            return infoRequestParameters;
        }
    }
}
