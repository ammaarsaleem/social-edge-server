/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using PlayFab.ServerModels;
using System.Collections.Generic;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Context;
using System.Linq;
using MongoDB.Bson;

namespace SocialEdgeSDK.Server.DataService
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
        private GetTitleDataResult _titleInternalData;
        private GetCatalogItemsResult _catalogItems;
        private GetStoreItemsResult _storeItems;
        private Dictionary<string, BsonDocument> _titleDataDict;
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
            SocialEdge.Init();
            var titleDataTask = Title.GetTitleData();
            _titleData = titleDataTask.Result.Result;
            _titleDataDict = _titleData.Data.ToDictionary(m => m.Key, m => BsonDocument.Parse(m.Value.ToString()));

            var titleInternalDataT = Title.GetTitleInternalData();
            _titleInternalData = titleInternalDataT.Result.Result;
            var titleInternalDataDict = _titleInternalData.Data.ToDictionary(m => m.Key, m => BsonDocument.Parse(m.Value.ToString()));
            titleInternalDataDict.ToList().ForEach(x => _titleDataDict.Add(x.Key, x.Value));

            string storeId = _titleDataDict["Economy"]["StoreId"].ToString();
            string catalogId = _titleDataDict["Economy"]["CatalogId"].ToString();
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
