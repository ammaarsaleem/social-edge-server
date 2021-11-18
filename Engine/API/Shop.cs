using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdge.Server.Models;

namespace SocialEdge.Server.Api
{
    public static class Shop
    {
        public static async Task<GetShopResult> GetShop(string shopId, string catalogId, string playerId=null)
        {
            var storeRequest = new GetStoreItemsServerRequest
            {
                StoreId = shopId,
                PlayFabId = playerId,
                CatalogVersion = catalogId
            };

            var storeT =  PlayFabServerAPI.GetStoreItemsAsync(storeRequest);

            var catalogRequest = new GetCatalogItemsRequest
            {
                CatalogVersion = catalogId
            };


            var catalogT =  PlayFabServerAPI.GetCatalogItemsAsync(catalogRequest);

            await Task.WhenAll(storeT,catalogT);

            var result = new GetShopResult
            {
                // storeItems = storeT.Result?.Result?.Store,
                // storeId = storeT.Result?.Result?.StoreId,
                // marketingModel = storeT.Result?.Result?.MarketingData,
                // catalogVersion = storeT.Result?.Result?.CatalogVersion,
                // catalogItems = catalogT.Result?.Result?.Catalog

                storeResult = storeT.Result.Result,
                catalogResult = catalogT.Result.Result
            };

            return result;
        }
    }
}