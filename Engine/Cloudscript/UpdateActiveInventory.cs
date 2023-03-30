/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.Samples;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Requests
{
    public class UpdateActiveInventoryResult
    {
        public bool status;
        public List<SpinWheelReward> freeSpinRewards;
        public List<SpinWheelReward> fortuneSpinRewards;
    }

    public class UpdateActiveInventory : FunctionContext
    {
         public UpdateActiveInventory(ITitleContext titleContext) { Base(titleContext); }

        [FunctionName("UpdateActiveInventory")]
        public UpdateActiveInventoryResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            var result = new UpdateActiveInventoryResult();

            if (data.ContainsKey("activeSkinId"))
            {
                string activeSkinId = data["activeSkinId"].ToString();
                List<PlayerInventoryItem> activeInventory = SocialEdgePlayer.PlayerModel.Info.activeInventory;
                PlayerInventoryItem item = activeInventory.Where<PlayerInventoryItem>(item => item.kind == "Skin").FirstOrDefault();
                if (item != null)
                {
                    item.key = activeSkinId;
                    result.status = true;
                }
            }
            else if(data.ContainsKey("updateSpinWheelRewards"))
            {
                SocialEdgePlayer.PlayerEconomy.ProcessSpinWheelRewards();
                result.freeSpinRewards = SocialEdgePlayer.PlayerModel.Economy.freeSpinRewards;
                result.fortuneSpinRewards = SocialEdgePlayer.PlayerModel.Economy.fortuneSpinRewards;
                result.status = true;
            }
            else if(data.ContainsKey("selectedEmojiId"))
            {
                int selectedEmojiId = int.Parse(data["selectedEmojiId"].ToString());
                SocialEdgePlayer.PlayerModel.Info.selectedEmojiId = selectedEmojiId;
                result.status = true;
            }

            CacheFlush();
            return result;
        }
    }
}
