using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using SocialEdge.Server.Models;

namespace SocialEdge.Server.Api
{
    public static class Shop
    {
        public static async Task<GetShopResult> GetShop(string shopId, string playerId=null)
        {
            var storeRequest = new GetStoreItemsServerRequest
            {
                StoreId = shopId,
                PlayFabId = playerId
            };

            var storeResult = await PlayFabServerAPI.GetStoreItemsAsync(storeRequest);

            var catalogRequest = new GetCatalogItemsRequest
            {
                CatalogVersion = storeResult.Result.CatalogVersion
            };

            var catalogResult = await PlayFabServerAPI.GetCatalogItemsAsync(catalogRequest);

            var result = new GetShopResult
            {
                storeItems = storeResult.Result.Store,
                storeId = storeResult.Result.StoreId,
                marketingModel = storeResult.Result.MarketingData,
                catalogVersion = storeResult.Result.CatalogVersion,
                catalogItems = catalogResult.Result.Catalog
            };

            return result;
        }
    }
}