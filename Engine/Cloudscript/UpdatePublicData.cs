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
using PlayFab.DataModels;
using SocialEdgeSDK.Server.Api;
using PlayFab;

namespace SocialEdgeSDK.Server.Requests
{
    public static class UpdatePublicData
    {
        [FunctionName("UpdatePublicData")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdge.Init();
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            var data = args["data"];

            var getTitleTokenT = await Player.GetTitleEntityToken();

            List<SetObject> dataList =  new List<SetObject>();
            foreach (var dataItem in data)
            {
                SetObject obj = new SetObject();
                obj.ObjectName = dataItem.Name.ToString();
                obj.DataObject = dataItem.Value.ToString();
                dataList.Add(obj);
            }

            SetObjectsRequest setObjectsDataReq = new SetObjectsRequest();
            setObjectsDataReq.Entity = new PlayFab.DataModels.EntityKey();
            setObjectsDataReq.AuthenticationContext = new PlayFabAuthenticationContext();
            setObjectsDataReq.AuthenticationContext.EntityToken = getTitleTokenT.Result.EntityToken;
            setObjectsDataReq.Entity.Id = context.CallerEntityProfile.Entity.Id;
            setObjectsDataReq.Entity.Type = "title_player_account";
            setObjectsDataReq.Objects = dataList;

            var setObjectsResT = await PlayFabDataAPI.SetObjectsAsync(setObjectsDataReq);
            var res = setObjectsResT.Result;
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

