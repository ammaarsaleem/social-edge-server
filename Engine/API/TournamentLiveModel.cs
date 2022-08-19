/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Models
{
    public class TournamentReward
    {
        #pragma warning disable format
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int minRank;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int maxRank;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int trophies;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string chestType;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int gems;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int hints;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int ratingBoosters;
        #pragma warning restore format
    }

    public class TournamentSlot
    {
        #pragma warning disable format
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int min;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int max;
        #pragma warning restore format
    }

    public class TournamentLiveData
    {
        #pragma warning disable format
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string shortCode;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool active;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string name;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string type;
                                                            public TournamentReward grandPrize;
                                                            public Dictionary<string, List<TournamentReward>> rewards;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long startTime;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long duration;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long waitTime;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int league;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int maxElo;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int minElo;
                                                            public List<TournamentSlot> slotsData;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int maxPlayers;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string collectionPrefix;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int noOfCollections;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int timeZoneMin;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int timeZoneMax;

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string rules;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long durationMinutes;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]   public long waitTimeMinutes;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool repeat;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int matchWonTrophies;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int matchLostTrophies;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int entryCurrency;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int entryCost;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int matchCurrency;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int matchCost;              
        #pragma warning restore format
    }

    public class TournamentLiveDocument
    {
        [BsonElement("TournamentLive")] public TournamentLiveData tournament;
    }

    public class TournamentLiveModel
    {
        [BsonIgnore] private string LIVE_TOURNAMENTS_COLLECTION = "tournamentsLive";
        [BsonIgnore] Dictionary<string, TournamentLiveData> _cache;

        public TournamentLiveModel()
        {
            _cache = new Dictionary<string, TournamentLiveData>();
        }

        public TournamentLiveData Get(string tournamentShortCode)
        {
            if (_cache.ContainsKey(tournamentShortCode))
                return _cache[tournamentShortCode];

            var collection = SocialEdge.DataService.GetCollection<TournamentLiveDocument>(LIVE_TOURNAMENTS_COLLECTION);
            var projection = Builders<TournamentLiveDocument>.Projection.Exclude("_id").Include("TournamentLive");
            var filter = Builders<TournamentLiveDocument>.Filter.Eq("TournamentLive.shortCode", tournamentShortCode);
            var taskT = collection.FindOne<TournamentLiveDocument>(filter, projection);
            taskT.Wait();
            if (taskT.Result != null) _cache.Add(tournamentShortCode, taskT.Result.tournament);
            SocialEdge.Log.LogInformation("Task fetch TOURNAMENT_LIVE:" + (taskT.Result != null ? "(success)" : "(null)"));
            return taskT.Result != null ? taskT.Result.tournament : null;
        }

        public string GetActiveShortCode(int timeZone)
        {
            // Check cache first
            var val = _cache.Where(t => timeZone >= t.Value.timeZoneMin && timeZone < t.Value.timeZoneMax).Select(t => (KeyValuePair<string, TournamentLiveData>?)t).FirstOrDefault();
            if (val != null)
                return val.Value.Value.shortCode;

            // Fetch from collection
            var collection = SocialEdge.DataService.GetCollection<TournamentLiveDocument>(LIVE_TOURNAMENTS_COLLECTION);
            FilterDefinition<TournamentLiveDocument> filter = Builders<TournamentLiveDocument>.Filter.Eq("tournament.active", true);
            filter = filter & Builders<TournamentLiveDocument>.Filter.Lte("tournament.timeZoneMin", timeZone);
            filter = filter & Builders<TournamentLiveDocument>.Filter.Gt("tournament.timeZoneMax", timeZone);
            ProjectionDefinition<TournamentLiveDocument> projection = Builders<TournamentLiveDocument>.Projection.Include("TournamentLive").Exclude<TournamentLiveDocument>("_id");

            var taskT = collection.FindOne<TournamentLiveDocument>(filter, projection);
            taskT.Wait();
            if (taskT.Result != null) _cache.Add(taskT.Result.tournament.shortCode.ToString(), taskT.Result.tournament);
            SocialEdge.Log.LogInformation("Task fetch TOURNAMENT_LIVE:" + (taskT.Result != null ? "(success)" : "(null)"));
            return taskT.Result != null ? taskT.Result.tournament.shortCode.ToString() : null;
        }

        public Dictionary<string, TournamentLiveData> Fetch()
        {
            var collection = SocialEdge.DataService.GetCollection<TournamentLiveDocument>(LIVE_TOURNAMENTS_COLLECTION);
            var projection = Builders<TournamentLiveDocument>.Projection.Exclude("_id").Include("TournamentLive");
            var filter = Builders<TournamentLiveDocument>.Filter.Empty;
            var taskT = collection.Find(filter, projection);
            taskT.Wait();
            if (taskT.Result != null && taskT.Result.Count > 0)
            {
                foreach (var doc in taskT.Result)
                {
                    if (!_cache.ContainsKey(doc.tournament.shortCode))
                        _cache.Add(doc.tournament.shortCode, doc.tournament);
                }

            }
            return _cache;
        }
    }
}
