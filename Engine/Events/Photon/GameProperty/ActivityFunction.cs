using PlayFab;
using Newtonsoft.Json;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using SocialEdge.Server.Constants;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SocialEdge.Playfab.Photon.Events
{
    public partial class GameProperties
    {
        [FunctionName(Constant.GAME_PROPERTY_ENGINE_ACTIVITY)]

        public async Task<OkObjectResult> ActivityFunc(
            [ActivityTrigger] GamePropertiesRequest body, ILogger log)
        {
            
           
            return Utils.GetSuccessResponse();
        }
    
    }
}