/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using SocialEdgeSDK.Server.DataService;
using PlayFab.Samples;

namespace SocialEdgeSDK.Server.Requests
{       
    public class UpdateVideoProgress : FunctionContext
    {
        public UpdateVideoProgress(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("UpdateVideoProgress")]
        public bool Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            var videoId = data["videoId"].Value.ToString();
            var progress = (float)data["progress"].Value;

            if(SocialEdgePlayer.PlayerModel.Info.videosProgress.ContainsKey(videoId))
            {
                SocialEdgePlayer.PlayerModel.Info.videosProgress[videoId] = progress;
            }
            else
            {
                SocialEdgePlayer.PlayerModel.Info.videosProgress.Add(videoId, progress);
            }

            CacheFlush();
            return true;
        }
    }
}
