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
using SocialEdge.Server.Common.Utils;
namespace SocialEdge.Playfab
{
    public class Playstream_OnPlayerCreated
    {
        [FunctionName("Playstream_OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;

            string hashCode = context.PlayerProfile.PlayerId.GetHashCode().ToString();
            string newDisplayName = "NewGuest" + hashCode;
            var request = new PlayFab.AdminModels.UpdateUserTitleDisplayNameRequest
            {
                PlayFabId = context.PlayerProfile.PlayerId,
                DisplayName = newDisplayName
            };

            log.LogInformation("Noor sent a log");
            var result = await PlayFabAdminAPI.UpdateUserTitleDisplayNameAsync(request);
            log.LogDebug(JsonConvert.SerializeObject(result));

            PlayFabResult<UpdateUserDataResult> updatePlayerDataResult = null;
            var updateReadOnlyDataReq = new UpdateUserDataRequest()
            {
                 Data = new Dictionary<string, string>
                 {
                     {"eloScore", "750"}
                 },
                 PlayFabId = context.PlayerProfile.PlayerId,
             };
             updatePlayerDataResult = await PlayFabServerAPI.UpdateUserReadOnlyDataAsync(updateReadOnlyDataReq);

              log.LogDebug(JsonConvert.SerializeObject(updatePlayerDataResult));
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

