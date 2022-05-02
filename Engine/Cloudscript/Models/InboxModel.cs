using PlayFab.ServerModels;
using System.Collections.Generic;
using SocialEdge.Server.Common.Utils;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Models;
using SocialEdge.Server.Constants;
using SocialEdge.Server.Db;
using SocialEdge.Server.Api;
using SocialEdge.Server.DataService;
using PlayFab.ProfilesModels;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;
using MongoDB.Bson.IO;

namespace SocialEdge.Server.Models
{
    public static class InboxModel
    {
        public static async Task<BsonDocument> Get(string inboxId)
        {
            return await SocialEdgeEnvironment.DataService.GetCollection("inbox").FindOneById(inboxId);
        }

        public static async Task<SocialEdge.Server.DataService.UpdateResult> Set(string inboxId, BsonDocument inbox)
        {  
            return await SocialEdgeEnvironment.DataService.GetCollection("inbox").ReplaceOneById(inboxId, inbox, true);
        }

        public static int Count(JObject inbox)
        {
            long now = UtilFunc.UTCNow();
            int count = 0;
            var messages = inbox["messages"];
            foreach(var msg in messages)
            {
               // msg.
               count++;
            }

            return 0;
        }

        /*
            var count = function (sparkPlayer) {
        var inbox = get(sparkPlayer);
        
        var count = 0;
        var now = moment.utc().valueOf();
        for (var i in inbox.messages) {
            var msg = inbox.messages[i];
            count += msg.startTime == undefined || now >= msg.startTime ? 1 : 0;
        }
        
        return count;
    };
        */
    }
    
}