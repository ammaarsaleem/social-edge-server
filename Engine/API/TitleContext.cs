using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections.Generic;
using Newtonsoft.Json;

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
            version = "0.0.0";

            //_titleData = 
            var t = SocialEdge.Server.Api.Title.GetTitleData();
            var serialized = JsonConvert.SerializeObject(t.Result);
            _titleData = JsonConvert.DeserializeObject<GetTitleDataResult>(serialized);
        }

    }
}