/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using SocialEdgeSDK.Server.Context;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using SocialEdgeSDK.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.DataService;

namespace SocialEdgeSDK.Server.Models
{
    public class InboxDataMessage
    {
        #pragma warning disable format   
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string msgId;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string type;
        [BsonRepresentation(MongoDB.Bson.BsonType.Boolean)]     public bool isDaily;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string heading;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string body;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long time;
                                                                public Dictionary<string, int> reward;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int trophies;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]       public int rank;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string chestType;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string tournamentType;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string tournamentId;
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]      public string league;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long startTime;
        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]       public long expireAt;
        #pragma warning restore format
    }

    public class InboxDataDocument
    {
        #pragma warning disable format  
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)][BsonIgnoreIfNull]    public ObjectId _id;
        [BsonElement("InboxData")]                                                public Dictionary<string, InboxDataMessage> _messages;
        #pragma warning restore format
    }

    public static class Inbox
    {
        public static InboxDataMessage CreateMessage()
        {
            return new InboxDataMessage()
            {
                msgId = Guid.NewGuid().ToString(),
                time = Utils.UTCNow(),
                reward = new Dictionary<string, int>() {},
                startTime = Utils.UTCNow()
            };
        }

        public static Dictionary<string, int> Collect(string messageId, SocialEdgePlayerContext socialEdgePlayer)
        {
            Dictionary<string, InboxDataMessage> inbox = socialEdgePlayer.Inbox;
            var msg = inbox.ContainsKey(messageId) ? inbox[messageId] : null;
            if (msg == null)
                return null;

            Dictionary<string, int> granted = msg.reward != null ? socialEdgePlayer.PlayerEconomy.Grant(msg.reward) : null;

            if (msg.isDaily == true)
            {
                var league = socialEdgePlayer.MiniProfile.League;
                msg.reward = Leagues.GetDailyRewardDictionary(league.ToString());
                msg.startTime = Utils.ToUTC(Utils.EndOfDay(DateTime.Now));
                msg.time = msg.startTime;
                socialEdgePlayer.SetDirtyBit(CachePlayerDataSegments.INBOX);
            }
            else // Remove 
            {
                InboxModel.Del(messageId, socialEdgePlayer);
            }

            if(msg.trophies > 0)
            {
                socialEdgePlayer.PlayerModel.Info.trophies2 = socialEdgePlayer.PlayerModel.Info.trophies2 + msg.trophies;
                granted.Add("trophies2", msg.trophies);
            }

            return granted;
        }

        public static void SetupLeaguePromotion(string qualifiedLeagueId, bool promoted, SocialEdgePlayerContext socialEdgePlayer)
        {
            LeagueSettingsData league = SocialEdge.TitleContext.LeagueSettings.leagues[qualifiedLeagueId];
            
            // Add promotion reward
            if (promoted)
            {
                var message = CreateMessage();
                message.type = "RewardLeaguePromotion";
                message.league =  league.name;
                message.reward = new Dictionary<string, int>() { ["gems"] = (int)league.qualification.reward.gems};
                InboxModel.Add(message, socialEdgePlayer);
            }

            // Update league rewards
            string leagueDailyRewardMsgId = InboxModel.FindOne("RewardDailyLeague", socialEdgePlayer);
            var msg = socialEdgePlayer.Inbox[leagueDailyRewardMsgId];
            var leageuDailyRewardMsgInfo =Inbox.CreateMessage();
            leageuDailyRewardMsgInfo.type = "RewardDailyLeague";
            leageuDailyRewardMsgInfo.isDaily = true;
            leageuDailyRewardMsgInfo.league = league.name;
            leageuDailyRewardMsgInfo.reward = new Dictionary<string, int>()
                {
                    ["gems"] = (int)league.dailyReward.gems,
                    ["coins"] = (int)league.dailyReward.coins
                };
            leageuDailyRewardMsgInfo.startTime = msg.startTime;
            leageuDailyRewardMsgInfo.time = msg.startTime;


            InboxModel.Update(leagueDailyRewardMsgId, leageuDailyRewardMsgInfo, socialEdgePlayer);
        }

        private static void ValidateDailyReward(SocialEdgePlayerContext socialEdgePlayer)
        {
            string msgInfo = InboxModel.FindOne("RewardDailyLeague", socialEdgePlayer);
            if (msgInfo == null)
            {
                var leagueId = socialEdgePlayer.MiniProfile.League.ToString();
                var reward = Leagues.GetDailyReward(leagueId);
                InboxDataMessage newMsgInfo = Inbox.CreateMessage();
                newMsgInfo.type = "RewardDailyLeague";
                newMsgInfo.league = Leagues.GetLeague(leagueId)["name"].ToString();
                newMsgInfo.isDaily = true;
                newMsgInfo.reward = new Dictionary<string, int>()
                    {
                        ["coins"] = (int)reward["coins"],
                        ["gems"] = (int)reward["gems"]
                    };

                InboxModel.Add(newMsgInfo, socialEdgePlayer);
            }
        }

        public static void CreateAnnouncementMessage(SocialEdgePlayerContext socialEdgePlayer, string title, string body)
        {
            var message = CreateMessage();
            message.type = "Announcement";
            message.heading = title;
            message.body = body;
            message.expireAt = Utils.UTCNow() + (5 * 60 * 1000);
            message.startTime = Utils.ToUTC(DateTime.UtcNow.AddDays(2));
            message.time = message.startTime;
            InboxModel.Add(message, socialEdgePlayer);
        }

        private static void RemoveExpired(SocialEdgePlayerContext socialEdgePlayer)
        {
            Dictionary<string, InboxDataMessage> inbox = socialEdgePlayer.Inbox;  
            if (inbox == null)
                return;

            long now = Utils.UTCNow();
            List<string> delIds = new List<string>();
            foreach (var item in inbox)
            {
                var msg = item.Value;
                if (msg.expireAt != 0 && Convert.ToInt64(msg.expireAt.ToString()) <= now)
                    delIds.Add(msg.msgId.ToString());
            }

            foreach(string id in delIds)
            {
                InboxModel.Del(id, socialEdgePlayer);
            }
        }

        public static void Validate(SocialEdgePlayerContext socialEdgePlayer)
        {
            RemoveExpired(socialEdgePlayer);
            ValidateDailyReward(socialEdgePlayer);
        }
    }
}
