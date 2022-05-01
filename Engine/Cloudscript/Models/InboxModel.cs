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

namespace SocialEdge.Server.Models
{
    public static class InboxModel
    {
        public static async Task<JObject> Get(string inboxId)
        {
            BsonDocument inboxT = await SocialEdgeEnvironment.DataService.GetCollection("inbox").FindOneById(inboxId);
            return new JObject(inboxT.ToJson());
        }

        public static async Task<SocialEdge.Server.DataService.UpdateResult> Set(string inboxId, JObject inbox)
        {
           var T = await SocialEdgeEnvironment.DataService.GetCollection("inbox").UpdateOneById(inboxId,  "inboxData", inbox.ToJson(), true);
           return T;
        }
    }
}