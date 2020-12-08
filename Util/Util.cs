using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using PlayFab.Samples;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdge.Playfab.Models;
using SocialEdge.Server.Constants;
namespace SocialEdge.Server.Util
{
    public static class Util
    {
        public static async Task<FunctionExecutionContext<dynamic>> Init(HttpRequest req)
        {
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            
            PlayFabSettings.staticSettings.TitleId = context.TitleAuthenticationContext.Id;
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);

            return context;
        }
    }
}