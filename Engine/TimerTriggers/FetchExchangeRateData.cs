/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;
using PlayFab;

namespace SocialEdgeSDK.Server.Requests
{
    public class FetchExchangeRateData
    {
        private IDataService _dataService;

        public FetchExchangeRateData(IDataService dataService) { _dataService = dataService; }

        [FunctionName("FetchExchangeRateData")]
        public void Run([TimerTrigger("0 30 3 * * *")]TimerInfo myTimer, ILogger log)
        {
            // once every minute "0 */1 * * * *"
            // once at the top of every hour "0 0 * * * *"
            // once every two hours "0 0 */2 * * *"
            // at 03:30 AM every day "0 30 3 * * *"
            // reference: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp

            if (!myTimer.IsPastDue)
            {
                //if( PlayFabSettings.staticSettings.TitleId == "918BD") //live : 918BD // dev : 9F379
                //{
                    SocialEdge.Init(null, null, _dataService);
                    SocialEdge.FetchExchangeRateData();
                    log.LogInformation("[FetchExchangeRateData]" + $"C# Timer trigger function executed at: {DateTime.Now}");
                //}
            }
        }
    }
}
