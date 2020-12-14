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
using System.Text;
using System.Net;
using SocialEdge.Server.Util;
namespace SocialEdge.Playfab
{
    public class RoomJoined
    {
//      EventRaised arguments
//      EvCode: custom event code.
//      Data: custom event data as sent from client SDK.
//      State: a serialized snapshot of the room's full state. It's sent only if SendState webflag is set when calling OpRaiseEvent 
//              and "IsPersistent" setting is set to true.
//      AuthCookie: an encrypted object invisible to client, optionally returned by web service upon successful custom authentication. 
//              It's sent only if SendAuthCookie webflag is set when calling OpRaiseEvent

        [FunctionName("RoomJoined")]
        public async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] 
            HttpRequest req,ILogger log)
        {
            Util.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            log.LogInformation("Webhook triggered");
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
            WriteTitleEventRequest titleEventRequest = new WriteTitleEventRequest();
            titleEventRequest.EventName = "room_joined";
            titleEventRequest.Body = new System.Collections.Generic.Dictionary<string, object> {
                { "WebHookEvent", "Room Joined" }
            };
            await PlayFabServerAPI.WriteTitleEventAsync(titleEventRequest);
            var tasks = new List<Task<PlayFabResult<UpdatePlayerStatisticsResult>>>();
            // var context = await Util.Init(req);
            
            if(args!=null)
            {
                
                var eventRaised = args["EvCode"];
                var data = args["Data"];
                if(eventRaised=="UpdStats")
                {
                    /*extract properties*/
                    foreach(var player in data)
                    {
                        /*Example code which sets value to 1*/

                        var request = new PlayFab.ServerModels.UpdatePlayerStatisticsRequest 
                        {
                            PlayFabId = player.Id,
                            Statistics = new List<StatisticUpdate>
                            {
                                new StatisticUpdate{StatisticName="won", Value=1}
                            }
                        };
                        var updateStatsTask = PlayFabServerAPI.UpdatePlayerStatisticsAsync(request);
                        tasks.Add(updateStatsTask);
                    }        

                    var result = Task.WhenAll(tasks);
                    if(tasks.FindAll(t=>t.IsCompletedSuccessfully).Count == tasks.Count)
                    {
                        /*all tasks have completed successfully*/
                    }
                }
            }
            
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}

