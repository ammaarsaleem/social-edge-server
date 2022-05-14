/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using PlayFab;
using PlayFab.ServerModels;

namespace SocialEdgeSDK.Server.Api
{
    public static class Title
    {
        public static async Task<PlayFabResult<GetTitleDataResult>> GetTitleData()
        {   
            var request = new GetTitleDataRequest();
            var result = await PlayFabServerAPI.GetTitleDataAsync(new GetTitleDataRequest());
            return result;
        }
        
        public static async Task<PlayFabResult<GetTitleDataResult>> GetTitleInternalData()
        {   
            var request = new GetTitleDataRequest();
            var result = await PlayFabServerAPI.GetTitleInternalDataAsync(new GetTitleDataRequest());
            return result;
        }
    }
}