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
namespace SocialEdge.Server.Requests
{
    public class GetMetaData
    {
        IDbHelper _dbHelper;
        public GetMetaData(IDbHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        /// <summary>
        
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

            string playerTitleId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;
            GetMetaDataResult result = null;
            GetShopResult shopResultModel = null;
            string playerSettings = string.Empty;
            List<Task> tasks = null;
            string playerId = string.Empty;

            try
            {
                playerId = context.PlayerProfile.PlayerId;
                playerId = args["playerId"];

                var getFriendsT = Player.GetFriendsList(playerId);
                var titleDataT = await Title.GetTitleData();
                

                if (titleDataT.Error == null)
                {
                    tasks = new List<Task>();
                    var titleDataResult = titleDataT.Result;

                    if(titleDataResult.Data.ContainsKey("StoreId"))
                    {
                        string storeId = titleDataResult.Data["StoreId"];
                        var getShopT = Shop.GetShop(storeId);
                        tasks.Add(getShopT);
                    }
                }

                tasks.Add(getFriendsT);
                await Task.WhenAll(tasks);
                result = PrepareResult(getFriendsT.Result.Result,shopResultModel);

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

     
        private GetMetaDataResult PrepareResult(GetFriendsListResult friendsList, GetShopResult shop)
        {
            var result = new GetMetaDataResult
            {
                shopResult = shop,
                friendsListResult = friendsList
            };

            return result;
        }
  

        private async void GetStore(string storeId = null)
        {
            PlayFabResult<GetStoreItemsResult> storeItems = null;
            PlayFabResult<GetCatalogItemsResult> catalogItems = null;
            if(!string.IsNullOrEmpty(storeId))
            {
                var storeRequest = new PlayFab.ServerModels.GetStoreItemsServerRequest();
                storeItems = await PlayFabServerAPI.GetStoreItemsAsync(storeRequest);              
            }

            var catalogRequest = new PlayFab.ServerModels.GetCatalogItemsRequest();
            catalogItems = await PlayFabServerAPI.GetCatalogItemsAsync(catalogRequest);

        }

    }  

}
