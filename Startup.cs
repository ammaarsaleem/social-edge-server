
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.DependencyInjection
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Linq;
[assembly: FunctionsStartup(typeof(SocialEdge.Playfab.Startup))]
namespace SocialEdge.Playfab
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
              builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddFilter(level => true);
            });

            var config = (IConfiguration)builder.Services.First(d => d.ServiceType == typeof(IConfiguration)).ImplementationInstance;

            builder.Services.AddSingleton((s) =>
            {
                // MongoClient client = new MongoClient(config[Settings.MONGO_CONNECTION_STRING]);
                MongoClient client = new MongoClient("mongodb+srv://MyMongoDBUser:MyMongoDBUserPassword@socialedgecluster.hsxfp.mongodb.net/Development?retryWrites=true&w=majority");
                return client;
            });
        }
    }
}