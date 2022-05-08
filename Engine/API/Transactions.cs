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
        public static async Task<bool> Consume(string itemId, int qty, SocialEdgePlayer playerContext)
        {
            bool used = false;
        
            if (itemId == "gems") 
            {
                SubtractUserVirtualCurrencyRequest request = new SubtractUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerContext.PlayerId;
                request.VirtualCurrency = "GM";
                var resultT = await PlayFab.PlayFabServerAPI.SubtractUserVirtualCurrencyAsync(request);
                used = resultT.Result.VirtualCurrency != null;
            }
            else if (itemId == "coins") 
            {
                SubtractUserVirtualCurrencyRequest request = new SubtractUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerContext.PlayerId;
                request.VirtualCurrency = "CN";
                var resultT = await PlayFab.PlayFabServerAPI.SubtractUserVirtualCurrencyAsync(request);
                used = resultT.Result.VirtualCurrency != null;
            }
            else // consume virtual good
            {
                ConsumeItemRequest request = new ConsumeItemRequest();
                request.PlayFabId = playerContext.PlayerId;
                request.ConsumeCount = qty;
                request.ItemInstanceId = itemId;

                var resultT = await PlayFab.PlayFabServerAPI.ConsumeItemAsync(request);
                used = resultT.Result.ItemInstanceId != null;
            }

            return used;
        }

        public static async Task<bool> Add(string itemId, int qty, SocialEdgePlayer playerContext)
        {
            bool added = false;
        
            if (itemId == "gems") 
            {
                AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerContext.PlayerId;
                request.VirtualCurrency = "GM";
                var resultT = await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
                added = resultT.Result.VirtualCurrency != null;
            } 
            else if (itemId == "coins")
            {
                AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerContext.PlayerId;
                request.VirtualCurrency = "CN";
                var resultT = await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
                added = resultT.Result.VirtualCurrency != null;
            } 
            else // add virtual good
            {
                GrantItemsToUserRequest request = new GrantItemsToUserRequest();
                request.ItemIds = new List<string> {itemId};
                request.PlayFabId = playerContext.PlayerId;
                var resutlT = await PlayFab.PlayFabServerAPI.GrantItemsToUserAsync(request);
                added = resutlT.Result.ItemGrantResults.Count != 0;
            }

            return added;
        }

        public static async Task<Dictionary<string, int>> Grant(Dictionary<string, int> rewards, SocialEdgePlayer playerContext)
        {
            Dictionary<string, int> rewarded = new Dictionary<string, int>();
        
            foreach(var item in rewards)
            {
                bool added = await Add(item.Key, (int)item.Value, playerContext);
                if (added)
                {
                    rewarded.Add(item.Key, (int)item.Value);
                }
            }
        
            return rewarded;
        }

        public static async Task<Dictionary<string, int>> Grant(BsonDocument rewards, SocialEdgePlayer playerContext)
        {
            Dictionary<string, int> rewarded = new Dictionary<string, int>();
        
            foreach(var item in rewards)
            {
                bool added = await Add(item.Name, (int)item.Value, playerContext);
                if (added)
                {
                    rewarded.Add(item.Name, (int)item.Value);
                }
            }
        
            return rewarded;
        }

        public static async Task<int> GrantTrophies(int qty, SocialEdgePlayer playerContext)
        {
            int trophies = (int)playerContext.PublicData["trpy"] + qty;
            playerContext.PublicData["trpy"] = trophies;

            BsonDocument publicData = new BsonDocument() {["PublicProfileEx"] = Utils.CleanupJsonString(playerContext.PublicDataJson)};
            await Player.UpdatePublicData(playerContext.EntityToken, playerContext.EntityId, publicData);
            return trophies;
        }
    }
}
