using PlayFab;
using Newtonsoft.Json;
using System.Net.Http;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SocialEdge.Playfab.Photon.Events
{
    public class GameProperties
    {
        [FunctionName("GameProperties")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req, ILogger log,
            [DurableClient] IDurableOrchestrationClient starter)
        {
            GamePropertiesRequest body = await req.Content.ReadAsAsync<GamePropertiesRequest>();
            log.LogInformation(body.Type);
            log.LogInformation(JsonConvert.SerializeObject(body.Properties));
            string message;
            if (!Utils.IsGameValid(body.GameId, body.UserId, out message))
            {
                var errorResponse = new
                {
                    ResultCode = 1,
                    Error = message
                };
                return new OkObjectResult(errorResponse);
            }

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
    
            private async Task EngineMethod()
            {}
    }
}