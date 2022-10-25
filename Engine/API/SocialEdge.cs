/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using PlayFab;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.MessageService;
using Microsoft.Extensions.Logging;
using  SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.Common;
using System.Collections.Generic;

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

        public static void Init(ILogger logger = null,
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

        public static async void FetchTodayGamesCount()
        {
            const string COLLECTION = "challenges";
            long utcSeconds = (Utils.UTCNow() / 1000) - (24 * 60 * 60);
            string timeStampObjectId = utcSeconds.ToString("X").ToLower().PadRight(24, '0');
            var collection = _dataService.GetCollection<ChallengeModelDocument>(COLLECTION);
            var filter = Builders<ChallengeModelDocument>.Filter.Gt("_id", ObjectId.Parse(timeStampObjectId));

            var keyName = ConfigConstants.MONGO_DATABASE_NAME + "_todayGamesCount";
            var todayGamesCount = (int)(await collection.Count(filter));
            await _dataService.GetCache().Set(keyName, todayGamesCount.ToString());
        }

        public static async void FetchTodayActivePlayersCount()
        {
            const string COLLECTION = "playerSearch";
            var collection = _dataService.GetCollection<PlayerSearchDataModelDocument>(COLLECTION);
            var beginTime = DateTime.SpecifyKind( DateTime.Today.AddDays(-1), DateTimeKind.Utc); 
            var filter = Builders<PlayerSearchDataModelDocument>.Filter.Gt("PlayerSearchData.activeTimeStamp", beginTime);

            var keyName = ConfigConstants.MONGO_DATABASE_NAME + "_todayActivePlayersCount";
            var todayActivePlayersCount = (int)(await collection.Count(filter));
            await _dataService.GetCache().Set(keyName, todayActivePlayersCount.ToString());
        }

        public static int GetTodayActivePlayersCount()
        {
            var keyName = ConfigConstants.MONGO_DATABASE_NAME + "_todayActivePlayersCount";
            var taskT = _dataService.GetCache().Get(keyName);
            taskT.Wait();
            return taskT.Result != null ? int.Parse(taskT.Result) : 20000;
        }

        public static int GetTodayGamesCount()
        {
            var keyName = ConfigConstants.MONGO_DATABASE_NAME + "_todayGamesCount";
            var taskT = _dataService.GetCache().Get(keyName);
            taskT.Wait();
            return taskT.Result != null ? int.Parse(taskT.Result) : 70000;
        }

         public static BsonDocument SearchPlayerByTag(string tag)
        {
            BsonDocument gsPlayerData = null;
            string query = "PlayerSearchData.tag";
            var collection =  SocialEdge.DataService.GetCollection<BsonDocument>("playerSearch");
            var taskT = collection.FindOne(query, tag);
            taskT.Wait(); 

            if(taskT.Result != null){
                    gsPlayerData = taskT.Result;
                }

            return gsPlayerData;
        }
    }
}