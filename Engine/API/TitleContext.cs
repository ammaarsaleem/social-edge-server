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
        PlayFabResult<GetTitleDataResult> _titleData { get; set; }

        string _titleDataStr { get; set; }
        GetCatalogItemsResult _catalogItems  { get; set; }
        GetStoreItemsResult _storeItems   { get; set; }
        
    }
    public class TitleContext : ITitleContext
    {
        public string version { get; set; }
        public PlayFabResult<GetTitleDataResult> _titleData  { get; set; }

        public string _titleDataStr { get; set; }
        public GetCatalogItemsResult _catalogItems  { get; set; }
        public GetStoreItemsResult _storeItems   { get; set; }

        public TitleContext()
        {
            version = "0.0.0";

            var titleDataTask = SocialEdge.Server.Api.Title.GetTitleData();
            _titleData = titleDataTask.Result;
            _titleDataStr = JsonConvert.SerializeObject(_titleData);
            //_titleDataStr = JsonConvert.DeserializeObject<GetTitleDataResult>(serialized).ToString();
        }

    }
}