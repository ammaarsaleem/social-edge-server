/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using SocialEdgeSDK.Server.Context;
using MongoDB.Bson;
using System.Collections.Generic;
using PlayFab.ServerModels;
using System.Threading.Tasks;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Api
{
    public static class Transactions
    {
        public static async Task<bool> Consume(string itemId, int qty, SocialEdgePlayerContext socialEdgePlayer)
        {
            bool used = false;
        
            if (itemId == "gems") 
            {
                socialEdgePlayer.PlayerEconomy.SubtractVirtualCurrency("GM", qty);
                used = true;
            }
            else if (itemId == "coins") 
            {
                socialEdgePlayer.PlayerEconomy.SubtractVirtualCurrency("CN", qty);
                used = true;
            }
            else // consume virtual good
            {
                var taskT = Player.ConsumeItem(socialEdgePlayer.PlayerId, itemId);
                taskT.Wait();
                used = taskT.Result.Result.ItemInstanceId != null; 
            }

            return used;
        }

        public static async Task<bool> Add(string itemId, int qty, SocialEdgePlayerContext socialEdgePlayer)
        {
            bool added = false;
        
            if (itemId == "gems") 
            {
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("GM", qty);
                added = true;
            } 
            else if (itemId == "coins")
            {
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", qty);
                added = true;
            } 
            else // add virtual good
            {
                var taskT = Player.GrantItem(socialEdgePlayer.PlayerId, itemId);
                taskT.Wait();
                return taskT.Result.Result.ItemGrantResults.Count != 0;
            }

            return added;
        }

        public static async Task<Dictionary<string, int>> Grant(Dictionary<string, int> rewards, SocialEdgePlayerContext socialEdgePlayer)
        {
            Dictionary<string, int> rewarded = new Dictionary<string, int>();
        
            foreach(var item in rewards)
            {
                bool added = await Add(item.Key, (int)item.Value, socialEdgePlayer);
                if (added)
                {
                    rewarded.Add(item.Key, (int)item.Value);
                }
            }
        
            return rewarded;
        }

        public static async Task<Dictionary<string, int>> Grant(BsonDocument rewards, SocialEdgePlayerContext socialEdgePlayer)
        {
            Dictionary<string, int> rewarded = new Dictionary<string, int>();
        
            foreach(var item in rewards)
            {
                bool added = await Add(item.Name, (int)item.Value, socialEdgePlayer);
                if (added)
                {
                    rewarded.Add(item.Name, (int)item.Value);
                }
            }
        
            return rewarded;
        }

        public static int GrantTrophies(int qty, SocialEdgePlayerContext socialEdgePlayer)
        {
            socialEdgePlayer.PlayerModel.Info.trophies = socialEdgePlayer.PlayerModel.Info.trophies + qty;
            return socialEdgePlayer.PlayerModel.Info.trophies;
        }
    }
}
