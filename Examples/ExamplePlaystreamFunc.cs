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
// namespace SocialEdge.Playfab
    public class OnPlayerCreated
    {
[FunctionName("ExamplePlaystreamFunc")]

        public static async Task<dynamic> ExamplePlaystreamFunc(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log)
        {
            /* Create the function execution's context through the request */
            var context = JsonConvert.DeserializeObject<PlayerPlayStreamFunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            
            PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
            var args = context.FunctionArgument;

            var playerStatUpdatedEvent = PlayFabSimpleJson.DeserializeObject<dynamic>(context.PlayStreamEventEnvelope.EventData);

            var request = new UpdateUserInternalDataRequest
            {
                PlayFabId = context.PlayerProfile.PlayerId,
                Data = new Dictionary<string, string>
                {
                    { "HighSkillContent", "true" },
                    { "XPAtHighSkillUnlock", "10" }
                }
            };

            /* Use the ApiSettings and AuthenticationContext provided to the function as context for making API calls. */
            // var serverApi = new PlayFabServerInstanceAPI(context.ApiSettings, context.AuthenticationContext);

            /* Execute the Server API request */
            var updateUserDataResponse = await PlayFabServerAPI.UpdateUserInternalDataAsync(request);

            log.LogInformation($"Unlocked HighSkillContent for {context.PlayerProfile.DisplayName}");

            return new
            {
                profile = context.PlayerProfile
            };
        }
    }