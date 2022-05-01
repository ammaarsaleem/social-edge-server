using System;
using PlayFab;
using System.Net.Http;
using SocialEdge.Server.DataService;

namespace SocialEdge.Server.Common.Utils
{
    public static class SocialEdgeEnvironment
    {
        private static IDataService _dataService = null;
        private static ITitleContext _titleContext = null;

        public static ITitleContext TitleContext { get => _titleContext; }

        public static void Init(HttpRequestMessage req = null, ITitleContext titleContext = null, IDataService dataService = null)
        {   
            if(string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {    PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable(ConfigConstants.PLAYFAB_TITLE_ID, 
                                                                                        EnvironmentVariableTarget.Process);
            }
            if(string.IsNullOrEmpty(PlayFabSettings.staticSettings.DeveloperSecretKey))
            {    PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(ConfigConstants.PLAYFAB_DEV_SECRET_KEY, 
                                                                                                    EnvironmentVariableTarget.Process);
            }

            if (dataService != null && _dataService == null)
            {
                _dataService = dataService;
            }

            if (titleContext != null && _titleContext == null)
            {
                _titleContext = titleContext;
            }
        }
    }
}