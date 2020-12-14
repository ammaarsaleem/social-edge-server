using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using PlayFab.Json;
using System.Collections.Generic;
using PlayFab.DataModels;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
// using PlayFab.Plugins.CloudScript;
using PlayFab.Samples;
using SocialEdge.Server.Constants;
using PlayFab;
using System.Net;
using System.Text;
using SocialEdge.Server.Util;
namespace SocialEdge.Playfab
{
    public class SetPlayerName_OnPlayerCreated
    {
        [FunctionName("SetPlayerName_OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            Util.Init(req);
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;

            string hashCode = context.PlayerProfile.PlayerId.GetHashCode().ToString();
            string newDisplayName = "Guest" + hashCode;
            var request = new PlayFab.AdminModels.UpdateUserTitleDisplayNameRequest
            {
                PlayFabId = context.PlayerProfile.PlayerId,
                DisplayName = newDisplayName
            };
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

