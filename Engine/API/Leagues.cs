using System;
using SocialEdge.Server.Common.Utils;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace SocialEdge.Server.Api
{
    public static class Leagues
    {
        public static BsonDocument GetDailyReward(string leagueId)
        {
            return SocialEdgeEnvironment.TitleContext.GetTitleDataProperty("Leagues")[leagueId]["dailyReward"];
        }

        public static int GetQualifiedLeageId(int trophies)
        {
            BsonDocument leagueSettings = SocialEdgeEnvironment.TitleContext.GetTitleDataProperty("Leagues");
            int i = leagueSettings.ElementCount - 1;
            while (i >= 0 && trophies <= (int)leagueSettings[i.ToString()]["qualification"]["trophies"]) i--;
            return Math.Clamp(i, 0, leagueSettings.ElementCount - 1);
        }
        
        public static JToken GetLeague(string leagueId)
        {
            return SocialEdgeEnvironment.TitleContext.GetTitleDataProperty("Leagues")[leagueId];
        }
    }
}