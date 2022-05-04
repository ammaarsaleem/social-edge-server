using System;
using SocialEdge.Server.Common.Utils;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using System.Collections.Generic;
using PlayFab.ServerModels;
using System.Threading.Tasks;

namespace SocialEdge.Server.Api
{
    public static class Transactions
    {
        public static async Task<bool> Consume(string playerId, string itemId, int qty)
        {
            bool used = false;
        
            if (itemId == "gems") 
            {
                SubtractUserVirtualCurrencyRequest request = new SubtractUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerId;
                request.VirtualCurrency = "GM";
                var resultT = await PlayFab.PlayFabServerAPI.SubtractUserVirtualCurrencyAsync(request);
                used = resultT.Result.VirtualCurrency != null;
            }
            else if (itemId == "coins") 
            {
                SubtractUserVirtualCurrencyRequest request = new SubtractUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerId;
                request.VirtualCurrency = "CN";
                var resultT = await PlayFab.PlayFabServerAPI.SubtractUserVirtualCurrencyAsync(request);
                used = resultT.Result.VirtualCurrency != null;
            }
            else // consume virtual good
            {
                ConsumeItemRequest request = new ConsumeItemRequest();
                request.PlayFabId = playerId;
                request.ConsumeCount = qty;
                request.ItemInstanceId = itemId;

                var resultT = await PlayFab.PlayFabServerAPI.ConsumeItemAsync(request);
                used = resultT.Result.ItemInstanceId != null;
            }

            return used;
        }

        public static async Task<bool> Add(string playerId, string itemId, int qty)
        {
            bool added = false;
        
            if (itemId == "gems") 
            {
                AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerId;
                request.VirtualCurrency = "GM";
                var resultT = await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
                added = resultT.Result.VirtualCurrency != null;
            } 
            else if (itemId == "coins")
            {
                AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
                request.Amount = qty;
                request.PlayFabId = playerId;
                request.VirtualCurrency = "CN";
                var resultT = await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
                added = resultT.Result.VirtualCurrency != null;
            } 
            else // add virtual good
            {
                GrantItemsToUserRequest request = new GrantItemsToUserRequest();
                request.ItemIds = new List<string> {itemId};
                request.PlayFabId = playerId;
                var resutlT = await PlayFab.PlayFabServerAPI.GrantItemsToUserAsync(request);
                added = resutlT.Result.ItemGrantResults.Count != 0;
            }

            return added;
        }

        public static async Task<Dictionary<string, int>> Grant(string playerId, Dictionary<string, int> rewards)
        {
            Dictionary<string, int> rewarded = new Dictionary<string, int>();
        
            foreach(var item in rewards)
            {
                bool added = await Add(playerId, item.Key, (int)item.Value);
                if (added)
                {
                    rewarded.Add(item.Key, (int)item.Value);
                }
            }
        
            return rewarded;
        }

        //public static async Task<bool> GrantTrophies(string playerId, int qty)
        //{
//
  //      }
    }
}
