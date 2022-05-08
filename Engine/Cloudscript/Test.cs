using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using SocialEdgeSDK.Server.Context;
using PlayFab.Samples;
using System.Collections.Generic;
using SocialEdgeSDK.Server.Db;
using SocialEdgeSDK.Server.DataService;
// using SocialEdgeSDK.Server.Realtime;
namespace SocialEdgeSDK.Server.Requests
{
    public class Test
    {
        IDbHelper _dbHelper;
        IDataService _dataService;
        public Test(IDataService dataService)
        {
            _dataService = dataService;
            // _dbHelper = dbHelper;
        }
        /// <summary>
        /// Wild search for a player by namee
        /// </summary>
        /// <param name="name">the name of the user to fetch</param>
        /// <returns>serialiazed json</returns>
        [FunctionName("Test")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdge.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            var collection = _dataService.GetCollection("BooksTest");
            var res = await collection.FindOneById("1",null);
            try
            {
            //     Realtime.PubSub.AddToGroup()
            //     string message = "abc";
            //    Realtime.PubSub.SendToUser("FromTest",playerId,message );
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

