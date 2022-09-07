/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Requests
{
    public class TodayGamesCounter
    {
        private IDataService _dataService;

        public TodayGamesCounter(IDataService dataService) { _dataService = dataService; }

        [FunctionName("TodayGamesCounter")]
        public void Run([TimerTrigger("0 */8 * * * *")]TimerInfo myTimer, ILogger log)
        {
            // once every minute "0 */1 * * * *"
            // once at the top of every hour "0 0 * * * *"
            // once every two hours "0 0 */2 * * *"
            // reference: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp

            if (!myTimer.IsPastDue)
            {
                SocialEdge.Init(null, null, _dataService);
                SocialEdge.FetchTodayGamesCount();
                log.LogInformation("[TodayGamesCounter]" + $"C# Timer trigger function executed at: {DateTime.Now}");
            }
        }
    }
}
