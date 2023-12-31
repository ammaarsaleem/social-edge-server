/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using SocialEdgeSDK.Server.Context;
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

namespace SocialEdgeSDK.Server.Db
{
    public class DbHelper: IDbHelper
    {
        private readonly MongoClient _dbClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<BsonDocument> _playerCollection;
        // private readonly ILogger _logger;

        public DbHelper(MongoClient dbclient)
        {
            SocialEdge.Init();
            _dbClient = dbclient;
            string dbName = ConfigConstants.MONGO_DATABASE_NAME;
            _database = _dbClient.GetDatabase(dbName);
            _playerCollection = _database.GetCollection<BsonDocument>(Constants.PLAYER_COLLECTION);                     
        }

        public async Task<bool> RegisterPlayer(string playFabId, string name, DateTime loginTime)
        {   
            
            
            var filter = Builders<BsonDocument>.Filter.Eq("playerId",playFabId);
            var update = Builders<BsonDocument>.Update.Set("playerId",playFabId)
                                                        .Set("displayName", name)
                                                        .Set("lastLogin", loginTime);
            
            var options = new UpdateOptions{IsUpsert = true};
            var upsertTask = await _playerCollection.UpdateOneAsync(filter,update,options);
            return true;
        }

        public async Task<Dictionary<string,object>> SearchPlayerByName(string name)
        {
            var nameFilter = Builders<BsonDocument>.Filter.Eq("displayName", name);
            var findTask = await _playerCollection.FindAsync(nameFilter);
            return findTask.FirstOrDefault().ToDictionary();
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