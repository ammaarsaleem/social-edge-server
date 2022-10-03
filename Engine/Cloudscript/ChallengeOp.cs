/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PlayFab.Samples;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Models;
using MongoDB.Bson.Serialization;
using SocialEdgeSDK.Server.Context;
using System.Linq;

namespace SocialEdgeSDK.Server.Requests
{
    public class ChallengeEndDataModel
    {
        public Dictionary<string, int> winnerBonusRewards;
        public Dictionary<string, ChallengeEndPlayerModel> playersData;
    }

    public class ChallengeEndPlayerModel
    {
        public int eloChange;
        public int eloScore;
        public int totalGamesWon;
        public int totalGamesLost;
        public int totalGamesDrawn;
        public int trophies;
        public int league;
        public long piggyBankReward;
        public long piggyBankExipryTimestamp;
        public PlayerDataEvent dailyEventData;
        public FriendData friendData;
        public ChallengeEndPlayerModel (PlayerDataModel playerModel, PlayerMiniProfileData miniProfile, ChallengePlayerModel playerChallengeData, FriendData friend)
        {
            dailyEventData = playerModel.Events;
            eloChange = playerChallengeData.eloChange;
            eloScore = playerModel.Info.eloScore;
            league = miniProfile.League;
            piggyBankExipryTimestamp = playerModel.Economy.piggyBankExpiryTimestamp;
            piggyBankReward = playerChallengeData.piggyBankReward;
            totalGamesLost = playerModel.Info.gamesLost;
            totalGamesWon = playerModel.Info.gamesWon;
            totalGamesDrawn = playerModel.Info.gamesDrawn;
            trophies = playerModel.Info.trophies;
            friendData = friend;
        }
    }

    public class ChallengeOpResult
    {
        public string op;
        public bool status;
        public string challengeId;
        public ChallengeEndDataModel challengeEndedInfo;
    }

    public class ChallengeOp : FunctionContext
    {
        public ChallengeOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("ChallengeOp")]
        public ChallengeOpResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            ChallengeOpResult opResult = new ChallengeOpResult();
            var op = data["op"];
            opResult.op = op;

            if (op == "startChallenge")
            {
                SocialEdgePlayer.PlayerModel.Prefetch(PlayerModelFields.INFO, PlayerModelFields.CHALLENGE, PlayerModelFields.TOURNAMENT);                                                                
                var challengeData = BsonSerializer.Deserialize<ChallengeData>(data["challengeData"].ToString());

                //patch due a bug in client v6.34.22
                //TODO: remove after mandatory update
                var playerKeyToUpdate = string.Empty;
                var newPlayerKey = string.Empty;

                foreach(var player in challengeData.playersData)
                {
                    if(!player.Value.isBot && !string.IsNullOrEmpty(player.Value.tournamentId) && !player.Key.Equals(SocialEdgePlayer.PlayerId))
                    {
                        var tournamentModel = SocialEdgeTournament.TournamentModel.Get(player.Value.tournamentId);

                        if(tournamentModel != null && !string.IsNullOrEmpty(tournamentModel.playerId) && !player.Key.Equals(tournamentModel.playerId))
                        {
                            playerKeyToUpdate = player.Key;
                            newPlayerKey = tournamentModel.playerId;
                        }
                    }
                }

                if(!string.IsNullOrEmpty(playerKeyToUpdate))
                {
                    var playerValue = challengeData.playersData[playerKeyToUpdate];
                    challengeData.playersData.Remove(playerKeyToUpdate);
                    challengeData.playersData.Add(newPlayerKey, playerValue);
                }
                //patch end

                var challengeId = SocialEdgeChallenge.ChallengeModel.Create(challengeData);
                SocialEdgeChallenge.ChallengeModel.ReadOnly();

                foreach(var player in challengeData.playersData)
                {
                    if(!player.Value.isBot)
                    {
                        var socialEdgePlayer = player.Key == SocialEdgePlayer.PlayerId ? SocialEdgePlayer : LoadPlayer(player.Key);
                        socialEdgePlayer.PlayerModel.Challenge.currentChallengeId = challengeId;
                        if(player.Value.betValue > 0)
                        {
                            socialEdgePlayer.PlayerEconomy.SubtractVirtualCurrency("CN", (int)player.Value.betValue);
                        }
                    }
                }

                opResult.status = true;
                opResult.challengeId = challengeId;
            }
            else if (op == "endChallenge")
            {
                SocialEdgePlayer.PlayerModel.Prefetch( PlayerModelFields.ECONOMY, 
                                                       PlayerModelFields.EVENTS, 
                                                       PlayerModelFields.INFO, 
                                                       PlayerModelFields.CHALLENGE, 
                                                       PlayerModelFields.TOURNAMENT);

                string challengeId = data["challengeId"].ToString();
                string winnerId = data["winnerId"].ToString();
                string gameEndReason = data["gameEndReason"].ToString();

                ChallengeData challengeData = SocialEdgeChallenge.ChallengeModel.Get(challengeId);
                bool isAbandoned = !string.IsNullOrEmpty(challengeData.gameEndReason) && challengeData.gameEndReason.Equals("ABANDONED");
                SocialEdgeChallenge.ChallengeModel.Challenge.winnerId = winnerId;
                SocialEdgeChallenge.ChallengeModel.Challenge.gameEndReason = gameEndReason;
                
                opResult.status = true;
                opResult.challengeId = challengeId;
                opResult.challengeEndedInfo = new ChallengeEndDataModel();
                opResult.challengeEndedInfo.playersData = new Dictionary<string, ChallengeEndPlayerModel>();

                foreach(var player in challengeData.playersData)
                {
                    if(!player.Value.isBot)
                    {
                        var socialEdgePlayer = player.Key == SocialEdgePlayer.PlayerId ? SocialEdgePlayer : LoadPlayer(player.Key);

                        //in case someone already abandoned the challenge,
                        //reverting the already processed changes
                        if(isAbandoned)
                        {
                            socialEdgePlayer.PlayerModel.Info.gamesLost = socialEdgePlayer.PlayerModel.Info.gamesLost - 1;
                            socialEdgePlayer.PlayerModel.Info.eloScore = socialEdgePlayer.PlayerModel.Info.eloScore - player.Value.eloChange;
                            socialEdgePlayer.PlayerModel.Economy.piggyBankGems = socialEdgePlayer.PlayerModel.Economy.piggyBankGems - player.Value.piggyBankReward;

                            if (player.Value.isEventMatch) 
                            {
                                socialEdgePlayer.PlayerModel.Events.dailyEventState = "running";
                            }
                        }

                        var otherPlayerId = challengeData.playersData.Where(p => p.Key != socialEdgePlayer.PlayerId).Select(p => p.Key).FirstOrDefault();
                        Challenge.EndGame(SocialEdgeChallenge, SocialEdgeTournament, socialEdgePlayer, gameEndReason, winnerId, otherPlayerId);
                        var friendData = Friends.UpdateFriendsMatchTimestamp(otherPlayerId, socialEdgePlayer);
                        opResult.challengeEndedInfo.playersData.Add(player.Key, new ChallengeEndPlayerModel(socialEdgePlayer.PlayerModel, socialEdgePlayer.MiniProfile, player.Value, friendData));

                        if(!string.IsNullOrEmpty(player.Value.tournamentId) && !string.IsNullOrEmpty(winnerId) && winnerId.Equals(player.Key))
                        {
                            var winnerBonusRewards = new Dictionary<string, int>();
                            socialEdgePlayer.PlayerEconomy.ProcessWinnerBonusRewards(player.Value);
                            winnerBonusRewards.Add("bonusCoinsFree1", player.Value.winnerBonusRewards.bonusCoinsFree1);
                            winnerBonusRewards.Add("bonusCoinsFree2", player.Value.winnerBonusRewards.bonusCoinsFree2);
                            winnerBonusRewards.Add("bonusCoinsFree3", player.Value.winnerBonusRewards.bonusCoinsFree3);
                            winnerBonusRewards.Add("bonusCoinsRV1", player.Value.winnerBonusRewards.bonusCoinsRV1);
                            winnerBonusRewards.Add("bonusCoinsRV2", player.Value.winnerBonusRewards.bonusCoinsRV2);
                            winnerBonusRewards.Add("bonusCoinsRV3", player.Value.winnerBonusRewards.bonusCoinsRV3);
                            opResult.challengeEndedInfo.winnerBonusRewards = winnerBonusRewards;
                        }
                    }
                }
            }

            CacheFlush();
            return opResult;
        }
    }
}
