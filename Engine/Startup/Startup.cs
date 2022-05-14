
/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Linq;
using SocialEdgeSDK.Server.Db;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;
using System;
using StackExchange.Redis;

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
                MongoClient client = new MongoClient("mongodb+srv://MyMongoDBUser:MyMongoDBUserPassword@socialedgecluster.hsxfp.mongodb.net/Development?retryWrites=true&w=majority");
                // MongoClient client = new MongoClient("mongodb+srv://MyMongoDBUser:MyMongoDBUserPassword@socialedgecluster.hsxfp.mongodb.net/myFirstDatabase?retryWrites=true&w=majority");
                // string connString = Environment.GetEnvironmentVariable(ConfigConstants.MONGO_CONNECTION_STRING, EnvironmentVariableTarget.Process);
                // MongoClient client = new MongoClient(connString);
                return client;
            });

            builder.Services.AddSingleton((r) =>
            {
                // string connString = "socialedgeserver.redis.cache.windows.net:6380,password=Gf7aTTxpvRfk+IKGqCbQwW7j+bWIKcr5B6bXAqj+ZSQ=,ssl=True,abortConnect=False";
                string connString = Environment.GetEnvironmentVariable(ConfigConstants.REDIS_CONNECTION_STRING, EnvironmentVariableTarget.Process);
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connString);
                return redis;
            });

            // builder.Services.AddSingleton((s) =>
            // {
            //     MongoClient client = new MongoClient("mongodb+srv://MyMongoDBUser:MyMongoDBUserPassword@socialedgecluster.hsxfp.mongodb.net/Development?retryWrites=true&w=majority");
            //     string connString = Environment.GetEnvironmentVariable(ConfigConstants.REDIS_CONNECTION_STRING, EnvironmentVariableTarget.Process);
            //     ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(connString);
            //     IDataService dataService = new DataService(client, redis);
            //     return dataService;
            // });

            builder.Services.AddSingleton<ICache,Cache>();
            builder.Services.AddSingleton<IDbHelper,DbHelper>();
            builder.Services.AddSingleton<IDataService,DataService>();
            builder.Services.AddSingleton<ITitleContext, TitleContext>();
        }
    }
}