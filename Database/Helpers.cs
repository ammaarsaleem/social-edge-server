using System;
using PlayFab;
using SocialEdge.Server.Common.Constants;
using System.Net.Http;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using SocialEdge.Server.Common.Utils;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
namespace SocialEdge.Server.Common.Db
{
    public class DbHelper
    {
        private readonly MongoClient _dbClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _playerCollection;
        // private readonly ILogger _logger;

        public DbHelper(MongoClient dbclient)
        {
            SocialEdgeEnvironment.Init();
            _dbClient = dbclient;
            string dbName = Constant.DATABASE;
            _database = _dbClient.GetDatabase(dbName);
            _playerCollection = _database.GetCollection<BsonDocument>("Player");
        }

        // private readonly IMongoCollection<Album> _albums;


        public async Task<UpdateResult> RegisterPlayer(string playFabId, string name, DateTime loginTime)
        {   

            var filter = Builders<BsonDocument>.Filter.Eq("playerId",playFabId);
            var update = Builders<BsonDocument>.Update.Set("playerId",playFabId)
                                                        .Set("displayName", name)
                                                        .Set("lastLogin", loginTime);
            
            var options = new UpdateOptions{IsUpsert = true};
            var upsertTask = await _playerCollection.UpdateOneAsync(filter,update,options);
            return upsertTask;
        }

        public async Task<IAsyncCursor<BsonDocument>> GetPlayer(string playerId, string name)
        {
            var idFilter = Builders<BsonDocument>.Filter.Eq("playerId", playerId);
            var nameFilter = Builders<BsonDocument>.Filter.Eq("displayName", name);
            var combinedFilter = Builders<BsonDocument>.Filter.Or(idFilter,nameFilter);

            var findTask = await _playerCollection.FindAsync(combinedFilter);
            return findTask;
        }


          [FunctionName("TestDb")]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        
        {   
            // var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var args = JsonConvert.DeserializeObject<Dictionary<string,string>>(requestBody);
            string playFabId = args["playfabid"];
            string name = args["name"];
            var filter = Builders<BsonDocument>.Filter.Eq("playerId",playFabId);
            var update = Builders<BsonDocument>.Update.Set("playerId",playFabId)
                                                        .Set("displayName", name);
                                                        // .Set("lastLogin", loginTime);
            
            var options = new UpdateOptions{IsUpsert = true};
            // var upsertTask = await _playerCollection.UpdateOneAsync(filter,update,options);
            var doc = new BsonDocument{
                { "name", name}, { "playerId", playFabId } 
            };

            await _playerCollection.FindAsync(filter);
        //    await _playerCollection.InsertOneAsync(doc);
            
        }
    }
}