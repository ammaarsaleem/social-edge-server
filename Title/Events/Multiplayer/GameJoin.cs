using PlayFab;
using Newtonsoft.Json;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using SocialEdgeSDK.Server.Constants;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SocialEdgeSDK.Playfab.Photon.Events
{
    public partial class GameJoin
    {
        [FunctionName(Constant.GAME_JOIN_TITLE_ACTIVITY)]

        public async Task<OkObjectResult> TitleActivityFunc(
            [ActivityTrigger] GameLeaveRequest body, ILogger log)
        {
            
           
            return Utils.GetSuccessResponse();
        }
    
    }
}