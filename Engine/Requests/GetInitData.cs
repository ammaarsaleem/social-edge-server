using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Requests.Models;
using SocialEdge.Server.Constants;
using SocialEdge.Server.Db;
namespace SocialEdge.Server.Requests
{
    public class GetInitData
    {
        IDbHelper _dbHelper;
        public GetInitData(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        /// Fetches player and title data. To be used for initialisation
        /// When called for the first time for a player, initialises the player, assigns a random name and sets default values
        /// </summary>
        /// <param name="playerId">the playerId of the user to fetch data/param>
        /// <returns>Dictionary with player data and title data</returns>
        [FunctionName("GetInitData")]
        public async Task<Dictionary<string, object>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;

            string playerTitleId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            Dictionary<string, object> result = null;
            string playerSettings = string.Empty;
            List<Task> tasks = null;
            // Task<PlayFabResult<PlayFab.AdminModels.UpdateUserTitleDisplayNameResult>> updateNameT = null;
            // Task<PlayFabResult<UpdateUserDataResult>> initPlayerT = null;
            string playerId = string.Empty;
            GetPlayerCombinedInfoResultPayload playerDataResult = null;
            GetUserDataResult playerInternalDataResult = null;

            try
            {
                playerId = args["playerId"];
                var playerDataRequest = CreatePlayerDataRequest(playerTitleId, playerId);
                var playerDataT = PlayFabServerAPI.GetPlayerCombinedInfoAsync(playerDataRequest);
                
                /*TODO: new title news method*/
                //var titleNewsT = PlayFabServerAPI.GetTitleNewsAsync(new GetTitleNewsRequest());
                var internalDataT = PlayFabServerAPI.GetUserInternalDataAsync(new GetUserDataRequest
                {
                    PlayFabId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId
                });

                
                await Task.WhenAll(internalDataT, playerDataT/*, titleNewsT*/);

                // if(UtilFunc.IsTaskCompleted(playerDataT))
                // {}

                if (playerDataT.IsCompletedSuccessfully && playerDataT.Result.Error == null
                    && internalDataT.IsCompletedSuccessfully && internalDataT.Result.Error == null)
                {
                    tasks = new List<Task>();
                    playerDataResult = playerDataT.Result.Result.InfoResultPayload;
                    playerInternalDataResult = internalDataT.Result.Result;

                    if (!IsPlayerInitialized(playerInternalDataResult.Data))
                    {
                        var initPlayerT = InitializePlayer(playerDataResult );
                        tasks.Add(initPlayerT);

                        var defaultSettingsT = SetDefaultSettings(playerDataResult.TitleData,playerId);
                        tasks.Add(defaultSettingsT);

                        var updateNameT = UpdateDisplayName(context.CallerEntityProfile.Lineage.MasterPlayerAccountId, playerDataT.Result.Result.PlayFabId);
                        tasks.Add(updateNameT);
                    } 

                    result = CreatePlayerResult(playerDataT);

                    if (tasks.Count > 0)
                        await Task.WhenAll(tasks);
                }


                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<PlayFabResult<UpdateUserDataResult>> InitializePlayer(GetPlayerCombinedInfoResultPayload result)
        {
            PlayFabResult<UpdateUserDataResult> updatePlayerDataResult = null;
            var updateReadOnlyDataReq = new UpdateUserInternalDataRequest
            {
                Data = new Dictionary<string, string>
                {
                    {Constant.IS_PLAYER_INIT,"true"}
                },
                PlayFabId = result.PlayerProfile.PlayerId
            };
            updatePlayerDataResult = await PlayFabServerAPI.UpdateUserInternalDataAsync(updateReadOnlyDataReq);

            return updatePlayerDataResult;
        }

        private async Task<PlayFabResult<UpdateUserDataResult>> SetDefaultSettings(Dictionary<string,string> titleData, string playerId)
        {
            PlayFabResult<UpdateUserDataResult> updatePlayerDataResult = null;
            string defaultSettings = GetDefaultSettingsFromTitle(titleData);
            if (!string.IsNullOrEmpty(defaultSettings))
            {
                var updateUserDataReq = new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string>
                    {
                        {Constant.PLAYER_SETTINGS, defaultSettings}
                    },
                    // PlayFabId = result.PlayerProfile.PlayerId
                    PlayFabId = playerId
                };
                updatePlayerDataResult = await PlayFabServerAPI.UpdateUserDataAsync(updateUserDataReq);
            }

            return updatePlayerDataResult;
        }
        private async Task<PlayFabResult<PlayFab.AdminModels.UpdateUserTitleDisplayNameResult>> UpdateDisplayName(string masterAccountId, string playFabId)
        {
            string displayName = CreateDisplayName(playFabId);
            var updateNameRequest = new PlayFab.AdminModels.UpdateUserTitleDisplayNameRequest
            {
                PlayFabId = masterAccountId,
                DisplayName = displayName
            };
            PlayFabResult<PlayFab.AdminModels.UpdateUserTitleDisplayNameResult> result = await PlayFabAdminAPI.UpdateUserTitleDisplayNameAsync(updateNameRequest);
            return result;
        }

        private bool IsPlayerInitialized(Dictionary<string, UserDataRecord> data)
        {
            if (data.ContainsKey(Constant.IS_PLAYER_INIT) && data[Constant.IS_PLAYER_INIT].Value == "true")
            {
                return true;
            }

            return false;
        }
        private string CreateDisplayName(string playFabId)
        {
            return "Guest" + playFabId.GetHashCode().ToString();
        }

        private Dictionary<string, object> CreatePlayerResult(Task<PlayFabResult<GetPlayerCombinedInfoResult>> playerDataTask)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            string playFabId = playerDataTask.Result.Result.PlayFabId;
            var playerDataResult = playerDataTask.Result.Result.InfoResultPayload;
            if (string.IsNullOrEmpty(playerDataResult.PlayerProfile.DisplayName))
            {
                playerDataResult.PlayerProfile.DisplayName = CreateDisplayName(playFabId);
            }

            if (!playerDataResult.UserReadOnlyData.ContainsKey(Constant.PLAYER_SETTINGS))
            {
                playerDataResult.UserReadOnlyData.Add(Constant.PLAYER_SETTINGS, new UserDataRecord
                { Value = GetDefaultSettingsFromTitle(playerDataResult.TitleData) });
            }

            if (playerDataTask.IsCompletedSuccessfully && playerDataTask.Result.Error == null)
            {
                PlayerModel player = new PlayerModel();

                player.combinedInfo = playerDataResult;
                // player.customSettings = null;
                result["player"] = player;
                result["titleData"] = player.combinedInfo.TitleData;
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


        private string GetDefaultSettingsFromTitle(Dictionary<string, string> data)
        {
            if (data.ContainsKey(Constant.PLAYER_SETTINGS))
            {
                return data[Constant.PLAYER_SETTINGS];
            }

            return string.Empty;
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
            var infoRequestParameters = new PlayFab.ServerModels.GetPlayerCombinedInfoRequestParams
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
