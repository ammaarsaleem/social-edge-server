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
using PlayFab.ProfilesModels;

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
                var getTitleTokenT = await Player.GetTitleEntityToken();
                var getFriendsT = await Player.GetFriendsList(playerId);
                var friends = getFriendsT.Result.Friends;
                var getFriendProfilesT = await Player.GetFriendProfiles(friends, getTitleTokenT.Result.EntityToken);

                // Prepare client response
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.friends = new GetFriendsListResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.titleData = new GetTitleDataResult();
                metaDataResponse.friends = getFriendsT.Result;
                metaDataResponse.shop.catalogResult = _titleContext.CatalogItems;
                metaDataResponse.shop.storeResult = _titleContext.StoreItems;
                metaDataResponse.titleData = _titleContext.TitleData;
                metaDataResponse.friendsProfiles = getFriendProfilesT.Result;

                var gameSettings = _titleContext.GetTitleDataProperty("GameSettings");
                var metaSettings = _titleContext.GetTitleDataProperty("Meta", gameSettings);
                int c = _titleContext.GetTitleDataProperty("backendAppVersion", metaSettings);

                return metaDataResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }  
}
