using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common.Models;
using SocialEdgeSDK.Server.Constants;
using SocialEdgeSDK.Server.DataService;
namespace SocialEdgeSDK.Playfab.Photon.Events
{
    public partial class GameJoin
    {
        private ICache _cache;
        public GameJoin(ICache cache)
        {
            _cache = cache;
        }
        /// <summary>
        /// Http trigger.Trigerred when a player joins a room
        /// </summary>
        /// <param name="GameLeaveRequest"></param>
        [FunctionName("GameJoin")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger (AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req,
            [DurableClient] IDurableClient starter,
            ILogger log)
        {
            SocialEdge.Init(req);
            string message = string.Empty;
            GameLeaveRequest body = await req.Content.ReadAsAsync<GameLeaveRequest>();

            if (!Utils.IsGameValid(body.GameId, body.UserId, out message))
            {
                message = "Game is not valid";
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }

            log.LogInformation("Game join start");

            string instanceId = await starter.StartNewAsync(Constant.GAME_JOIN_ORCHESTRATOR, body);

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

        [FunctionName(Constant.GAME_JOIN_ORCHESTRATOR)]
        public static async Task<Result> Orchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            SocialEdge.Init();
            var outputs = new List<OkObjectResult>();
            var reqBody = context.GetInput<GameLeaveRequest>();
            List<Task> tasks = new List<Task>();
            log.LogInformation("Game join orchestrator");
            tasks.Add(context.CallActivityAsync<OkObjectResult>(Constant.GAME_JOIN_ENGINE_ACTIVITY, reqBody));
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