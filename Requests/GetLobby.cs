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

namespace SocialEdge.Playfab
{
    public class GetLobby
    {
        [FunctionName("GetLobby")]
        public async Task<List<PlayerProfile>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            var request = new GetPlayersInSegmentRequest
            {
                SegmentId = Constant.ALL_PLAYERS
            };

            var result = await PlayFabServerAPI.GetPlayersInSegmentAsync(request);
            if(result.Error!=null)
            {
                throw new Exception($"An error occured while fetching the segment: {result.Error.GenerateErrorReport()}");
            }
            return result.Result.PlayerProfiles;
        }
    }
}

