using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections.Generic;
using Newtonsoft.Json;
using SocialEdge.Server.Api;

namespace SocialEdge.Server.DataService
{
    public interface ITitleContext
    {
        string version { get; set; }
        GetTitleDataResult _titleData { get; set; }
        GetCatalogItemsResult _catalogItems  { get; set; }
        GetStoreItemsResult _storeItems   { get; set; }
    }
    public class TitleContext : ITitleContext
    {
        public string version { get; set; }
        public GetTitleDataResult _titleData  { get; set; }
        public GetCatalogItemsResult _catalogItems  { get; set; }
        public GetStoreItemsResult _storeItems   { get; set; }

        public TitleContext()
        {
            FetchTitleContext();
        }

        private void FetchTitleContext()
        {
            version = "0.0.0";
            string storeId = string.Empty; 
            string catalogId = string.Empty;

            var titleDataTask = Title.GetTitleData();
            _titleData = titleDataTask.Result.Result;

            /*
            if(_titleData.Data.ContainsKey("StoreId"))
                storeId = _titleData.Data["StoreId"];

            if(_titleData.Data.ContainsKey("CatalogId"))    
                catalogId = _titleData.Data["CatalogId"];

            var getShopTask = await Shop.GetShop(storeId, catalogId);
            _catalogItems = getShopTask.catalogResult;
            _storeItems = getShopTask.storeResult;
            */
        }
    }
}