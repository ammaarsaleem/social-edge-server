using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using PlayFab.Json;
using System.Collections.Generic;
using PlayFab.DataModels;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using SocialEdge.Server.Common.Utils;
using PlayFab.Samples;
using SocialEdge.Server.DataService;
using StackExchange.Redis;
namespace SocialEdge.Server.Requests
{
    public class GetRoom
    {
        private ICache _cache;
        public GetRoom(ICache cache)
        {
            _cache = cache;
        }
        /// <summary>
        /// Fetches a room with players and their data
        /// </summary>
        /// <param name="roomId">the roomId of the user to fetch data</param>
        /// <returns>Serialized json</returns>
        [FunctionName("GetRoom")]
        public async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string roomId = args["roomId"];
            try
            {    
                Task<HashEntry[]> t =  _cache.GetRoom(roomId);
                HashEntry[] result = await t;
                if (t.IsCompletedSuccessfully)
                {
                    // result.Box();
                    
                    var serializedResult = JsonConvert.SerializeObject(result);
                    return serializedResult;
                }
                else
                {
                    throw new Exception($"An error occured while fetching the segment: {t.Exception.Message}");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}

