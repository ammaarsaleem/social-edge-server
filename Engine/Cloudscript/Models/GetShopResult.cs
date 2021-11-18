using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
namespace SocialEdge.Server.Models{
    public class GetShopResult
    {
        public GetCatalogItemsResult catalogResult;
        public GetStoreItemsResult storeResult;
        // public string catalogVersion;
        // public string storeId;
        // public StoreMarketingModel marketingModel;

        // public List<StoreItem> storeItems;
        // public List<CatalogItem> catalogItems;
    }
}