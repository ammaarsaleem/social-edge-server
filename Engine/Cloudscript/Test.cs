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
using System.Net.Http.Headers;
using System.Text;
using System.Net;

namespace SocialEdgeSDK.Server.Requests
{
    public class Test : FunctionContext
    {
       // private static readonly HttpClient client = new HttpClient();

        //IDbHelper _dbHelper;
        // IDataService _dataService;
        // public Test(IDataService dataService)
        // {
        //     _dataService = dataService;
        //     // _dbHelper = dbHelper;
        // }
        public Test(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("Test")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request");
            InitContext(req, log);
            string userId   = Args["userId"];
            string name    = Args["name"];
            
          // await GetLatestStateFromGSServer("");

            // var getDataT = GetLatestStateFromGSServer("");
            // getDataT.Wait();
            // if (getDataT != null)
            // {
            //     log.LogInformation("C# HTTP trigger function processed a request1");
            // }
            // else{
            //     log.LogInformation("C# HTTP trigger function processed a request");

            // }
                
         //       TestSettings();
         
            //TestDatabase();
            // TestRedis();
            //SavePlayerData(_dataService, documentId, documentData);
           // SocialEdge.FetchTodayActivePlayersCount();
            //int counter = SocialEdge.GetTodayActivePlayersCount();

            //log.LogInformation("GetTodayActivePlayersCount ::: " + counter);

            // BsonDocument playerDocument = null;
            // string deviceId = userId; //"604265AD-D11A-5EBB-9F36-345F72E5601D";
            // string fbId     = ""; //2858067714211413";
            // string appleId  = "000283.ca22434d1e984cffb362e20dbe62e710.0649"; 
            // var postT = SocialEdge.HttpPostAsync(deviceId, fbId, appleId);
            // postT.Wait();
            // if(postT.Result != null){
            //     playerDocument = postT.Result.AsBsonDocument;
            //     log.LogInformation("DATA UPDATE DONE : playerDocument : "  + playerDocument.ToJson());
            //     InitPlayerWithGsData(playerDocument);
            // }
            // else{
            //     log.LogInformation("NO UPDATE FOUND > > ");
            // }



            // GSPlayerModelDocument gsPlayerData = GetGSPlayerData(deviceId, fbId, appleId);
            // if(gsPlayerData != null)
            // {
            //     BsonDocument playerDocument = gsPlayerData.document;
            //     string userID   = Utils.GetString(playerDocument, "userId");
            //     var postT = SocialEdge.HttpPostAsync(deviceId, fbId, appleId);
            //     postT.Wait();
            //     if(postT.Result != null){
            //         playerDocument = postT.Result.AsBsonDocument;
            //         log.LogInformation("DATA UPDATE DONE : playerDocument : "  + userID);
            //     }
            //     else{
            //         log.LogInformation("NO UPDATE FOUND > > ");
            //     }
                

                // var postT = SocialEdge.HttpPostAsync(userID);
                // postT.Wait();
                // if(postT.Result != null){
                //     playerDocument = postT.Result.AsBsonDocument;
                //     log.LogInformation("DATA UPDATE DONE : playerDocument : "  + userID);
                // }
                // else{
                //     log.LogInformation("NO UPDATE FOUND > > ");
                // }

                // InitPlayerWithGsData(playerDocument);
            // }
            // else{
            //       SocialEdge.Log.LogInformation("PLAYER NOT FOUND");
            // }

            string responseMessage = string.IsNullOrEmpty(userId)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {userId}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public async Task<BsonDocument> PostAsyncCall(string userId)
        {
            //backend Ali
            //string url = "https://E393182rk0mL.preview.gamesparks.net/rs/server-send/Cd8LDzlSDvRDxhZCFy325Z1WbEZa0rLr/LogEventRequest";
            //string playerId = "5f44b3c1d6cbdd380c17a2ca";

            //backend Live
            string url = "https://X356692AvDZZ.live.gamesparks.net/rs/server-send/acbZTKyAuBKK6iy7ys2UuFblXswypab5/LogEventRequest";
            string playerId = "5fb10e0ee453fd577d73cefe";

            BsonDocument documentData = null;
            var values = new Dictionary<string, string>{
                { "@class", ".LogEventRequest" },
                { "eventKey", "CRM_Events" },
                { "playerId", playerId },
                { "requestId", "getPlayerState" },
                {"userId",userId}
            };

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())){
                string json = values.ToJson();
                streamWriter.Write(json);
            }

            try
            {
                WebResponse response = await httpWebRequest.GetResponseAsync();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseContent = reader.ReadToEnd();
                    BsonDocument responseData = BsonDocument.Parse(responseContent);
                    BsonDocument scriptData = Utils.GetDocument(responseData, "scriptData");
                    BsonDocument result     = Utils.GetDocument(scriptData, "result");
                    BsonDocument userData   = Utils.GetDocument(result, userId);
                    if(userData != null){
                        documentData  = Utils.GetDocument(userData, userId);
                        SocialEdge.Log.LogInformation("Success responseContent ::: " + userId);
                    }
                    else{
                        SocialEdge.Log.LogInformation("No Data found responseContent ::: " + responseContent);
                    }
                    
                }
            }
            catch (WebException webException)
            {
                if (webException.Response != null){
                    using (StreamReader reader = new StreamReader(webException.Response.GetResponseStream()))
                    {
                        string responseContent = reader.ReadToEnd();
                        SocialEdge.Log.LogInformation("Error responseContent ::: " + responseContent);
                    }
                }
            }

            return documentData;
        }

        // public bool SavePlayerData(IDataService dataService, string documentId, BsonDocument documentData)
        // {
        //     var collection =  dataService.GetCollection<GSTest>("usmantest");
        //     var taskT = collection.UpdateOneById<BsonDocument>(documentId, "data", documentData, true);
        //     taskT.Wait();

        //     if(taskT.Result == null){
        //         return false;
        //     }
        //     else{   
        //         return true;
        //     }
        // }

         public void TestRedis()
         {
             string theKey = "tempKey";
            ICache cacheDB = SocialEdge.DataService.GetCache();
            //SocialEdge.Log.LogInformation("CounterValue ::: " + cacheDB.KeyDelete(theKey));
            SocialEdge.Log.LogInformation("CounterValue GetValue::: " + cacheDB.GetValue(theKey));
            long CounterValue = cacheDB.Increment(theKey, 1);
            SocialEdge.Log.LogInformation("CounterValue ::: " + CounterValue);

         }
         public bool TestDatabase()
         {   
            var collection = SocialEdge.DataService.GetCollection<BsonDocument>("usmantest");
            var counterDoc = collection.IncAll("counter", 1);
            if(counterDoc.Result != null)
            {
                 var counter = (int)counterDoc.Result["counter"];
                 SocialEdge.Log.LogInformation("COUNTER VALUE  : : : : " + counter.ToString());
            }

            return true;
         }

        public void TestSettings()
         {   
            string minimumClientVersion =  Settings.MetaSettings["minimumClientVersion"].ToString();
            SocialEdge.Log.LogInformation("minimumClientVersion ::: " + minimumClientVersion);

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
         //public  void InitPlayerWithGsData( GSPlayerModelDocument gsPlayerData)
        public  void InitPlayerWithGsData( BsonDocument playerDocument)
        {
            //BsonDocument playerDocument = gsPlayerData.document;
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

                BsonDocument activeTournaments = Utils.GetDocument(priv, "activeTournaments");
                if(activeTournaments != null)
                {
                    foreach (BsonElement element in activeTournaments) 
                    {
                        SocialEdge.Log.LogInformation("NAME : : " + element.Name.ToString());
                        BsonDocument dataItem = element.Value.AsBsonDocument;
                        string shortCode  = Utils.GetString(dataItem, "shortCode");
                        if(string.Equals(shortCode, "TournamentWeeklyChampionship"))
                        {
                            long startTime    = Utils.GetLong(dataItem, "startTime");
                            int duration      = Utils.GetInt(dataItem, "duration");
                            int score         = Utils.GetInt(dataItem, "score");
                            bool isEnded = (Utils.UTCNow() <  startTime) || (Utils.UTCNow() > (startTime + duration * 60 * 1000 ));
                            if(!isEnded){
                                SocialEdge.Log.LogInformation("PLAYER ACTIVE TOURNMENT IS RUNNING ADD HIS SCORE : : " + score);
                            }
                            else{
                                SocialEdge.Log.LogInformation("PLAYER ACTIVE TOURNMENT ENDDED : : " + startTime + " SCORE : " + score);
                            }
                        }
                    }
                }

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

                }

            }

        }  
    }
}

