/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Newtonsoft.Json;
using SocialEdgeSDK.Server.Context;
using PlayFab.Samples;
using SocialEdgeSDK.Server.DataService;
using StackExchange.Redis;

namespace SocialEdgeSDK.Server.Requests
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
            SocialEdge.Init();
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
                 throw new Exception($"An error occured : " + e.Message);
            }
        }
    }
}

