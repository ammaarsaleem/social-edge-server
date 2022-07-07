/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using SocialEdgeSDK.Server.Context;
using MongoDB.Bson;
using SocialEdgeSDK.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Api
{
    public static class Inbox
    {
        public static BsonDocument CreateMessage(BsonDocument messageInfo)
        {
            BsonDocument message = GetDefaultMessage();

            message["id"] = Guid.NewGuid().ToString();
            message["type"] = messageInfo["type"];

            if (messageInfo.GetValue("isDaily", null) != null) message["isDaily"] = messageInfo["isDaily"];
            if (messageInfo.GetValue("heading", null) != null) message["heading"] = messageInfo["heading"];
            if (messageInfo.GetValue("body", null) != null) message["body"] = messageInfo["body"];
            if (messageInfo.GetValue("time", null) != null) message["time"] = messageInfo["time"];
            if (messageInfo.GetValue("reward", null) != null) message["reward"] = messageInfo["reward"];
            if (messageInfo.GetValue("trophies", null) != null) message["trophies"] = messageInfo["trophies"];
            if (messageInfo.GetValue("rank", null) != null) message["rank"] = messageInfo["rank"];
            if (messageInfo.GetValue("chest", null) != null) message["chestType"] = messageInfo["chest"];
            if (messageInfo.GetValue("tournamentType", null) != null) message["tournamentType"] = messageInfo["tournamentType"];
            if (messageInfo.GetValue("tournamentId", null) != null) message["tournamentId"] = messageInfo["tournamentId"];
            if (messageInfo.GetValue("league", null) != null) message["league"] = messageInfo["league"];
            if (messageInfo.GetValue("startTime", null) != null) message["startTime"] = messageInfo["startTime"];
            if (messageInfo.GetValue("expireAt", null) != null) message["expireAt"] = messageInfo["expireAt"];

            return message;
        }

        private static BsonDocument GetDefaultMessage()
        {
            return new BsonDocument() {

                ["id"] = 0,
                ["type"] = "",
                ["isDaily"] = false,
                ["heading"] = "",
                ["body"] = "",
                ["time"] = Utils.UTCNow(),
                ["reward"] = new BsonDocument() {},
                ["trophies"] = 0,
                ["rank"] = 0,
                ["chestType"] = "",
                ["tournamentType"] = "",
                ["league"] = "",
                ["startTime"] = Utils.UTCNow()
            };
        }

        public static async Task<Dictionary<string, int>> Collect(string messageId, SocialEdgePlayerContext socialEdgePlayer)
        {
            var inbox = socialEdgePlayer.Inbox;
            var msg = inbox["messages"].AsBsonDocument.Contains(messageId) ? inbox["messages"][messageId] : null;
            if (msg == null)
            {
                return null;
            }

            var granted = await Transactions.Grant(msg["reward"].AsBsonDocument, socialEdgePlayer);

            if (msg["isDaily"] == true)
            {
                var league = socialEdgePlayer.PublicData["leag"];
                msg["reward"] = Leagues.GetDailyReward(league.ToString());
                msg["startTime"] = Utils.ToUTC(Utils.EndOfDay(DateTime.Now));
                msg["time"] = msg["startTime"];
                socialEdgePlayer.SetDirtyBit(CachePlayerDataSegments.INBOX);
            }
            else if (msg.AsBsonDocument.Contains("tournamentId") == true)
            {
                // TODO Tournaments.deleteTournament(sparkPlayer, msg.tournamentId);
                InboxModel.Del(messageId, socialEdgePlayer);
            }

            return granted;
        }

        public static void SetupLeaguePromotion(string qualifiedLeagueId, bool promoted, SocialEdgePlayerContext socialEdgePlayer)
        {
            var league = Leagues.GetLeague(qualifiedLeagueId);
            
            // Add promotion reward
            if (promoted)
            {
                BsonDocument msgInfo = new BsonDocument()
                {
                    ["type"] = "RewardLeaguePromotion",
                    ["league"] = league["name"].ToString(),
                    ["reward"] = new BsonDocument() { ["gems"] = (int)league["qualification"]["reward"]["gems"]}
                };

                var message = CreateMessage(msgInfo);
                InboxModel.Add(message, socialEdgePlayer);
            }

            // Update league rewards
            var leagueDailyRewardMsgId = InboxModel.FindOne("RewardDailyLeague", socialEdgePlayer);
            var msg = socialEdgePlayer.Inbox["messages"][leagueDailyRewardMsgId];
            var leageuDailyRewardMsgInfo = new BsonDocument()
            {
                ["type"] = "RewardDailyLeague",
                ["isDaily"] = true,
                ["league"] = league["name"].ToString(),
                ["reward"] = new BsonDocument()
                    {
                        ["gems"] = (int)league["dailyReward"]["gems"],
                        ["coins"] = (int)league["dailyReward"]["coins"]
                    }
                ["startTime"] = msg["startTime"],
                ["time"] = msg["startTime"]
            };

            var leagueDailyRewardMessage = Inbox.CreateMessage(leageuDailyRewardMsgInfo);
            InboxModel.Update(leagueDailyRewardMsgId, leageuDailyRewardMsgInfo, socialEdgePlayer);
        }

        private static void ValidateDailyReward(SocialEdgePlayerContext socialEdgePlayer)
        {
            var msgInfo = InboxModel.FindOne("RewardDailyLeague", socialEdgePlayer);
            if (msgInfo == null)
            {
                var leagueId = socialEdgePlayer.PublicData["leag"].ToString();
                var reward = Leagues.GetDailyReward(leagueId);
                BsonDocument newMsgInfo = new BsonDocument()
                {
                    ["type"] = "RewardDailyLeague",
                    ["league"] = Leagues.GetLeague(leagueId)["name"].ToString(),
                    ["isDaily"] = true,
                    ["reward"] = new BsonDocument()
                    {
                        ["coins"] = reward["coins"],
                        ["gems"] = reward["gems"]
                    }
                };

                var message = Inbox.CreateMessage(newMsgInfo);
                InboxModel.Add(message, socialEdgePlayer);
            }
        }

        private static void RemoveExpired(SocialEdgePlayerContext socialEdgePlayer)
        {
            var inbox = socialEdgePlayer.Inbox;            
            long now = Utils.UTCNow();
            var messages = inbox["messages"].AsBsonDocument;
            List<string> delIds = new List<string>();
            foreach (string key in messages.Names)
            {
                var msg = messages[key].AsBsonDocument;
                if (msg.Contains("expireAt") && Convert.ToInt64(msg["expireAt"].ToString()) <= now)
                {
                    delIds.Add(msg["id"].ToString());
                }
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
