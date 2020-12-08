using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdge.Playfab.Models;
using SocialEdge.Server.Util;
namespace SocialEdge.Playfab
{
    public class GetInitData
    {
        [FunctionName("GetInitData")]
        public async Task<Dictionary<string,object>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            Dictionary<string,object> result = new Dictionary<string, object>();
            string playerId = string.Empty;
            
            var context = await Util.Init(req);
            dynamic args = context.FunctionArgument;
            
            if(args!=null)
            {
                playerId = args["playerId"];
            }
            try
            {
                var playerDataRequest = CreatePlayerDataRequest(context.CallerEntityProfile.Lineage.MasterPlayerAccountId, playerId);
                var titleNewsRequest = PlayFabServerAPI.GetTitleNewsAsync(new GetTitleNewsRequest());
                var playerDataTask =  PlayFabServerAPI.GetPlayerCombinedInfoAsync(playerDataRequest);
                List<Task> tasks = new List<Task> { playerDataTask, titleNewsRequest };
                await Task.WhenAll(tasks);

                CreatePlayerResult(tasks, playerDataTask, context.CallerEntityProfile.Objects,result);
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private Dictionary<string,object> CreatePlayerResult(List<Task> tasks, Task<PlayFabResult<GetPlayerCombinedInfoResult>> playerDataTask,
                                                Dictionary<string, PlayFab.ProfilesModels.EntityDataObject> playerSettings,
                                                 Dictionary<string,object> result)
        {
            PlayerModel player = new PlayerModel();
            if (playerDataTask.IsCompletedSuccessfully && playerDataTask.Result.Error == null)
            {
                player.combinedInfo = playerDataTask.Result.Result.InfoResultPayload;
                player.customSettings = playerSettings;
                result["player"] = player;
            }

            else if (playerDataTask.IsCompletedSuccessfully && playerDataTask.Result.Error != null)
            {
                string error = playerDataTask.Result.Error.ErrorMessage;
                result["error"] = error;
            }

            else
            {
                 string error = playerDataTask.Exception.InnerException.ToString();
                 result["error"] = error;
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
