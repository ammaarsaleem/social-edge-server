using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using SocialEdge.Server.Common.Utils;
using PlayFab.Samples;
using System.Collections.Generic;
using SocialEdge.Server.Db;
using SocialEdge.Server.DataService;
namespace SocialEdge.Server.Requests
{
    public class Test
    {
        IDbHelper _dbHelper;
        IDataService _dataService;
        public Test(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
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
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
           
            try
            {
               
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

