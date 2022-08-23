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
                SocialEdgePlayer.PlayerModel.Prefetch(new List<string>() {  PlayerModelFields.INFO, 
                                                                            PlayerModelFields.CHALLENGE, 
                                                                            PlayerModelFields.TOURNAMENT});
                                                                            
                var challengeData = BsonSerializer.Deserialize<ChallengeData>(data["challengeData"].ToString());
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
                SocialEdgePlayer.PlayerModel.Prefetch(new List<string>() {  PlayerModelFields.ECONOMY, 
                                                                            PlayerModelFields.EVENTS, 
                                                                            PlayerModelFields.INFO, 
                                                                            PlayerModelFields.CHALLENGE, 
                                                                            PlayerModelFields.TOURNAMENT});

                string challengeId = data["challengeId"].ToString();
                string winnerId = data["winnerId"].ToString();
                string gameEndReason = data["gameEndReason"].ToString();

                ChallengeData challengeData = SocialEdgeChallenge.ChallengeModel.Get(challengeId);
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
                        var otherPlayerId = challengeData.playersData.Where(p => p.Key != socialEdgePlayer.PlayerId).Select(p => p.Key).FirstOrDefault();
                        Challenge.EndGame(SocialEdgeChallenge, SocialEdgeTournament, socialEdgePlayer, gameEndReason, winnerId, otherPlayerId);
                        var friendData = Friends.UpdateFriendsMatchTimestamp(otherPlayerId, socialEdgePlayer);
                        opResult.challengeEndedInfo.playersData.Add(player.Key, new ChallengeEndPlayerModel(socialEdgePlayer.PlayerModel, socialEdgePlayer.MiniProfile, player.Value, friendData));
                    }
                }

                if (!string.IsNullOrEmpty(winnerId))
                {
                    ChallengePlayerModel winnerChallengeModel = SocialEdgeChallenge.ChallengeModel.Challenge.playersData[winnerId];

                    if (winnerChallengeModel.winnerBonusRewards != null)
                    {
                        opResult.challengeEndedInfo.winnerBonusRewards = new Dictionary<string, int>();
                        opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsFree1", winnerChallengeModel.winnerBonusRewards.bonusCoinsFree1);
                        opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsFree2", winnerChallengeModel.winnerBonusRewards.bonusCoinsFree2);
                        opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsFree3", winnerChallengeModel.winnerBonusRewards.bonusCoinsFree3);
                        opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsRV1", winnerChallengeModel.winnerBonusRewards.bonusCoinsRV1);
                        opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsRV2", winnerChallengeModel.winnerBonusRewards.bonusCoinsRV2);
                        opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsRV3", winnerChallengeModel.winnerBonusRewards.bonusCoinsRV3);
                    }
                }
            }

            CacheFlush();
            return opResult;
        }
    }
}
