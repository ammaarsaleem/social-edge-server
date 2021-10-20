using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;

namespace SocialEdge.Server.Api
{
    public static class Title
    {
        public static async Task<PlayFabResult<GetTitleDataResult>> GetTitleData()
        {   
            var request = new GetTitleDataRequest();
            var result = await PlayFabServerAPI.GetTitleDataAsync(new GetTitleDataRequest());
            return result;
        }
        
    }
}