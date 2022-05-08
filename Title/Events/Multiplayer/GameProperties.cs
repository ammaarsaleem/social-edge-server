using PlayFab;
using Newtonsoft.Json;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SocialEdgeSDK.Server.Constants;
namespace SocialEdgeSDK.Playfab.Photon.Events
{
    public partial class GameProperties
    {
        [FunctionName(Constant.GAME_PROPERTY_TITLE_ACTIVITY)]
        /// <summary>
        /// Reads gamee state to check match status.
        /// If match is finished, increments the score statistic of the winner 
        /// </summary>
        /// <param name="GamePropertiesRequest"></param>
        public async Task<OkObjectResult> TitleActivityFunc(
            [ActivityTrigger] GamePropertiesRequest body, ILogger log)
        {
            
            log.LogInformation(body.Type);
            log.LogInformation(JsonConvert.SerializeObject(body.Properties));
            string message=string.Empty;
            
            if (body.Properties.Count > 0)
            {
                bool? winnerExists = false;
                object winnerId = null;
                object matchFinished = null;
                if (body.Properties?.TryGetValue("FK", out matchFinished) == true)
                {
                    winnerExists = body.Properties?.TryGetValue("WinnerId", out winnerId);
                    if (winnerExists == true)
                    {
                        var playFabId = winnerId.ToString();
                        log.LogInformation("Winner Id is: " + winnerId);
                        var getStatsRequest = new GetPlayerStatisticsRequest
                        {
                            StatisticNames = new List<string> { "Score" },
                            PlayFabId = playFabId
                        };

                        PlayFabResult<GetPlayerStatisticsResult> getPlayerStatsResult = await PlayFabServerAPI.GetPlayerStatisticsAsync(getStatsRequest);
                        if (getPlayerStatsResult.Error == null)
                        {
                            int oldScore = 0;
                            log.LogInformation("player statistics fetched");
                            var statistics = getPlayerStatsResult?.Result?.Statistics;
                            if (statistics != null && statistics.Count > 0)
                            {
                                StatisticValue scoreStatistic = statistics.Where(s => s.StatisticName.Equals("Score")).FirstOrDefault();
                                oldScore = scoreStatistic == null ? 0 : scoreStatistic.Value;
                                log.LogInformation("player score: " + scoreStatistic.Value);
                            }
                            int newScore = oldScore + 1;
                            log.LogInformation("player score incremented");
                            StatisticUpdate updatedScore = new StatisticUpdate
                            {
                                StatisticName = "Score",
                                Value = newScore
                            };
                            var updateStatsRequest = new UpdatePlayerStatisticsRequest
                            {
                                Statistics = new List<StatisticUpdate> { updatedScore },
                                PlayFabId = playFabId
                            };
                            log.LogInformation("request created");
                            PlayFabResult<UpdatePlayerStatisticsResult> updateStatsResult = await PlayFabServerAPI.
                                                                                            UpdatePlayerStatisticsAsync(updateStatsRequest);

                            if (updateStatsResult.Error == null)
                            {
                                log.LogInformation("Score updated");
                                return Utils.GetSuccessResponse();
                            }

                        }
                        else
                        {
                            message = "Unablee to get player statistics";
                            log.LogInformation(message);
                        }
                    }
                }
            }
            return Utils.GetErrorResponse(message);
        }
    
    }
}