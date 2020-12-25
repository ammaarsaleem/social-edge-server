using System;
using PlayFab;
using SocialEdge.Server.Common.Constants;
using System.Net.Http;
namespace SocialEdge.Server.Common.Utils
{
    public static class SocialEdgeEnvironment
    {
        public static void Init(HttpRequestMessage req=null)
        {   
            PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable(Constant.PLAYFAB_TITLE_ID, 
                                                                                        EnvironmentVariableTarget.Process);
            PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(Constant.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
        }
    }
}