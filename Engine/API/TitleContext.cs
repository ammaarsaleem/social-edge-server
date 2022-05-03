using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SocialEdge.Server.Api;
using SocialEdge.Server.Common.Utils;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SocialEdge.Server.DataService
{
    public interface ITitleContext
    {
        string version { get; set; }
        GetTitleDataResult TitleData { get; }
        GetCatalogItemsResult CatalogItems { get; } 
        GetStoreItemsResult StoreItems { get; }

        dynamic GetTitleDataProperty(string key, dynamic dict = null);
        CatalogItem GetCatalogItem(string ItemId);
        StoreItem GetStoreItem(string ItemId);

    }
    public class TitleContext : ITitleContext
    {
        public string version { get; set; }
        private GetTitleDataResult _titleData;
        private GetCatalogItemsResult _catalogItems;
        private GetStoreItemsResult _storeItems;
        private Dictionary<string, JObject> _titleDataDict;
        private Dictionary<string, CatalogItem> _catalogItemsDict;
        private Dictionary<string, StoreItem> _storeItemsDict;

        public GetTitleDataResult TitleData { get => _titleData; }
        public GetCatalogItemsResult CatalogItems { get => _catalogItems; } 
        public GetStoreItemsResult StoreItems { get => _storeItems; }

        public TitleContext()
        {
            FetchTitleContext();
        }

        private void FetchTitleContext()
        {
            version = "0.0.0";
            SocialEdgeEnvironment.Init();
            var titleDataTask = Title.GetTitleData();
            _titleData = titleDataTask.Result.Result;
            _titleDataDict = _titleData.Data.ToDictionary(m => m.Key, m => JObject.Parse(m.Value.ToString()));
            string storeId = _titleDataDict["Economy"]["StoreId"].Value<string>();
            string catalogId = _titleDataDict["Economy"]["CatalogId"].Value<string>();
            var getShopTask = Shop.GetShop(storeId, catalogId);
            _catalogItems = getShopTask.Result.catalogResult;
            _storeItems = getShopTask.Result.storeResult;
            _catalogItemsDict = _catalogItems.Catalog.ToDictionary(m => m.ItemId, m => m);
            _storeItemsDict = _storeItems.Store.ToDictionary(m => m.ItemId, m => m);
        }

        public dynamic GetTitleDataProperty(string key, dynamic dict = null)
        {
            dict = dict == null ? _titleDataDict : dict;
            return dict.ContainsKey(key) ? dict[key] : null;
        }

        public CatalogItem GetCatalogItem(string ItemId)
        {
            return _catalogItemsDict.ContainsKey(ItemId) ? _catalogItemsDict[ItemId] : null;
        }
        
        public StoreItem GetStoreItem(string ItemId)
        {
            return _storeItemsDict.ContainsKey(ItemId) ? _storeItemsDict[ItemId] : null;
        }
    }
}