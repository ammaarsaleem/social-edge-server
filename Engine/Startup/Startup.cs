
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

                //MongoClient client = new MongoClient(ConfigConstants.MONGO_CONNECTION_STRING);
                //MongoClient client = new MongoClient("mongodb+srv://MyMongoDBUser:MyMongoDBUserPassword@socialedgecluster.hsxfp.mongodb.net/Development?retryWrites=true&w=majority");
                // MongoClient client = new MongoClient("mongodb+srv://MyMongoDBUser:MyMongoDBUserPassword@socialedgecluster.hsxfp.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
                // string connString = Environment.GetEnvironmentVariable(ConfigConstants.MONGO_CONNECTION_STRING, EnvironmentVariableTarget.Process);
                // MongoClient client = new MongoClient(connString);
                return client;
            });

#if USE_REDIS
            builder.Services.AddSingleton((r) =>
            {
                string connString  = ConfigConstants.REDIS_CONNECTION_STRING;
                //string connString = "socialedgeserver.redis.cache.windows.net:6380,password=Gf7aTTxpvRfk+IKGqCbQwW7j+bWIKcr5B6bXAqj+ZSQ=,ssl=True,abortConnect=False";
                //string connString = Environment.GetEnvironmentVariable(ConfigConstants.REDIS_CONNECTION_STRING, EnvironmentVariableTarget.Process);
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connString);
                return redis;
            });
#endif

            // builder.Services.AddSingleton((s) =>
            // {
            //     MongoClient client = new MongoClient("mongodb+srv://MyMongoDBUser:MyMongoDBUserPassword@socialedgecluster.hsxfp.mongodb.net/Development?retryWrites=true&w=majority");
            //     string connString = Environment.GetEnvironmentVariable(ConfigConstants.REDIS_CONNECTION_STRING, EnvironmentVariableTarget.Process);
            //     ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connString);
            //     IDataService dataService = new DataService(client, redis);
            //     return dataService;
            // });

            // builder.Services.AddSingleton((t) =>
            // {
            //     string connString = "DefaultEndpointsProtocol=https;AccountName=storageaccountchess812e;AccountKey=Y4gMwGaJVvdin1xywKyyLVCUbiN0zy6WAg3NW9owPD+l7TPdj/i5dJKr+MRlCEhVGOO3LhcjBKbmMcTZYWxQnQ==;EndpointSuffix=core.windows.net";
            //     string connContainer = "playerprofile";
            //     //string connString = "DefaultEndpointsProtocol=https;AccountName=chessstarsblobstorage;AccountKey=9UE+ONgiQuIt9/vrUQtN2VUuDGSkMiLEi6qLlpNL8koLMHo4d68xmTMx4T/CMiiuNj7143VCawev+AStDkzjTw==;EndpointSuffix=core.windows.net";
            //     //string connContainer = "dlc";
            //     var containerClientPlayer = new BlobContainerClient(connString, connContainer);
            //     return containerClientPlayer;
            // });

            builder.Services.AddSingleton((t) =>
            {
                string connString = ConfigConstants.AZURE_STORAGE;
                //string connString = "DefaultEndpointsProtocol=https;AccountName=storageaccountchess812e;AccountKey=Y4gMwGaJVvdin1xywKyyLVCUbiN0zy6WAg3NW9owPD+l7TPdj/i5dJKr+MRlCEhVGOO3LhcjBKbmMcTZYWxQnQ==;EndpointSuffix=core.windows.net";
                //string connContainer = "dlc";
                //string connString = "DefaultEndpointsProtocol=https;AccountName=chessstarsblobstorage;AccountKey=9UE+ONgiQuIt9/vrUQtN2VUuDGSkMiLEi6qLlpNL8koLMHo4d68xmTMx4T/CMiiuNj7143VCawev+AStDkzjTw==;EndpointSuffix=core.windows.net";
                //string connContainer = "profilepics";
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