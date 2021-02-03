using System;
using SocialEdge.Server.Common.Utils;
using MongoDB.Driver;
using MongoDB.Bson;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using SocialEdge.Server.Cache;
namespace SocialEdge.Server.Db
{
    public class DbHelper: IDbHelper
    {
        private readonly MongoClient _dbClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _playerCollection;
        // private readonly ILogger _logger;

        public DbHelper(MongoClient dbclient,ICache cache)
        {
            SocialEdgeEnvironment.Init();
            _dbClient = dbclient;
            string dbName = ConfigConstants.DATABASE;
            _database = _dbClient.GetDatabase(dbName);
            _playerCollection = _database.GetCollection<BsonDocument>("Player");                     
        }

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

        public async Task<BsonDocument> SearchPlayer(string name)
        {
            var nameFilter = Builders<BsonDocument>.Filter.Eq("displayName", name);
            var findTask = await _playerCollection.FindAsync(nameFilter);
            return findTask.FirstOrDefault();
        }


        [FunctionName("TestDb")]
        public async Task<string> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {   
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var args = JsonConvert.DeserializeObject<Dictionary<string,string>>(requestBody);
                string playFabId = args["playfabid"];
                string name = args["name"];
                var filter = Builders<BsonDocument>.Filter.Eq("playerId",playFabId);
                var update = Builders<BsonDocument>.Update.Set("playerId",playFabId)
                                                            .Set("displayName", name);
            
                var options = new UpdateOptions{IsUpsert = true};
                var doc = new BsonDocument
                {
                    { "name", name}, { "playerId", playFabId } 
                };

                return "OK";
            }        
            catch(Exception e)
            {
                return e.InnerException.Message;
            }
        }
    }
}