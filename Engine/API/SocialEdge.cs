/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using PlayFab;
using System.Net.Http;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.MessageService;
using Microsoft.Extensions.Logging;

namespace SocialEdgeSDK.Server.Context
{
    public static class SocialEdge
    {
        private static IDataService _dataService = null;
        private static IMessageService _messageService = null;
        private static ITitleContext _titleContext = null;
        private static ILogger _log = null;

        public static ITitleContext TitleContext { get => _titleContext; }
        public static IDataService DataService { get => _dataService; }
        public static IMessageService MessageService { get => _messageService; }
        public static ILogger Log { get => _log; }

        public static void Init(HttpRequestMessage req = null,
                                ILogger logger = null,
                                ITitleContext titleContext = null,
                                IDataService dataService = null,
                                IMessageService messageService = null)
        {
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            {
                PlayFabSettings.staticSettings.TitleId = Environment.GetEnvironmentVariable(ConfigConstants.PLAYFAB_TITLE_ID,
                                                                                        EnvironmentVariableTarget.Process);
            }
            if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.DeveloperSecretKey))
            {
                PlayFabSettings.staticSettings.DeveloperSecretKey = Environment.GetEnvironmentVariable(ConfigConstants.PLAYFAB_DEV_SECRET_KEY,
                                                                                                    EnvironmentVariableTarget.Process);
            }

            if (dataService != null && _dataService == null)
            {
                _dataService = dataService;
            }

            if (messageService != null && _messageService == null)
            {
                _messageService = messageService;
            }

            if (titleContext != null && _titleContext == null)
            {
                _titleContext = titleContext;
            }

            if (logger != null && _log == null)
            {
                _log = logger;
            }
        }
    }
}