using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections.Generic;

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
        }

    }
}