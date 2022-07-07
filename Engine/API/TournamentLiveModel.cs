/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Models
{
    public class TournamentReward
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int minRank;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int maxRank;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int trophies;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string chestType;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int gems;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int hints;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int ratingBoosters;
    }

    public class TournamentSlot
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int min;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]   public int max;
    }

    public class TournamentRewards
    {
        [BsonElement("0")]  public List<TournamentReward> r0;
        [BsonElement("1")]  public List<TournamentReward> r1;
        [BsonElement("2")]  public List<TournamentReward> r2;
        [BsonElement("3")]  public List<TournamentReward> r3;
        [BsonElement("4")]  public List<TournamentReward> r4;
        [BsonElement("5")]  public List<TournamentReward> r5;
        [BsonElement("6")]  public List<TournamentReward> r6;
        [BsonElement("7")]  public List<TournamentReward> r7;
    }

    public class TournamentLiveData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string shortCode;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)] public bool active;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string name;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]  public string type;
                                                            public TournamentReward grandPrize;
                                                            public TournamentRewards rewards;
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
    }

    public class TournamentLiveDocument
    {
        [BsonElement("TournamentLive")] public TournamentLiveData tournament;
    }

    public class TournamentLiveModel
    {
        [BsonIgnore] private string LIVE_TOURNAMENTS_COLLECTION = "tournamentsLive";
        [BsonIgnore] private bool _isCached;
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
            return taskT.Result != null ? taskT.Result.tournament : null;
        }

        public string GetActiveShortCode (int timeZone)
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
            ProjectionDefinition<TournamentLiveDocument> projection = Builders<TournamentLiveDocument>.Projection.Include("tournament.shortCode").Exclude<TournamentLiveDocument>("_id");
            
            var taskT = collection.FindOne<TournamentLiveDocument>(filter, projection);
            taskT.Wait();
            if (taskT.Result != null) _cache.Add(taskT.Result.tournament.shortCode.ToString(), taskT.Result.tournament);
            return taskT.Result != null ? taskT.Result.tournament.shortCode.ToString() : null;
        }
    }
}
