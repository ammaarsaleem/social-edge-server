using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.DataService;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using System;
using SocialEdgeSDK.Server.Models;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Common;
using System.Collections.Generic;
using PlayFab.Samples;
using System.Net.Http;

namespace SocialEdgeSDK.Server.Requests
{
    public class Test : FunctionContext
    {
        //IDbHelper _dbHelper;
        // IDataService _dataService;
        // public Test(IDataService dataService)
        // {
        //     _dataService = dataService;
        //     // _dbHelper = dbHelper;
        // }

        public Test(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("Test")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            InitContext(req, log);
            string userId  =  Args["userId"];
            string deviceId = Args["deviceId"].ToString();
            string fbId     = Args["fbId"].ToString();
            string appleId  = Args["appleId"].ToString();

            GSPlayerModelDocument gsPlayerData = GetGSPlayerData(deviceId, fbId, appleId);
            if(gsPlayerData != null)
            {
                 InitPlayerWithGsData(gsPlayerData);
            }
            else{
                  SocialEdge.Log.LogInformation("PLAYER NOT FOUND");
            }

            string responseMessage = string.IsNullOrEmpty(userId)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {userId}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public GSPlayerModelDocument GetGSPlayerData(string deviceId, string fbId, string appleId)
        {
            SocialEdge.Log.LogInformation("PLAYER IDs deviceId: " + deviceId + " fbId: "+fbId + " appleId: "+appleId);
            GSPlayerModelDocument gsPlayerData  = null;
            string query = null;
            string findId = null;

            if(!string.IsNullOrEmpty(fbId)){
                query = "PlayerDataModel.facebookId";
                findId = fbId;
            }
            else if(!string.IsNullOrEmpty(appleId)){
                query = "PlayerDataModel.appleId";
                findId = appleId;
            }
            else{
                query = "PlayerDataModel.deviceId";
                findId = deviceId;
            }
            
            SocialEdge.Log.LogInformation("FIND QUERY: " + query + " findId: "+findId);

            if(!string.IsNullOrEmpty(findId)){

                var collection =  SocialEdge.DataService.GetCollection<GSPlayerModelDocument>("gsDataCollection");
                var taskT = collection.FindOne(query, findId);
                taskT.Wait(); 

                if(taskT.Result != null){
                    gsPlayerData = taskT.Result;
                }
            }
            
            return gsPlayerData;
        }
        public  void InitPlayerWithGsData( GSPlayerModelDocument gsPlayerData)
        {
            BsonDocument playerDocument = gsPlayerData.document;
            string deviceId = Utils.GetString(playerDocument, "deviceId"); 
            BsonDocument sparkPlayer = Utils.GetDocument(playerDocument, "sparkPlayer");
            string displayName = Utils.GetString(sparkPlayer,"displayName"); 
            SocialEdge.Log.LogInformation("PLAYER FOUND : " + displayName);

            int numCoins = Utils.GetInt(sparkPlayer, "coins"); 
            int tournamentMaxScore = Utils.GetInt(sparkPlayer, "tournamentMaxScore");
            int dollarSpent = Utils.GetInt(sparkPlayer, "dollarSpent");

            BsonDocument playerData = Utils.GetDocument(playerDocument, "playerData");
            if(playerData != null)
            {
                BsonDocument pub = Utils.GetDocument(playerData, "pub");
                BsonDocument priv = Utils.GetDocument(playerData, "priv");

                long eventTimeStamp      = Utils.GetLong(priv,"eventTimeStamp");
                long dailyEventExpiryTimestamp = Utils.GetLong(priv,"dailyEventExpiryTimestamp");

                 BsonDocument inventory = Utils.GetDocument(sparkPlayer, "inventory");
                Dictionary<string, int> gsItemDictionary = new Dictionary<string, int>();

                //Add object in Inventory
                if(inventory != null)
                {
                   List<string> iventoryItemsList = new List<string>();
                    foreach (BsonElement element in inventory) 
                    {
                        string shortCode = element.Name;
                        if(shortCode == "DefaultOwnedItemsV1" || shortCode == "DefaultOwnedItemsV2"){
                            continue;
                        }

                        CatalogItem itemData = null;
                        if(SocialEdge.TitleContext.GetCatalogItemDictionary().ContainsKey(shortCode)){
                            itemData = SocialEdge.TitleContext.GetCatalogItemDictionary()[shortCode];

                        if(itemData != null && !iventoryItemsList.Contains(itemData.ItemId)){
                            iventoryItemsList.Add(itemData.ItemId);
                            BsonValue value = element.Value;
                            gsItemDictionary.Add(itemData.ItemId, value.AsInt32);
                        }                
                    }

                }

                int coins = 0;
                int gems  = 0;
                foreach (var item in gsItemDictionary)
                {
                    CatalogItem itemData = SocialEdge.TitleContext.GetCatalogItem(item.Key);

                    if(itemData.Bundle != null && itemData.Bundle.BundledVirtualCurrencies != null)
                    {
                        if(itemData.Bundle.BundledVirtualCurrencies.ContainsKey("CN"))
                        {
                            coins += ((int)itemData.Bundle.BundledVirtualCurrencies["CN"]);
                        }

                        if(itemData.Bundle.BundledVirtualCurrencies.ContainsKey("GM"))
                        {
                            gems += ((int)itemData.Bundle.BundledVirtualCurrencies["GM"]);
                        }
                    }
                }

                SocialEdge.Log.LogInformation("TOTAL DEDCUT COINS: " + coins + " GEMS:" + gems);

                if(coins > 0){
                    SocialEdge.Log.LogInformation("TOTAL DEDCUT COINS: " + coins);
                }

                if(gems > 0){
                    SocialEdge.Log.LogInformation("TOTAL DEDCUT GEMS: " + gems);
                }

                SocialEdge.Log.LogInformation("gsItemDictionary : " + gsItemDictionary.ToJson());

                // BsonArray playerActiveInventory = priv["playerActiveInventory"].AsBsonArray;
                // if(playerActiveInventory != null && playerActiveInventory.Count > 0){
                //     for(int i=0; i<playerActiveInventory.Count; i++)
                //     {
                //         BsonDocument dataItem = playerActiveInventory[i].AsBsonDocument;
                //         string itemType  = GetString(dataItem, "kind");
                //         string itemValue = GetString(dataItem, "shopItemKey");
                //         if(itemType == "Skin")
                //         {
                //             string skin = itemValue;
                //             log.LogInformation("Skin:" + skin);
                //         }
                //         else if(itemType == "Avatar")
                //         {
                //             string Avatar = itemValue;
                //             log.LogInformation("Avatar:" + Avatar);
                //         }
                //         else if(itemType == "AvatarBgColor")
                //         {
                //             string AvatarBgColor = itemValue;
                //             log.LogInformation("AvatarBgColor:" + AvatarBgColor);
                //         }
                //     }

                // }
                }

            }

        }  
    }
}

