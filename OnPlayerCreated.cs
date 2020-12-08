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

namespace SocialEdge.Playfab
{
    public class OnPlayerCreated
    {
        [FunctionName("OnPlayerCreated")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
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
                        ObjectName = "playerCustomData",
                        EscapedDataObject = "{\"Meta\":{\"backendAppVersion\":20,\"androidURL\":\"https://play.google.com/store/apps/details?id=com.turbolabz.instantchess.android.googleplay\",\"iosURL\":\"https://itunes.apple.com/us/app/chess/id1386718098?mt=8\",\"rateAppThreshold\":1,\"nthWinsRateApp\":10,\"contactSupportURL\":\"https://contactus.huuugegames.com\",\"minimumClientVersion\":\"6.2.20\"}}"
                    }
                }
            };

            var a = await PlayFabDataAPI.SetObjectsAsync(request);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

