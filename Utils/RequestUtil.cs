using System;
using PlayFab;
using SocialEdge.Server.Constants;
using System.Net.Http;
namespace SocialEdge.Server.Utils
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