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

            GetMetaDataResult result = null;
            List<Task> tasks = null;
            string storeId = string.Empty; 
            string catalogId = string.Empty;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            try
            {
                // playerId = context.PlayerProfile.PlayerId;
                // if(!string.IsNullOrEmpty(args["player"]))
                //     playerId = args["playerId"];

                var getFriendsT = Player.GetFriendsList(playerId);
                tasks = new List<Task>();
                tasks.Add(getFriendsT);
                // tasks.Add(getFriendsT);

                var titleDataT = await Title.GetTitleData();

                if (titleDataT.Error == null)
                {
                    tasks = new List<Task>();
                    var titleDataResult = titleDataT.Result;

                    if(titleDataResult.Data.ContainsKey("StoreId"))
                        storeId = titleDataResult.Data["StoreId"];

                    if(titleDataResult.Data.ContainsKey("CatalogId"))    
                        catalogId = titleDataResult.Data["CatalogId"];
                    
                    var getShopT = Shop.GetShop(storeId,catalogId);
                    tasks.Add(getShopT);

                    await Task.WhenAll(tasks);
                    result = PrepareResult(getFriendsT.Result.Result,getShopT.Result);
                }
                else
                {
                    await Task.WhenAll(tasks);
                    result = PrepareResult(getFriendsT.Result.Result,null);
                }

                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

     
        private GetMetaDataResult PrepareResult(GetFriendsListResult getFriendsResult, GetShopResult getShopResult)
        {
            var result = new GetMetaDataResult
            {
                shop = getShopResult,
                friends = getFriendsResult
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
