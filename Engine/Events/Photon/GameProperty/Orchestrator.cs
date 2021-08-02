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
namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameProperties
    {

        /// <summary>
        /// Http trigger.Trigerred whenever a property is updated
        /// </summary>
        /// <param name="GamePropertiesRequest"></param>
          [FunctionName("GameProperties")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger (AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req,
            [DurableClient] IDurableClient starter,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            string message = string.Empty;
            GamePropertiesRequest body = await req.Content.ReadAsAsync<GamePropertiesRequest>();

            if (!Utils.IsGameValid(body.GameId, body.UserId, out message))
            {
                message = "Game is not valid";
                log.LogInformation(message);
                return Utils.GetErrorResponse(message);
            }

            log.LogInformation("Game properties start");

            string instanceId = await starter.StartNewAsync(Constant.GAME_PROPERTY_ORCHESTRATOR, body);

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

        [FunctionName(Constant.GAME_PROPERTY_ORCHESTRATOR)]
        public static async Task<Result> Orchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context, ILogger log)
        {
            SocialEdgeEnvironment.Init();
            var outputs = new List<OkObjectResult>();
            var reqBody = context.GetInput<GamePropertiesRequest>();
            List<Task> tasks = new List<Task>();
            log.LogInformation("Game properties orchestrator");
            tasks.Add(context.CallActivityAsync<OkObjectResult>(Constant.GAME_PROPERTY_ENGINE_ACTIVITY, reqBody));
            tasks.Add(context.CallActivityAsync<OkObjectResult>(Constant.GAME_PROPERTY_TITLE_ACTIVITY, reqBody));

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