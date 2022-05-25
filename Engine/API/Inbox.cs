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
        public static BsonDocument Create(BsonDocument messageInfo)
        {
            BsonDocument message = _getDefaultMessage();

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

        private static BsonDocument _getDefaultMessage()
        {
            return new BsonDocument() {

                ["id"] = 0,
                ["type"] = "",
                ["isDaily"] = false,
                ["heading"] = "",
                ["body"] = "",
                ["time"] = Utils.UTCNow(),
                ["reward"] = new BsonDocument(),
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
                socialEdgePlayer.SetDirtyBit(CacheSegment.INBOX);
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

                var message = Create(msgInfo);
                InboxModel.Add(message, socialEdgePlayer);
            }

            // Update league rewards
            var leagueDailyRewardMsgId = InboxModel.FindOne("RewardDailyLeague", socialEdgePlayer);
            var inboxT = InboxModel.Get(socialEdgePlayer.InboxId);
            inboxT.Wait();
            var inbox = inboxT.Result;
            var msg = inbox["messages"][leagueDailyRewardMsgId];
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

            var leagueDailyRewardMessage = Inbox.Create(leageuDailyRewardMsgInfo);
            InboxModel.Update(leagueDailyRewardMsgId, leageuDailyRewardMsgInfo, socialEdgePlayer);
        }
    }
}

/*
var Inbox = (function () {
    
    var _getMessageByType = function(sparkPlayer, msgType) {
        var inboxData = InboxModel.get(sparkPlayer);
        var keys = Object.keys(inboxData.messages);
        var found = false;
        var i = 0;
        while (!found && i < keys.length) {
            found = inboxData.messages[keys[i]].type == msgType;
            if (!found) i++;
        }
        
        return found ? inboxData.messages[keys[i]] : null;
    };

    var validate = function(sparkPlayer) {
        var playerData = PlayerModel.get(sparkPlayer);
        
        // Daily league reward
        var msgInfo = _getMessageByType(sparkPlayer, 'RewardDailyLeague');
        if (msgInfo == null) {
            var reward = Leagues.getDailyReward(playerData.pub.league);
            
            var newMsgInfo = {
                type: "RewardDailyLeague",
                league: Leagues.getLeague(playerData.pub.league).name,
                isDaily: true,
                reward : {
                    coins: reward.coins,
                    gems: reward.gems
                }
            };

            var message = Inbox.create(sparkPlayer, newMsgInfo);
            InboxModel.add(sparkPlayer, message);
        }
        else // if(msgInfo.reward.coins == undefined) 
         {
            msgInfo.reward = Leagues.getDailyReward(playerData.pub.league);
            Inbox.update(sparkPlayer, msgInfo.id, msgInfo);
        }
        
        // Daily subscription reward
        var msgId = Inbox.find(sparkPlayer, 'RewardDailySubscription');
        // var isSubscriber = PlayerModel.isSubscriber(playerData);
        // if (isSubscriber == true && msgId == null) {
        //     var subscriptionRewardsSet = Spark.getProperties().getPropertySet("SubscriptionRewardsSet");
        //     var subscriptionRewardsProperty = subscriptionRewardsSet["subscriptionRewardsProperty"];
        //     var reward = Leagues.getDailyReward(playerData.pub.league);
        //     var msgInfo = {
        //         type: "RewardDailySubscription",
        //         isDaily: true,
        //         reward : {
        //             SpecialItemTicket: subscriptionRewardsProperty.dailyReward.tickets 
        //         }
        //     };
        //     var message = Inbox.create(sparkPlayer, msgInfo);
        //     InboxModel.add(sparkPlayer, message);            
        // }
        // else 
        if (msgId != null) {
            InboxModel.del(sparkPlayer, msgId);
        }

        validateChampionshipRewardMessages(sparkPlayer);
    };
    
    
    var validateChampionshipRewardMessages = function (sparkPlayer)
    {
        var tournamentRewardMessages = findAll(sparkPlayer, 'RewardTournamentEnd');
        for (var i = 0; i < tournamentRewardMessages.length; i++) {
            // Removing tournament reward messages other than weekly championship
            var removeMsg = false;
            if (tournamentRewardMessages[i].tournamentType !== 'Weekly' && tournamentRewardMessages[i].tournamentType !== 'Daily') {
                removeMsg = true;
            }

            // Removing expired championship reward messages
            if (!removeMsg) {
                if (tournamentRewardMessages[i].hasOwnProperty('expireAt')) {
                    var currentTime = moment.utc().valueOf();
                    var timeLeft = tournamentRewardMessages[i].expireAt - currentTime;
                    if (timeLeft <= 0) {
                        removeMsg = true;
                    }
                }
            }

            if (removeMsg) {
                InboxModel.del(sparkPlayer, tournamentRewardMessages[i].id);
            }
        }
    }

    };

*/