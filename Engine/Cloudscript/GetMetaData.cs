using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Models;
using SocialEdge.Server.Constants;
using SocialEdge.Server.Db;
using SocialEdge.Server.Api;
using SocialEdge.Server.DataService;

namespace SocialEdge.Server.Requests
{
    public class GetMetaData
    {
        IDbHelper _dbHelper;
        ITitleContext _titleContext;
        public GetMetaData(IDbHelper dbHelper, ITitleContext titleContext)
        {
            _dbHelper = dbHelper;
            _titleContext = titleContext;
        }

        /// </summary>
        /// <param name="playerId">the playerId of the user to fetch data</param>
        /// <returns></returns>
        [FunctionName("GetMetaData")]
        public async Task<GetMetaDataResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            try
            {
                // Fetch friends
                var getFriendsT = Player.GetFriendsList(playerId);

                // Prepare client response
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.friends = new GetFriendsListResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.friends = getFriendsT.Result.Result;
                metaDataResponse.shop.catalogResult = _titleContext._catalogItems;
                metaDataResponse.shop.storeResult = _titleContext._storeItems;

                _titleContext.version = "0.0.1";

                return metaDataResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }  
}
