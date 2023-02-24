/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Common;
using System.Threading.Tasks;
using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;

namespace SocialEdgeSDK.Server.Models
{
    public class ExchangeRateDocument
    {
        #pragma warning disable format                                                        
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]    public string _id;
        [BsonElement("ExchangeRateData")]                       public BsonDocument _data;
        #pragma warning restore format
    }

    public class PlayerInappDocument
    {
        #pragma warning disable format                                                        
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]    public string _id;
        [BsonElement("inappData")]                       public BsonDocument inappData;
        #pragma warning restore format
    }
    
    public class ChessPuzzleDocument
    {
         #pragma warning disable format     
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]                                public string _id;
        [BsonElement("fen")][BsonRepresentation(MongoDB.Bson.BsonType.String)]              public string fen;
        [BsonElement("moves")][BsonRepresentation(MongoDB.Bson.BsonType.String)]            public string moves;
        [BsonElement("description")][BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string description;
        [BsonElement("rating")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]            public int rating;
        [BsonElement("puzzleId")][BsonRepresentation(MongoDB.Bson.BsonType.Int32)]          public int puzzleId;
        #pragma warning restore format
    }

    public static class CommonModel
    {
        public static  Dictionary<string, object> CRM_SearchPlayerByTag(string tag)
        {
            string query = "PlayerSearchData.tag";
            var collection =  SocialEdge.DataService.GetCollection<BsonDocument>("playerSearch");
            var taskT = collection.FindOne(query, tag);
            taskT.Wait(); 

            if(taskT.Result != null){
                return taskT.Result.ToDictionary();
            }
            else{
                return null;
            }
        }
       
        public static  Dictionary<string, object> CRM_SearchPlayerById(string id, string collectionName)
        {
            var collection =  SocialEdge.DataService.GetCollection<BsonDocument>(collectionName);
            var projection = Builders<BsonDocument>.Projection.Exclude("_id");
            var taskT = collection.FindOneById<BsonDocument>(id, projection);
            taskT.Wait();

            if(taskT.Result != null){
                return taskT.Result.ToDictionary();
            }
            else{
                return null;
            }
        }

        public static string getPlayFabId(string DOBPlayerID){

            int maxLength = 16;
            string playFabId = DOBPlayerID.Substring(DOBPlayerID.Length - maxLength);
            playFabId = playFabId.ToUpper();
            return playFabId;
        }

         public static  Dictionary<string, object> CRM_SearchPlayer(string userId, string userTag)
        {
            Dictionary<string, object> data =  new Dictionary<string, object>();

            if(!string.IsNullOrEmpty(userId))
            {
                string DOBPlayerID = userId.ToLower().PadLeft(24, '0'); 
                Dictionary<string, object> playerData  = CRM_SearchPlayerById(DOBPlayerID, "playerModel");
                if(playerData != null)
                {
                    string playfabId = getPlayFabId(DOBPlayerID);
                    playerData.Add("playfabId", playfabId);
                    playerData.Add("playerId", DOBPlayerID);

                    data.Add("code", 0);
                    data.Add("userData",playerData);
                    data.Add("searchData", CRM_SearchPlayerById(DOBPlayerID, "playerSearch"));
                    data.Add("playFabData",GetPlayerInfo(playfabId));
                }
                else{
                    data.Add("code", 1);
                    data.Add("error", "Player not found : " + DOBPlayerID);
                }
            }
            else if(!string.IsNullOrEmpty(userTag))
            {
                Dictionary<string, object> searchData  = CRM_SearchPlayerByTag(userTag);
                if(searchData != null)
                {
                    string DOBPlayerID = searchData["_id"].ToString();
                    Dictionary<string, object> playerData  = CRM_SearchPlayerById(DOBPlayerID, "playerModel");

                    string playfabId = getPlayFabId(DOBPlayerID);
                    playerData.Add("playfabId", playfabId);
                    playerData.Add("playerId", DOBPlayerID);

                    data.Add("code", 0);
                    data.Add("userData",playerData);
                    data.Add("searchData", searchData);
                    data.Add("playFabData", GetPlayerInfo(playfabId));
                 }
                 else{
                    data.Add("code", 1);
                    data.Add("error", "Tag not found : " + userTag);
                }
            }

            return data;
        }

        public static Dictionary<string, object> GetPlayerInfo(string playerId)
        {
            var resulT = GetPlayerCombinedInfo(playerId);
            resulT.Wait();

            Dictionary<string, object> data = null;
            if(resulT.Result.Result != null)
            {
                GetPlayerCombinedInfoResultPayload combinedInfo =  resulT.Result.Result.InfoResultPayload;
                if (combinedInfo != null){

                     BsonDocument doc = combinedInfo.ToBsonDocument();
                     data =  doc.ToDictionary();
                }
            }
            
            return data;
        }

        public static  Dictionary<string, object> CRM_UpdatePlayer(string userId, int amount, string currencyType)
        {
            var taskT = AddVirtualCurrency(userId, amount, currencyType);
            taskT.Wait();

            Dictionary<string, object> data =  new Dictionary<string, object>();

             if(taskT.Result != null) {

                ModifyUserVirtualCurrencyResult result =  taskT.Result.Result;
                BsonDocument doc = result.ToBsonDocument();
                data.Add("code", 0);
                data.Add("MESSAGE", "player updated successfully");
                data.Add("userData",doc.ToDictionary());   
             } 
             else{
                data.Add("code", 1);
                data.Add("MESSAGE", "something went wrong");
                data.Add("error", "user not found : " + userId);
            }

            return data;

        }

      
        //Playfab functions used for CRM 
        public static async Task<PlayFabResult<GetPlayerCombinedInfoResult>> GetPlayerCombinedInfo(string playerId)
        {
            GetPlayerCombinedInfoRequest request = new GetPlayerCombinedInfoRequest();
            request.PlayFabId = playerId;
            request.InfoRequestParameters = new GetPlayerCombinedInfoRequestParams();
            request.InfoRequestParameters.GetPlayerProfile = true;
            request.InfoRequestParameters.GetUserReadOnlyData = false;
            request.InfoRequestParameters.GetPlayerStatistics = true;
            request.InfoRequestParameters.GetUserInventory = true;
            request.InfoRequestParameters.GetUserVirtualCurrency = true;
            request.InfoRequestParameters.GetUserAccountInfo = true;
            request.InfoRequestParameters.ProfileConstraints = new PlayerProfileViewConstraints();
            request.InfoRequestParameters.ProfileConstraints.ShowLocations = false;
            request.InfoRequestParameters.ProfileConstraints.ShowAvatarUrl = false;
            request.InfoRequestParameters.ProfileConstraints.ShowBannedUntil = false;
            request.InfoRequestParameters.ProfileConstraints.ShowCreated = false;
            request.InfoRequestParameters.ProfileConstraints.ShowDisplayName = true;
            request.InfoRequestParameters.ProfileConstraints.ShowLastLogin = false;

            return await PlayFab.PlayFabServerAPI.GetPlayerCombinedInfoAsync(request);
        }

         public static async Task<PlayFabResult<ModifyUserVirtualCurrencyResult>> AddVirtualCurrency(string playerId, int amount, string currencyType)
        {
            AddUserVirtualCurrencyRequest request = new AddUserVirtualCurrencyRequest();
            request.Amount = amount;
            request.PlayFabId = playerId;
            request.VirtualCurrency = currencyType;
            return await PlayFab.PlayFabServerAPI.AddUserVirtualCurrencyAsync(request);
        }

        public static void SavePlayerInappData(SocialEdgePlayerContext SocialEdgePlayer,  dynamic data, CatalogItem purchaseItem)
        {
            BsonDocument cData = BsonDocument.Parse(data.ToString());
            string remoteProductId  =  Utils.GetString(cData, "itemId");
            string currencyCode     =  Utils.GetString(cData, "currencyCode");
            double productPrice     = (double)Utils.Getfloat(cData, "productPrice");
            long currentTime        = Utils.UTCNow();

             BsonDocument serverData = new BsonDocument() // Create BSON document which document name is "Book"  
            .Add("clientdata", cData) 
            .Add("shortCode", SocialEdge.TitleContext.GetShortCodeFromItemId(remoteProductId)) 
            .Add("gems", SocialEdgePlayer.VirtualCurrency["GM"].ToString()) 
            .Add("playerId", SocialEdgePlayer.PlayerDBId)
            .Add("eloScore", SocialEdgePlayer.PlayerModel.Info.eloScore) 
            .Add("playDays", SocialEdgePlayer.PlayerModel.Info.playDays) 
            .Add("clientVersion", SocialEdgePlayer.PlayerModel.Meta.clientVersion) 
            .Add("storeId", SocialEdgePlayer.PlayerModel.Meta.storeId) 
            .Add("country", SocialEdgePlayer.PublicProfile.location) 
            .Add("displayName", SocialEdgePlayer.PublicProfile.displayName) 
            .Add("playerCreated", SocialEdgePlayer.PublicProfile.created) 
            .Add("tag", SocialEdgePlayer.PlayerModel.Info.tag) 
            .Add("currentTime", DateTime.Now)
            .Add("currentDate", Utils.getDateFormat(currentTime))
            
            .Add("productPrice", productPrice) 
            .Add("currencyCode", currencyCode); 
            
            double dollarRate = 1;
            if(productPrice > 0)
            {
                BsonDocument exchangeRateDocument = SocialEdge.GetExchangeRateData();
                if(exchangeRateDocument != null){

                    BsonDocument exchangeRateData = Utils.GetDocument(exchangeRateDocument, "ExchangeRateData");
                    long time_last_update_unix = Utils.GetLong(exchangeRateData, "time_last_update_unix");
                    string theDate = Utils.getDateFormat(time_last_update_unix * 1000);
                    BsonDocument conversion_rates = Utils.GetDocument(exchangeRateData, "conversion_rates");
                    SocialEdge.Log.LogInformation("FindDocument conversion_rates : " + conversion_rates.ToJson());
                    dollarRate = (double) Utils.Getfloat(conversion_rates, currencyCode);

                    serverData.Add("dollarRate", dollarRate);
                    serverData.Add("conversionDate", theDate);
                    serverData.Add("conversionTimestamp", time_last_update_unix);
                }
            }

            if(dollarRate <= 0 ){
                dollarRate = 1;
            }

            double dollarPrice =  Math.Round((productPrice/dollarRate), 3);

            if(dollarPrice <= 0 && purchaseItem != null){
                if(purchaseItem.VirtualCurrencyPrices.ContainsKey("RM")){
                    double cPrice = (double)purchaseItem.VirtualCurrencyPrices["RM"];
                    dollarPrice = cPrice/100;
                }
            }
            serverData.Add("dollarPrice", dollarPrice);
            SocialEdge.Log.LogInformation($"FindDocument conversion_rates for: {currencyCode} :: {dollarRate} dollarPrice: {dollarPrice}");

            PlayerInappDocument inappData = new PlayerInappDocument();
            inappData.inappData = serverData;
            SocialEdge.SavePlayerInappData(inappData);
        }

        public static Task<ChessPuzzleDocument> GetPuzzle(int puzzleId)
        {
            try
            {
                var collection =  SocialEdge.DataService.GetCollection<ChessPuzzleDocument>("chessPuzzles");
                var totalPuzzles = collection.DocumentCount;
                puzzleId = puzzleId > totalPuzzles ? new Random().Next((int)totalPuzzles/2, (int)totalPuzzles) : puzzleId;
                return collection.FindOne("puzzleId", puzzleId);
            }
            catch(Exception e)
            {
                throw new Exception($"An error occured GetPuzzle: " + e.Message);
            }
        }    
        public static void GetInappDataDailyCount(string theDate)
        {
            string COLLECTION_NAME = "inappData";
            try
            {  
                //BsonDocument documemtData = null;
                var collection = SocialEdge.DataService.GetDatabase().GetCollection<PlayerInappDocument>(COLLECTION_NAME);
                FilterDefinition<PlayerInappDocument> filter = Builders<PlayerInappDocument>.Filter.Eq("inappData.currentDate", theDate);
                var sortByScore = Builders<PlayerInappDocument>.Sort.Descending("_id");
                var projection = Builders<PlayerInappDocument>.Projection.Exclude("inappData.clientdata");
                List<BsonDocument> data = collection.Find(filter).Sort(sortByScore).Project(projection).ToList<BsonDocument>();
                if(data.Count > 0){
                   // documemtData =  data.ToBsonDocument();
                    SocialEdge.Log.LogInformation("documemtData ::: " + data);

                }
                else{
                    SocialEdge.Log.LogInformation("FindDocument ::: No data found");
                }                
            }
            catch(Exception e)
            {
                throw new Exception($"An error occured SavePlayerInappData: " + e.Message);
            }
        }

        public static async Task<PlayFabResult<SendPushNotificationResult>> SendPushNotification(SendPushNotificationRequest request)
         {
            var result = await PlayFabServerAPI.SendPushNotificationAsync(request);
            SocialEdge.Log.LogInformation("RESULT : " + result.ToJson());
            return result;  
         }
    }
}