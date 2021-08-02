using PlayFab;
using Newtonsoft.Json;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using SocialEdge.Server.Constants;
using SocialEdge.Server.Common.Models;
namespace SocialEdge.Playfab.Photon
{
    public static class Utils
    {
        public static bool IsGameValid(string gameId, out string message)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                message = "Missing GameId.";
                return false;
            }

            message = "";
            return true;
        }

        public static bool IsGameValid(string gameId, string userId, out string message)
        {
            if (string.IsNullOrEmpty(gameId))
            {
                message = "Missing GameId.";
                return false;
            }

            if (string.IsNullOrEmpty(userId))
            {
                message = "Missing UserId.";
                return false;
            }

            message = "";
            return true;
        }

        public static OkObjectResult GetErrorResponse(string message)
        {
            var errorResponse = new
            {
                ResultCode = 1,
                Error = message
            };

            return new OkObjectResult(errorResponse);
        }

        public static OkObjectResult GetSuccessResponse()
        {
            var response = new
            {
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }

        public static async Task<Result> AddPlayerChallenge(string currentChallengeId, List<string> activeChallenges, string playerId)
        {
            string error = string.Empty;
            bool isPlayerDataUpdated = false;
            if (!activeChallenges.Any(s => s.Equals(currentChallengeId)))
            {
                activeChallenges.Add(currentChallengeId);
                var addPlayerToGroupResult = await AddToGroup(playerId, currentChallengeId);
                var isPlayerAddedToGroup = addPlayerToGroupResult.isSuccess;
                if (isPlayerAddedToGroup)
                {
                    var updatePlayerDataResult = await UpdateUserChallenges(activeChallenges, playerId);
                    isPlayerDataUpdated = updatePlayerDataResult.isSuccess;
                    if (isPlayerDataUpdated)
                    {
                        return new Result(isPlayerDataUpdated, error);
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
            return new Result(isPlayerDataUpdated, error);
        }

        public static async Task<Result> RemovePlayerChallenge(string currentChallengeId, List<string> activeChallenges, string playerId)
        {
            string error = string.Empty;
            bool isPlayerDataUpdated = false;
            // if (activeChallenges.Any(s => s.Equals(currentChallengeId)))
            // {
                activeChallenges.RemoveAll(c => c.Equals(currentChallengeId));
                var removePlayerFromGroupResult = await RemoveFromGroup(playerId, currentChallengeId);
                var isPlayerRemovedFromGroup = removePlayerFromGroupResult.isSuccess;
                if (isPlayerRemovedFromGroup)
                {
                    var updatePlayerDataResult = await UpdateUserChallenges(activeChallenges, playerId);
                    isPlayerDataUpdated = updatePlayerDataResult.isSuccess;
                    if (isPlayerDataUpdated)
                    {
                        return new Result(isPlayerDataUpdated, error);
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
            // }
            // else
            // {
            //     error = "this challenge is not in active challenges list";
            // }

            return new Result(isPlayerDataUpdated, error);
        }

        public static List<string> GetActiveChallenges(GetUserDataResult playerData)
        {
            List<string> activeChallenges = new List<string>();
            string activeChallengesData = string.Empty;
            if (playerData.Data.ContainsKey(Constant.ACTIVE_CHALLENGES))
            {
                activeChallengesData = playerData.Data[Constant.ACTIVE_CHALLENGES].Value;
            }

            if (!string.IsNullOrEmpty(activeChallengesData))
            {
                activeChallenges = JsonConvert.DeserializeObject<List<string>>(activeChallengesData);
            }

            return activeChallenges;
        }

        public static async Task<Result> UpdateUserChallenges(List<string> activeChallenges, string playerId)
        {
            string error = string.Empty;
            bool hasUpdated = false;
            string activeChallengesJson = JsonConvert.SerializeObject(activeChallenges);
            var activeChallengesDict = new Dictionary<string, string>();
            activeChallengesDict.Add(Constant.ACTIVE_CHALLENGES, activeChallengesJson);

            var updateDataRequest = new UpdateUserDataRequest
            {
                PlayFabId = playerId,
                Data = activeChallengesDict
            };

            var updateDataResult = await PlayFabServerAPI.UpdateUserReadOnlyDataAsync(updateDataRequest);
            if (updateDataResult.Error == null)
            {
                hasUpdated = true;
            }
            else
            {
                error = updateDataResult.Error.ErrorMessage;
            }
            return new Result(hasUpdated, error);
        }

        public static async Task<Result> AddToGroup(string playerId, string groupId)
        {
            string error = string.Empty;
            bool addedToGroup = false;
            var request = new AddSharedGroupMembersRequest
            {
                PlayFabIds = new List<string> { playerId },
                SharedGroupId = groupId
            };

            var result = await PlayFabServerAPI.AddSharedGroupMembersAsync(request);
            if (result.Error == null)
                addedToGroup = true;
            else
                error = result.Error.ErrorMessage;

            return new Result(addedToGroup, error);
        }

        public static async Task<Result> RemoveFromGroup(string playerId, string groupId)
        {
            string error = string.Empty;
            bool removedFromGroup = false;
            var request = new RemoveSharedGroupMembersRequest
            {
                PlayFabIds = new List<string> { playerId },
                SharedGroupId = groupId
            };

            var result = await PlayFabServerAPI.RemoveSharedGroupMembersAsync(request);
            if (result.Error == null)
                removedFromGroup = true;
            else
                error = result.Error.ErrorMessage;

            return new Result(removedFromGroup, error);
        }

        public static async Task<Result> UpdateUsersChallenges(string currentChallengeId, List<Task> getDataTasks)
        {
            List<Task> updateDataTasks = new List<Task>();
            bool isCompleted = false;
            string error = string.Empty;
            foreach (Task<PlayFabResult<GetUserDataResult>> playerDataTask in getDataTasks)
            {
                List<string> activeChallenges = GetActiveChallenges(playerDataTask.Result.Result);
                activeChallenges.RemoveAll(c => c.Equals(currentChallengeId));

                string activeChallengesJson = JsonConvert.SerializeObject(activeChallenges);
                Dictionary<string, string> activeChallengesDict = new Dictionary<string, string>();
                activeChallengesDict.Add(Constant.ACTIVE_CHALLENGES, activeChallengesJson);

                var request = new UpdateUserDataRequest
                {
                    Data = activeChallengesDict,
                    PlayFabId = playerDataTask.Result.Result.PlayFabId
                };

                Task<PlayFabResult<UpdateUserDataResult>> updateTask = PlayFabServerAPI.UpdateUserReadOnlyDataAsync(request);
                updateDataTasks.Add(updateTask);
            }

            Task updateT = Task.WhenAll(updateDataTasks);
            await updateT;
            if (updateT.Status == TaskStatus.RanToCompletion)
            {
                isCompleted = true;
            }
            else
            {
                error = updateT.Status.ToString();
            }

            return new Result(isCompleted, error);
        }

       
        
    }

}
