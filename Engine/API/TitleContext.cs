using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using SocialEdge.Server.Api;

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
        private Dictionary<string, dynamic> _titleDataDict;
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
            string storeId = string.Empty; 
            string catalogId = string.Empty;

            var titleDataTask = Title.GetTitleData();
            _titleData = titleDataTask.Result.Result;

            if(_titleData.Data.ContainsKey("StoreId"))
                storeId = _titleData.Data["StoreId"];

            if(_titleData.Data.ContainsKey("CatalogId"))    
                catalogId = _titleData.Data["CatalogId"];

            var getShopTask = Shop.GetShop(storeId, catalogId);
            _catalogItems = getShopTask.Result.catalogResult;
            _storeItems = getShopTask.Result.storeResult;

            _titleDataDict = new Dictionary<string, dynamic> ();
            if (_titleData != null)
            {
                foreach (var obj in _titleData.Data)
                {
                    _titleDataDict.Add(obj.Key, JsonConvert.DeserializeObject<dynamic>(obj.Value));
                }
            }

            _catalogItemsDict = new Dictionary<string, CatalogItem>();
            if (_catalogItems != null)
            {
                foreach(var item in _catalogItems.Catalog)
                {
                    _catalogItemsDict.Add(item.ItemId, item);
                }
            }

            _storeItemsDict = new Dictionary<string, StoreItem>();
            if (_storeItems != null)
            {
                foreach(var item in _storeItems.Store)
                {
                    _storeItemsDict.Add(item.ItemId, item);
                }
            }
        }

        public dynamic GetTitleDataProperty(string key, dynamic dict = null)
        {
            dict = dict == null ? _titleDataDict : dict;
            return dict.ContainsKey(key) ? dict[key] : null;
        }

        public CatalogItem GetCatalogItem(string ItemId)
        {
            return _titleDataDict.ContainsKey(ItemId) ? _titleDataDict[ItemId] : null;
        }
        
        public StoreItem GetStoreItem(string ItemId)
        {
            return _storeItemsDict.ContainsKey(ItemId) ? _storeItemsDict[ItemId] : null;
        }
    }
}
