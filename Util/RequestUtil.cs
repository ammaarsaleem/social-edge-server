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
using System.Net.Http;
namespace SocialEdge.Server.Util
{
    public static class RequestUtil
    {
        public static void Init(HttpRequestMessage req)
        {
            // var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.ReadAsStringAsync());
            
            PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable(Constant.PLAYFAB_TITLE_ID, 
                                                                                        EnvironmentVariableTarget.Process);
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
        }
    }
}