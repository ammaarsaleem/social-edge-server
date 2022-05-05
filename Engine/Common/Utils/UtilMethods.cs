using System.Threading.Tasks;
using PlayFab;
using System;

namespace SocialEdge.Server.Common.Utils
{
    public static class UtilFunc
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
    }

}