using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
// using PlayFab.ServerModels;
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
using SocialEdge.Playfab.Constants;
using PlayFab;
using PlayFab.ClientModels;

namespace SocialEdge.Playfab
{
    public class Authenticate
    {
        [FunctionName("AuthenticateWithCustomId")]
        public async void Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            var request = new LoginWithCustomIDRequest
            {
                CreateAccount = true,
                CustomId="TestPlayer",
                InfoRequestParameters = ProfileInfoGetSettings()
            };
            
            var objectsRequest = new GetObjectsRequest
            {
                 
            };
            
            var result = await PlayFabClientAPI.LoginWithCustomIDAsync(request);

            if(result.Error==null)
            {

            }
            else
            {
                throw new Exception($"An error occured while fetching the segment: {result.Error.GenerateErrorReport()}");
            }
            // return result.Result.PlayerProfiles;
        }

        private GetPlayerCombinedInfoRequestParams ProfileInfoGetSettings()
        {
            var infoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetUserData = true,
                GetUserAccountInfo = true,
                GetPlayerProfile = true,
                GetUserInventory = true,
                GetPlayerStatistics = true,
                GetUserReadOnlyData = true
            };

            return infoRequestParameters;
        }
    }
}

