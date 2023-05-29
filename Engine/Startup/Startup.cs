
/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

#define USE_REDIS

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Linq;
using SocialEdgeSDK.Server.Db;
using SocialEdgeSDK.Server.DataService;
using StackExchange.Redis;
using Azure.Storage.Blobs;
using SocialEdgeSDK.Server.MessageService;
using SocialEdgeSDK.Server.Context;

[assembly: FunctionsStartup(typeof(SocialEdgeSDK.Playfab.Startup))]

namespace SocialEdgeSDK.Playfab
{
    public class Startup : FunctionsStartup
    {
        /// <summary>
        /// Configures dependencies of services
        /// </summary>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });

            var config = (IConfiguration)builder.Services.First(d => d.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            builder.Services.AddSingleton((s) =>
            {
                MongoClientSettings settings = MongoClientSettings.FromConnectionString(ConfigConstants.MONGO_CONNECTION_STRING);
                // change some fields based on your needs
                settings.MaxConnectionIdleTime = System.TimeSpan.FromMinutes(1);
                settings.MinConnectionPoolSize = 5;
                settings.MaxConnectionPoolSize = 800;
                MongoClient client = new MongoClient(settings);
                return client;
            });

#if USE_REDIS
            builder.Services.AddSingleton((r) =>
            {
                string connString  = ConfigConstants.REDIS_CONNECTION_STRING;
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connString);
                return redis;
            });
#endif
            builder.Services.AddSingleton((t) =>
            {
                string connString = ConfigConstants.AZURE_STORAGE;
                BlobServiceClient blobServiceClient = new BlobServiceClient(connString);
                return blobServiceClient;
            });
#if USE_REDIS
            builder.Services.AddSingleton<ICache,Cache>();
#endif
            builder.Services.AddSingleton<IDbHelper,DbHelper>();
            builder.Services.AddSingleton<IDataService,DataService>();
            builder.Services.AddSingleton<ITitleContext, TitleContext>();
            builder.Services.AddSingleton<IMessageService, MessageService>();
        }
    }
}