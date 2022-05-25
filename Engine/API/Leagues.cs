/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using SocialEdgeSDK.Server.Context;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;

namespace SocialEdgeSDK.Server.Api
{
    public static class Leagues
    {
        public static BsonDocument GetDailyReward(string leagueId)
        {
            return SocialEdge.TitleContext.GetTitleDataProperty("Leagues")[leagueId]["dailyReward"];
        }

        public static int GetQualifiedLeageId(int trophies)
        {
            BsonDocument leagueSettings = SocialEdge.TitleContext.GetTitleDataProperty("Leagues");
            int i = leagueSettings.ElementCount - 1;
            while (i >= 0 && trophies <= (int)leagueSettings[i.ToString()]["qualification"]["trophies"]) i--;
            return Math.Clamp(i, 0, leagueSettings.ElementCount - 1);
        }
        
        public static BsonDocument GetLeague(string leagueId)
        {
            return SocialEdge.TitleContext.GetTitleDataProperty("Leagues")[leagueId];
        }
    }
}