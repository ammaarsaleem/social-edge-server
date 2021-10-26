using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Common.Models;
using SocialEdge.Server.Constants;
using SocialEdge.Server.DataService;
namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameClose
    {
        private ICache _cache;
        public GameClose(ICache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Http trigger.Trigerred when a game is closed and the room is shutting down
        /// </summary>
        /// <param name="GameCloseRequest"></param>
        [FunctionName("GameClose")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger (AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req,
            [DurableClient] IDurableClient starter,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            string message = string.Empty;
            GameCloseRequest body = await req.Content.ReadAsAsync<GameCloseRequest>();

            if (!Utils.IsGameValid(body.GameId, out message))
            {
                message = "Game is not valid";
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }

            log.LogInformation("Game close start");

            string instanceId = await starter.StartNewAsync(Constant.GAME_CLOSE_ORCHESTRATOR, body);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            HttpResponseMessage result =  starter.CreateCheckStatusResponse(req, instanceId,true);
            if(result.IsSuccessStatusCode)
            {
                return Utils.GetSuccessResponse();
            }
            else
            {
                return Utils.GetErrorResponse(result.StatusCode.ToString());
            }
        }

        [FunctionName(Constant.GAME_CLOSE_ORCHESTRATOR)]
        public static async Task<Result> Orchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            SocialEdgeEnvironment.Init();
            var outputs = new List<OkObjectResult>();
            var reqBody = context.GetInput<GameCloseRequest>();
            List<Task> tasks = new List<Task>();
            log.LogInformation("Game close orchestrator");
            tasks.Add(context.CallActivityAsync<OkObjectResult>(Constant.GAME_CLOSE_ENGINE_ACTIVITY, reqBody));
            // tasks.Add(context.CallActivityAsync<OkObjectResult>(Constant.GAME_JOIN_ACTIVITY_TITLE, reqBody));

            Task t = Task.WhenAll(tasks);
            await t;

            if(t.IsCompletedSuccessfully)
            {
                return new Result(true,string.Empty);
            }
            else
            {
                return new Result(false,t.Status.ToString());
            }

        }
    }
}