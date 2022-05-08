/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using PlayFab;
using System;

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
    }
}