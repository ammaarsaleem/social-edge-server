using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Mvc;
namespace SocialEdge.Playfab.Photon.Events
{
    public class GameEvent
    {
        [FunctionName("GameEvent")]
        public async Task<OkObjectResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequestMessage req, ILogger log)
        {
            // Get request body
            GameEventRequest body = await req.Content.ReadAsAsync<GameEventRequest>();

            // Logs for testing. Remove this in production
            var okMsg = $"{req.RequestUri} - Recieved Game Event";
            log.LogInformation(okMsg);

            var response = new
            {
                ResultCode = 0,
                Message = "Success"
            };

            return new OkObjectResult(response);
        }
    }
}