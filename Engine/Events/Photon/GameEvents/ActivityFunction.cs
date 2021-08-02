using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SocialEdge.Server.Constants;
namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameEvent
    {
        [FunctionName(Constant.GAME_EVENT_ENGINE_ACTIVITY)]
        public async Task<OkObjectResult> ActivityFunc(
            [ActivityTrigger] GameEventRequest body, ILogger log)
        {

         
            return new OkObjectResult("");
        }
    }
}