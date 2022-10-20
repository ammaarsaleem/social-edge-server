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

    }
}