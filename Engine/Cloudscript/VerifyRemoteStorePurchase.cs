/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;
using System.Collections.Generic;

namespace SocialEdgeSDK.Server.Requests
{
     public class RemotePurchaseResult
    {
        public int responseCode = 0;
        public string responseMessage = "Success";
        public string itemId;
        public bool isAdded;
        public long removeAdsTimeStamp;
        public int addedGems = 0;
        public long addedCoins = 0;
        public int piggyBank = 0;
        public string dynamicBundleShortCode = "";
        public Dictionary<string, string> dynamicGemSpotBundle = null;
        public long rvUnlockTimestamp;
        public Dictionary<string, object> boughtItem = null;
        public long piggyBankExpiryTimestamp;
    }

    public class VerifyRemoteStorePurchase : FunctionContext
    {
       // IDataService _dataService;
        public VerifyRemoteStorePurchase(ITitleContext titleContext, IDataService dataService) {Base(titleContext, dataService); }

        [FunctionName("VerifyRemoteStorePurchase")]
            public  RemotePurchaseResult  Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
            {
                log.LogInformation("C# HTTP trigger function processed a request.");
                InitContext<FunctionExecutionContext<dynamic>>(req, log);

                //var sharedSecret = "39c303891fcf4e9ca7f00f144e78d1e0";
                var data = Args["data"];
                string  remoteProductId = data["itemId"].Value;
                string instanceId       = data["instanceId"].Value;
                long expiryTime         = data["expiryTimeStamp"].Value;
                string subscriptionType = data["subscriptionType"].Value;

               try
                {
                    RemotePurchaseResult result = new RemotePurchaseResult();
                    dynamic commonSettings = Settings.CommonSettings;
                    
                     PlayFab.ServerModels.CatalogItem purchaseItem =  SocialEdge.TitleContext.GetCatalogItem(remoteProductId);
                     if(purchaseItem == null)
                     {
                             result.responseCode = 1;
                             result.responseMessage = "Item Not found Catalog:" + remoteProductId;;
                             return result;
                     }

                    ItemInstance invenotryObject = SocialEdgePlayer.PlayerEconomy.GetInventoryItem(SocialEdgePlayer, instanceId);
                    if(invenotryObject == null)
                    {
                        result.responseCode = 1;
                        result.responseMessage = "Item Not found in invenotry:" + instanceId;;
                        return result;
                        
                    } 

                    log.LogInformation("Before VerifyRemoteStorePurchase : VirtualCurrency GM : " + SocialEdgePlayer.VirtualCurrency["GM"].ToString());

                    if(remoteProductId.Contains("piggybank"))
                    {
                        int gemsCredit  = SocialEdgePlayer.PlayerModel.Economy.piggyBankGems;
                        var addVirualCurrencyT = Player.AddVirtualCurrency(SocialEdgePlayer.PlayerId, gemsCredit, "GM");
                        addVirualCurrencyT.Wait();

                        result.addedGems = gemsCredit;
                        result.piggyBank = gemsCredit;

                        SocialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = 0;
                        SocialEdgePlayer.PlayerModel.Economy.piggyBankGems = 0;
                    }

                    result.boughtItem = new Dictionary<string, object>();
                    result.boughtItem.Add("shortCode", SocialEdge.TitleContext.GetShortCodeFromItemId(remoteProductId));
                    result.boughtItem.Add("quantity", invenotryObject.RemainingUses);
            
                    if(remoteProductId.Contains("subscription")) 
                    {
                        SocialEdgePlayer.PlayerModel.Economy.subscriptionExpiryTime = expiryTime;
                        SocialEdgePlayer.PlayerModel.Economy.subscriptionType = subscriptionType;
                    }

                    string[] itemsList = remoteProductId.Split(".");  
                    string remoteProductShortCode = itemsList[itemsList.Length-1];

                    if(Settings.DynamicPurchaseTiers.Contains(remoteProductShortCode))
                    {
                        SocialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount = 0;
                        SocialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier = "A";
                        string tierValue = Settings.DynamicPurchaseTiers[remoteProductShortCode].ToString();

                        if(Utils.compareTier(tierValue, SocialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier) == 1)
                        {
                            SocialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier = tierValue;
                        }
                    } 

                    string tempItemId = Settings.DynamicDisplayBundles[SocialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier][SocialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier].ToString();
                    result.dynamicBundleShortCode = Utils.GetShortCode(tempItemId);
                    result.dynamicGemSpotBundle = SocialEdgePlayer.PlayerEconomy.GetDynamicGemSpotBundle();
                    result.rvUnlockTimestamp = SocialEdgePlayer.PlayerModel.Economy.rvUnlockTimestamp;
                    result.piggyBankExpiryTimestamp = SocialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp; 

                    SocialEdgePlayer.CacheFlush();

                    log.LogInformation("AFTER VerifyRemoteStorePurchase : VirtualCurrency GM : " + SocialEdgePlayer.VirtualCurrency["GM"].ToString());
                    log.LogInformation("VerifyRemoteStorePurchase : called addedGems : " + result.ToString());
                    return result;
                }
                catch (Exception e)
                 {
                     throw new Exception($"An error occured : " + e.Message);
                 }

            }
    }
}
