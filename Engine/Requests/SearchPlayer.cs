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
namespace SocialEdge.Server.Requests
{
    public class SearchPlayerByName
    {
        IDbHelper _dbHelper;
        public SearchPlayerByName(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        [FunctionName("SearchPlayerByName")]
        public async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string queryName = args["name"];
            try
            {
                string name = string.Empty;
                Dictionary<string,object> resultDict = await _dbHelper.SearchPlayerByName(queryName);
                if(resultDict!=null && resultDict.ContainsKey("playerId"))
                {
                    name = resultDict["playerId"].ToString();   
                }
                
                else
                {
                    throw new Exception($"An error occured while searching player with name: {queryName}");
                }
                
                return name;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

