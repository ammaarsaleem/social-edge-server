/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
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

        public static Dictionary<string, int> GetDailyRewardDictionary(string leagueId, dynamic progression, int index)
        {
            Dictionary<string, int>  outDict = new Dictionary<string, int>();
            var dict = SocialEdge.TitleContext.GetTitleDataProperty("Leagues")[leagueId]["dailyReward"];

            foreach(var item in dict)
                outDict.Add(item.Name, (int)((int)item.Value + Math.Ceiling((double)progression[index % progression.Count] * (int)item.Value)));

            return outDict;
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