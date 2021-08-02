using System;
using PlayFab;
using System.Net.Http;
namespace SocialEdge.Server.Common.Utils
{
    public static class SocialEdgeEnvironment
    {
        public static void Init(HttpRequestMessage req=null)
        {   
            if(string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {    PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable(ConfigConstants.PLAYFAB_TITLE_ID, 
                                                                                        EnvironmentVariableTarget.Process);
            }
            if(string.IsNullOrEmpty(PlayFabSettings.staticSettings.DeveloperSecretKey))
            {    PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(ConfigConstants.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
            }
        }
    }
}