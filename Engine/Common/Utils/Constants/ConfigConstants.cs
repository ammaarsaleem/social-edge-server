/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential
using System;

namespace SocialEdgeSDK.Server.Context
{
    public static class ConfigConstants
    {
        public const string PLAYFAB_DEV_SECRET_KEY = "PLAYFAB_DEV_SECRET_KEY";
        public const string PLAYFAB_TITLE_ID = "PLAYFAB_TITLE_ID";
        public const string PLAYFAB_CLOUD_NAME = "PLAYFAB_CLOUD_NAME";
        public static string REDIS_CONNECTION_STRING => GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        public static string AZURE_STORAGE => GetEnvironmentVariable("AzureWebJobsStorage");       
        public static string MONGO_DATABASE_NAME => GetEnvironmentVariable("MONGO_DATABASE_NAME");
        public static string MONGO_CONNECTION_STRING => GetEnvironmentVariable("MONGO_CONNECTION_STRING");
        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
