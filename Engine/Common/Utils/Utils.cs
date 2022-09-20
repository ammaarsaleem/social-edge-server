/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using PlayFab;
using System;
using SocialEdgeSDK.Server.Context;
using MongoDB.Bson;

namespace SocialEdgeSDK.Server.Common
{
    public static class Utils
    {
        public static bool IsTaskCompleted(Task<PlayFabBaseResult> task) 
        {
            if(task.IsCompletedSuccessfully && task.Result.Error==null)
                return true;
            else
                return false;
        }

        public static long UTCNow()
        {
            return (long)DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static long ToUTC(DateTime dateTime)
        {
            return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds; 
        }

        public static DateTime EpochToDateTime(long milli)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(milli);
        }

        public static string CleanupJsonString(string json)
        {
            json = json.Replace("\\n", "").Replace("\n", "").Replace("\\", "");

            if (json[0] == '\"')
            {
                char[] ar = json.ToCharArray();
                ar[0] = ' ';
                json = new string(ar);
            }

            if (json[json.Length - 1] == '\"')
            {
                char[] ar = json.ToCharArray();
                ar[json.Length - 1] = ' ';
                json = new string(ar);
            }

            return json;
        }

        public static DateTime StartOfDay(DateTime dateTime) 
        {  
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);  
        }  

        public static DateTime EndOfDay(DateTime dateTime) 
        {  
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, 999);
        }  

        public static int compareTier(string t1, string t2)
        {
            int t1Int = int.Parse(t1.Substring(1, t1.Length-1));
            int t2Int = int.Parse(t2.Substring(1, t2.Length-1));
            return t1Int == t2Int ? 0 : t1Int < t2Int ? -1 : 1;
        }

        public static string AppendItemId(string itemId)
        {
            return "com.turbolabz.instantchess." + itemId;
        }

         public static string GetShortCode(string tempId)
         {
            string itemId     = AppendItemId(tempId);
            string shortCode  =  SocialEdge.TitleContext.GetShortCodeFromItemId(itemId);
            return shortCode;
         }

         public static int GetRandomInteger(int min, int max)
         {
            return min + (int)(new Random().NextDouble() * (max - min));
         }

        public static double RoundToNearestMultiple(double numToRound, double numToRoundTo) 
        {
            return Math.Round(numToRound / numToRoundTo) * numToRoundTo;
        }

        public static string DbIdFromPlayerId(string playerId)
        {
            return playerId.ToLower().PadLeft(24, '0');
        }

        
        public static bool CompareVersions(string v1, string v2)
        {
            var v1Split = v1.Split('.');
            var v2Split = v2.Split('.');

            if(v1Split.Length > v2Split.Length)
            {
                return false;
            }

            for (var i = 0; i < v1Split.Length; ++i)
            {
                if (int.Parse(v1Split[i]) > int.Parse(v2Split[i]))
                { 
                    return false;
                }
            }

            return true;
        }

        public static BsonDocument GetDocument(BsonDocument doc, string key)
        {
            bool keyExists = doc.TryGetValue(key, out BsonValue keyData);
            if(keyExists){
                return BsonDocument.Parse(keyData.ToString());
            }
            else{
                return null;
            }
        }

        public static BsonArray GetArray(BsonDocument doc, string key)
        {
            BsonArray returnValue = null;
            bool keyExists = doc.TryGetValue(key, out BsonValue keyData);
            if(keyExists){
                if(keyData.IsBsonArray){
                    returnValue = keyData.AsBsonArray;
                }
            }
            return returnValue;
        }

        public static int GetInt(BsonDocument doc, string key)
        {
            int returnValue = 0;
            bool keyExists = doc.TryGetValue(key, out BsonValue keyData);
            if(keyExists)
            {
                if (keyData.IsDouble == true){
                    returnValue = (int)keyData.AsDouble;
                }
                else if (keyData.IsInt32 == true){
                     returnValue = keyData.AsInt32;
                }
            }
            return returnValue;
        }

        public static long GetLong(BsonDocument doc, string key)
        {
            long returnValue = 0;
            bool keyExists = doc.TryGetValue(key, out BsonValue keyData);
            if(keyExists)
            {
                if (keyData.IsDouble == true){
                    returnValue = (long)keyData.AsDouble;
                }
                else if (keyData.IsInt32 == true){
                     returnValue = (long)keyData.AsInt32;
                }
            }            
            return returnValue;
        }
        public static string GetString(BsonDocument doc, string key)
        {
            bool keyExists = doc.TryGetValue(key, out BsonValue keyData);
            return keyExists && keyData.BsonType != BsonType.Null ? keyData.AsString : string.Empty;
        }
        public static bool GetBool(BsonDocument doc, string key)
        {
            return doc.Contains(key) ? doc[key].AsBoolean : false;
        }

        public static float Getfloat(BsonDocument doc, string key)
        {
            float returnValue = 0;
             bool keyExists = doc.TryGetValue(key, out BsonValue keyData);
            if(keyExists)
            {
                if (keyData.IsDecimal128 == true){
                    returnValue = (float)keyData.AsDecimal128;
                }
                else if (keyData.IsDouble == true){
                    returnValue = (float)keyData.AsDouble;
                }
                else if (keyData.IsInt32 == true){
                     returnValue = (float)keyData.AsInt32;
                }
            }            
            return returnValue;
        }
    }
}