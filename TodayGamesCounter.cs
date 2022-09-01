using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
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
        public void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            // once every minute "0 */1 * * * *"
            // once at the top of every hour "0 0 * * * *"
            // once every two hours "0 0 */2 * * *"
            // reference: https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp

            SocialEdge.Init(null, null, _dataService);
            SocialEdge.FetchTodayGamesCount();
            log.LogInformation("[TodayGamesCounter]" + $"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
