using System;
using SocialEdge.Server.Common.Utils;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;

namespace SocialEdge.Server.Api
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
                ["time"] = UtilFunc.UTCNow(),
                ["reward"] = new BsonDocument(),
                ["trophies"] = 0,
                ["rank"] = 0,
                ["chestType"] = "",
                ["tournamentType"] = "",
                ["league"] = "",
                ["startTime"] = UtilFunc.UTCNow()
            };
        }
    }
}

/*
var Inbox = (function () {


    var collect = function (sparkPlayer, messageId) {
        var inbox = InboxModel.get(sparkPlayer);
        var msg = inbox.messages[messageId];
        if (msg == undefined) {
            return null;
        }
        
        var granted = Transactions.grant(sparkPlayer, msg.reward, msg.trophies);
        
        if (msg.isDaily == true) {
            var playerData = PlayerModel.get(sparkPlayer);
            msg.reward = Leagues.getDailyReward(playerData.pub.league);
            msg.startTime = moment(moment().endOf('day').toString()).valueOf();
            msg.time = msg.startTime;
            InboxModel.set(sparkPlayer);
        }
        else {
            if (msg.tournamentId) {
                Tournaments.deleteTournament(sparkPlayer, msg.tournamentId);
            }
            
            InboxModel.del(sparkPlayer, messageId);
        }

        return granted;
    };
    
    var setupLeaguePromotion = function(sparkPlayer, qualifiedLeagueId, promoted) {
        var league = Leagues.getLeague(qualifiedLeagueId);
        
        //Add promotion reward
        if(promoted) {
            var msgInfo = {
                type: "RewardLeaguePromotion",
                league: league.name,
                reward: {
                    gems: league.qualification.reward.gems
                }
            }
            var message = Inbox.create(sparkPlayer, msgInfo);
            InboxModel.add(sparkPlayer, message);
        }
        
        // Update league rewards
        var leagueDailyRewardMsgId = Inbox.find(sparkPlayer, "RewardDailyLeague");
        var inbox = InboxModel.get(sparkPlayer);
        var msg = inbox.messages[leagueDailyRewardMsgId];
        var leageuDailyRewardMsgInfo = {
            type: "RewardDailyLeague",
            isDaily: true,
            league: league.name,
            reward: {
                gems: league.dailyReward.gems,
                coins: league.dailyReward.coins 
            },
            startTime: msg.startTime,
            time: msg.startTime
        }
        var leagueDailyRewardMessage = Inbox.create(sparkPlayer, leageuDailyRewardMsgInfo);
        Inbox.update(sparkPlayer, leagueDailyRewardMsgId, leagueDailyRewardMessage);
    }
    
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
    
    var update = function(sparkPlayer, messageId, message) {
        var inbox = InboxModel.get(sparkPlayer);
        inbox.messages[messageId] = message;
        inbox.messages[messageId].id = messageId;
        InboxModel.set(sparkPlayer);
    };
    
    var find = function(sparkPlayer, msgType) {
        var inboxData = InboxModel.get(sparkPlayer, true);
        var keys = Object.keys(inboxData.messages);
        var found = false;
        var i = 0;
        while (!found && i < keys.length) {
            found = inboxData.messages[keys[i]].type == msgType;
            if (!found) i++;
        }
        
        return found ? inboxData.messages[keys[i]].id : null;
    }

    var findAll = function(sparkPlayer, msgType) {
        var inboxData = InboxModel.get(sparkPlayer, true);
        var keys = Object.keys(inboxData.messages);
        
        var messages = [];
        var i = 0;
        while (i < keys.length) {
            var found = inboxData.messages[keys[i]].type == msgType;
            if (found) {
                messages.push(inboxData.messages[keys[i]]);
            }
            
            i++;
        }
        
        return messages;
    }
    
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

    var _getDefaultMessage = function () {
        return {
            id: 0,
            type: '',
            isDaily: false,
            heading: '',
            body: '',
            time: moment.utc().valueOf(),
            reward: {},
            trophies: 0,
            rank: 0,
            chestType: '',
            tournamentType: '',
            league: '',
            startTime: moment.utc().valueOf()
        }
    };

*/