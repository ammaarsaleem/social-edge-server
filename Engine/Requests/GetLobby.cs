using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using SocialEdge.Server.Common.Utils;
using PlayFab.Samples;
using SocialEdge.Server.Constants;
using PlayFab;

namespace SocialEdge.Server.Requests
{
    public class GetLobby
    {
        [FunctionName("GetLobby")]
        public async Task<List<PlayerProfile>> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            try
            {
                var request = new GetPlayersInSegmentRequest
                {
                    SegmentId = Constant.COMMUNITY_SEGMENT
                };

                var result = await PlayFabServerAPI.GetPlayersInSegmentAsync(request);
                if (result.Error != null)
                {
                    throw new Exception($"An error occured while fetching the segment: {result.Error.GenerateErrorReport()}");
                }
                return result.Result.PlayerProfiles;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

