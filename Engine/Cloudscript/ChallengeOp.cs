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
        public int trophies;
        public int league;
        public long piggyBankReward;
        public long piggyBankExipryTimestamp;
        public PlayerDataEvent dailyEventData;
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
                opResult.status = true;
                var challengeData = BsonSerializer.Deserialize<ChallengeData>(data["challengeData"].ToString());
                opResult.challengeId = SocialEdgeChallenge.ChallengeModel.Create(challengeData);
                SocialEdgeChallenge.ChallengeModel.ReadOnly();
            }
            else if (op == "endChallenge")
            {
                string challengeId = Args["challengeId"].ToString();
                string winnerId = Args["winnerId"].ToString();
                string gameEndReason = Args["gameEndReason"].ToString();

                ChallengeData challengeData = SocialEdgeChallenge.ChallengeModel.Challenge;
                SocialEdgePlayerContext player1 = SocialEdgePlayer;
                SocialEdgePlayerContext player2 = challengeData.player1Data.playerId == player1.PlayerId ? 
                                                                        LoadPlayer(challengeData.player2Data.playerId) :
                                                                        LoadPlayer(challengeData.player1Data.playerId);

                Challenge.EndGame(SocialEdgeChallenge, SocialEdgeTournament, player1, player2, gameEndReason, winnerId);

                opResult.status = true;
                opResult.challengeId = challengeId;
                opResult.challengeEndedInfo = new ChallengeEndDataModel();
                opResult.challengeEndedInfo.playersData = new Dictionary<string, ChallengeEndPlayerModel>();
                opResult.challengeEndedInfo.playersData.Add(player1.PlayerId, CreateChallengeEndPlayerModelResult(player1.PlayerModel, SocialEdgeChallenge.ChallengeModel.Challenge.player1Data));
                opResult.challengeEndedInfo.playersData.Add(player2.PlayerId, CreateChallengeEndPlayerModelResult(player2.PlayerModel, SocialEdgeChallenge.ChallengeModel.Challenge.player2Data));

                if (winnerId != null)
                {
                    ChallengePlayerModel winnerChallengeModel = SocialEdgeChallenge.ChallengeModel.Challenge.player1Data.playerId == player1.PlayerId ?
                                                                    SocialEdgeChallenge.ChallengeModel.Challenge.player1Data : SocialEdgeChallenge.ChallengeModel.Challenge.player2Data;
                    opResult.challengeEndedInfo.winnerBonusRewards = new Dictionary<string, int>();
                    opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsFree1", winnerChallengeModel.winnerBonusRewards.bonusCoinsFree1);
                    opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsFree2", winnerChallengeModel.winnerBonusRewards.bonusCoinsFree2);
                    opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsFree3", winnerChallengeModel.winnerBonusRewards.bonusCoinsFree3);
                    opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsRV1", winnerChallengeModel.winnerBonusRewards.bonusCoinsRV1);
                    opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsRV2", winnerChallengeModel.winnerBonusRewards.bonusCoinsRV2);
                    opResult.challengeEndedInfo.winnerBonusRewards.Add("bonusCoinsRV3", winnerChallengeModel.winnerBonusRewards.bonusCoinsRV3);
                }
            }

            CacheFlush();
            return opResult;
        }

        private ChallengeEndPlayerModel CreateChallengeEndPlayerModelResult(PlayerDataModel playerModel, ChallengePlayerModel playerChallengeData)
        {
            ChallengeEndPlayerModel model = new ChallengeEndPlayerModel();
            model.dailyEventData = playerModel.Events;
            model.eloChange = 0;//TODO
            model.eloScore = playerModel.Info.eloScore;
            model.league = playerModel.Info.league;
            model.piggyBankExipryTimestamp = playerModel.Economy.piggyBankExpiryTimestamp;
            model.piggyBankReward = playerChallengeData.piggyBankReward;
            model.totalGamesLost = playerModel.Info.gamesLost;
            model.totalGamesWon = playerModel.Info.gamesWon;
            model.trophies = playerModel.Info.trophies;

            return model;
        }
    }
}
