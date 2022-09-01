/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using System.Net;
using SocialEdgeSDK.Server.Context;
using System.Collections.Generic;

namespace SocialEdgeSDK.Server.Requests
{
    public static class UpdatePlayerData
    {
        [FunctionName("UpdatePlayerData")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdge.Init();
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            var data = args["data"];

            Dictionary<string, string> dataDict = new Dictionary<string, string>();
            foreach (var dataItem in data)
            {
                dataDict.Add(dataItem.Name.ToString(), dataItem.Value.ToString());
            }
            
            PlayFab.ServerModels.UpdateUserDataRequest getUserDataReq = new PlayFab.ServerModels.UpdateUserDataRequest();
            getUserDataReq.PlayFabId = playerId;
            getUserDataReq.Data = dataDict;

            var getUserDataT = await PlayFab.PlayFabServerAPI.UpdateUserReadOnlyDataAsync(getUserDataReq);
            var res = getUserDataT.Result;
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
   }
}
