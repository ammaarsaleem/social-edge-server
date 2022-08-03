/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using PlayFab.ServerModels;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialEdgeSDK.Server.Context
{
    public class LeagueQualificationRewardData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int gems;
    }

    public class LeagueQualificationData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int trophies;
                                                                public LeagueQualificationRewardData reward;
    }

    public class LeagueTrophiesData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int win;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int loss;
    }

    public class LeagueDailyRewardData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int coins;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int gems;
    }

    public class LeagueSettingsData
    {
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string name;
                                                                public LeagueQualificationData qualification;
                                                                public LeagueTrophiesData trophies;
                                                                public LeagueDailyRewardData dailyReward;
    }

    public class LeagueSettingModel
    {
        public Dictionary<string, LeagueSettingsData> leagues;
    }
}