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
    public class OnPlayerCreated
    {
        [FunctionName("OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function,  "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            Util.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;

            // var titleDataRequest = new GetTitleDataRequest
            // {
            //     Keys = new List<string>{
            //         Constant.PLAYER_SETTINGS,
            //         Constant.PLAYER_NAME_NOUNS,
            //         Constant.PLAYER_NAME_ADJECTIVES
            //     } 
            // };
            
            // var titleDataResult = await PlayFabServerAPI.GetTitleInternalDataAsync(titleDataRequest);
            // var res = titleDataResult.Result.Data[Constant.PLAYER_SETTINGS];

            var request = new SetObjectsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey
                {
                    Type = "title_player_account",
                    Id = context.CallerEntityProfile.Entity.Id
                },
                Objects = new System.Collections.Generic.List<SetObject>
                {
                    new SetObject{
                        ObjectName = Constant.PLAYER_SETTINGS,
                        EscapedDataObject = "{\"Game_Start_Delay\": \"5000\"}"
                    }
                }
            };
            
            var setObjectsResult = await PlayFabDataAPI.SetObjectsAsync(request);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

