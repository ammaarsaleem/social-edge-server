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
using Xunit;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class Test
    {
        //IDbHelper _dbHelper;
        IDataService _dataService;
        public Test(IDataService dataService)
        {
            _dataService = dataService;
            // _dbHelper = dbHelper;
        }
        /// <summary>
        /// Wild search for a player by namee
        /// </summary>
        /// <param name="name">the name of the user to fetch</param>
        /// <returns>serialiazed json</returns>
        [FunctionName("Test")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            userId = userId ?? data?.userId;

            GSPlayerModelDocument gsPlayerData =  GetGSPlayerData(_dataService);
            if(gsPlayerData != null)
            {
                InitPlayerWithGsData(gsPlayerData, log);
            }

            string responseMessage = string.IsNullOrEmpty(userId)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {userId}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }

        public GSPlayerModelDocument GetGSPlayerData(IDataService _dataService)
        {
            GSPlayerModelDocument gsPlayerData  = null;
            string deviceId = "F9B9EEDD-DCB8-4B2D-A016-38817F457245";
            var collection =  _dataService.GetCollection<GSPlayerModelDocument>("gsDataCollection");
            var taskT = collection.FindOne("PlayerDataModel.deviceId", deviceId);
            taskT.Wait(); 

            if(taskT.Result != null){
                gsPlayerData = taskT.Result;
            }

            return gsPlayerData;
        }
        public  void InitPlayerWithGsData( GSPlayerModelDocument gsPlayerData, ILogger log)
        {
            BsonDocument playerDocument = gsPlayerData.document;
            string deviceId = Utils.GetString(playerDocument, "deviceId"); 
            BsonDocument sparkPlayer = Utils.GetDocument(playerDocument, "sparkPlayer");

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

                if(sparkPlayer.Contains("challengeCount"))
                {
                    BsonArray dailyGamesCount = sparkPlayer["challengeCount"].AsBsonArray;
                     if(dailyGamesCount != null)
                     {
                        for(int i=0; i<dailyGamesCount.Count; i++)
                        {
                            BsonDocument dailyData = dailyGamesCount[i].AsBsonDocument;
                            BsonElement dailyElement = dailyData.GetElement(0);
                            string dateKey = dailyElement.Name;
                            BsonDocument dailyMatches = dailyElement.Value.AsBsonDocument;
                            GameResults result = new GameResults();
                            result.won = Utils.GetInt(dailyMatches, "win");
                            result.lost = Utils.GetInt(dailyMatches, "loss");
                            result.drawn = Utils.GetInt(dailyMatches, "draw");

                            DateTime oDate = DateTime.Parse(dateKey);
                            var dateKey1 = oDate.ToShortDateString();

                            log.LogInformation("DATE : " + dateKey1);

                        }
                     }
                }

                BsonArray dailyEventRewards = priv["dailyEventRewards"].AsBsonArray;
                if(dailyEventRewards != null && dailyEventRewards.Count > 0){
                    for(int i=0; i<dailyEventRewards.Count; i++)
                    {
                        BsonDocument dataItem = dailyEventRewards[i].AsBsonDocument;
                        int gems  = Utils.GetInt(dataItem, "gems");
                        int coins = Utils.GetInt(dataItem, "coins");
                        DailyEventRewards reward = new DailyEventRewards();
                        reward.gems = gems;
                        reward.coins = coins;
                    }

                }

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

