/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using PlayFab.ServerModels;
using System.Collections.Generic;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Context;
using System.Linq;
using MongoDB.Bson;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization;

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
        string GetShortCodeFromItemId(string itemId);

        LeagueSettingModel LeagueSettings { get; }
        EconomySettingsModel EconomySettings { get; }

        bool AdminFetchBackBufferAndSwap();
    }

    internal class TitleContextDataBuffer
    {
        private int _index;
        private GetTitleDataResult _titleData;
        private GetTitleDataResult _titleInternalData;
        private GetCatalogItemsResult _catalogItems;
        private GetStoreItemsResult _storeItems;
        private Dictionary<string, BsonDocument> _titleDataDict;
        private Dictionary<string, CatalogItem> _catalogItemsDict;
        private Dictionary<string, StoreItem> _storeItemsDict;

        private LeagueSettingModel _leagueSettings;
        private EconomySettingsModel _economySettings;

        internal GetTitleDataResult titleData => _titleData;
        internal GetTitleDataResult titleInternalData => _titleInternalData;
        internal GetCatalogItemsResult catalogItems => _catalogItems;
        internal GetStoreItemsResult storeItems => _storeItems;

        internal Dictionary<string, BsonDocument> titleDataDict => _titleDataDict;
        internal Dictionary<string, CatalogItem> catalogItemsDict => _catalogItemsDict;
        internal Dictionary<string, StoreItem> storeItemsDict => _storeItemsDict;

        internal LeagueSettingModel leagueSettings => _leagueSettings;
        internal EconomySettingsModel economySettings => _economySettings;

        public TitleContextDataBuffer(int index)
        {
            _index = index;
        }

        internal bool Fetch()
        {
            var titleDataTask = Title.GetTitleData();
            titleDataTask.Wait();
            if (titleDataTask.Result == null)
                return false;

            _titleData = titleDataTask.Result.Result;
            _titleDataDict = _titleData.Data.ToDictionary(m => m.Key, m => BsonDocument.Parse(m.Value.ToString()));

            var titleInternalDataT = Title.GetTitleInternalData();
            titleInternalDataT.Wait();
            if (titleInternalDataT.Result == null)
                return false;

            _titleInternalData = titleInternalDataT.Result.Result;
            var titleInternalDataDict = _titleInternalData.Data.ToDictionary(m => m.Key, m => BsonDocument.Parse(m.Value.ToString()));
            titleInternalDataDict.ToList().ForEach(x => _titleDataDict.Add(x.Key, x.Value));

            string storeId = _titleDataDict["Economy"]["StoreId"].ToString();
            string catalogId = _titleDataDict["Economy"]["CatalogId"].ToString();
            var getShopTask = Shop.GetShop(storeId, catalogId);
            getShopTask.Wait();
            if (getShopTask.Result == null)
                return false;

            _catalogItems = getShopTask.Result.catalogResult;
            _storeItems = getShopTask.Result.storeResult;
            _catalogItemsDict = _catalogItems.Catalog.ToDictionary(m => m.ItemId, m => m);
            _storeItemsDict = _storeItems.Store.ToDictionary(m => m.ItemId, m => m);

            _leagueSettings = new LeagueSettingModel();
            _leagueSettings.leagues = BsonSerializer.Deserialize<Dictionary<string, LeagueSettingsData>>(_titleDataDict["Leagues"].ToString());

            _economySettings = new EconomySettingsModel();
            _economySettings = BsonSerializer.Deserialize<EconomySettingsModel>(_titleDataDict["Economy"].ToString());

            Console.WriteLine("****** *********************  *****");
            Console.WriteLine("*     ( FetchTitleContext[" + _index + "] )    *");
            Console.WriteLine("****** *********************  *****");

            return true;
        }
    }

    public class TitleContext : ITitleContext
    {
        public string version { get; set; }

        private int _dataBufferIdx;
        private TitleContextDataBuffer[] _dataBuffers;

        public GetTitleDataResult TitleData { get => _dataBuffers[_dataBufferIdx].titleData; }
        public GetCatalogItemsResult CatalogItems { get => _dataBuffers[_dataBufferIdx].catalogItems; } 
        public GetStoreItemsResult StoreItems { get => _dataBuffers[_dataBufferIdx].storeItems; }

        public LeagueSettingModel LeagueSettings { get => _dataBuffers[_dataBufferIdx].leagueSettings; }
        public EconomySettingsModel EconomySettings { get => _dataBuffers[_dataBufferIdx].economySettings; }

        private TitleContextDataBuffer CurrentBuffer { get => _dataBuffers[_dataBufferIdx]; }

        public TitleContext()
        {
            SocialEdge.Init();

            version = "0.0.0";
            _dataBufferIdx = 0;
            _dataBuffers = new TitleContextDataBuffer[2];
            for (int i = 0; i < _dataBuffers.Length; i++)
                _dataBuffers[i] = new TitleContextDataBuffer(i);

            _dataBuffers[_dataBufferIdx].Fetch();
        }

        public bool AdminFetchBackBufferAndSwap()
        {
            bool status = _dataBuffers[1 - _dataBufferIdx].Fetch();
            if (status == true)
                _dataBufferIdx = 1 - _dataBufferIdx;

            return status;
        }

        public dynamic GetTitleDataProperty(string key, dynamic dict = null)
        {
            dict = dict == null ? CurrentBuffer.titleDataDict : dict;
            return dict.ContainsKey(key) ? dict[key] : null;
        }

        public CatalogItem GetCatalogItem(string ItemId)
        {
            return CurrentBuffer.catalogItemsDict.ContainsKey(ItemId) ? CurrentBuffer.catalogItemsDict[ItemId] : null;
        }
        
        public StoreItem GetStoreItem(string ItemId)
        {
            return CurrentBuffer.storeItemsDict.ContainsKey(ItemId) ? CurrentBuffer.storeItemsDict[ItemId] : null;
        }

        public string GetShortCodeFromItemId(string itemId)
        {
            CatalogItem itemData = GetCatalogItem(itemId);
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(itemData.CustomData);
            string shortCode = dictionary["shortCode"].ToString();
            return shortCode;
        }
    }
}
