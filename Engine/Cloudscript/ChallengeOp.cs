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

namespace SocialEdgeSDK.Server.Requests
{
    public class ChallengeOpResult
    {
        public string op;
        public bool status;
        public string challengeId;
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

                //parse challenge data
                var challengeData = BsonSerializer.Deserialize<ChallengeData>(data["challengeData"].ToString());

                //save challenge data in db
                var challengeId = SocialEdgeChallenge.ChallengeModel.Create(challengeData);

                //load second player data
                var socialEdgePlayer2 = LoadPlayer(challengeData.player2Data.playerId);

                //save challenge id for both players and in response
                SocialEdgeChallenge.ChallengeModel.ReadOnly();
                opResult.challengeId = challengeId;
                SocialEdgePlayer.PlayerModel.Challenge.currentChallengeId = challengeId;
                socialEdgePlayer2.PlayerModel.Challenge.currentChallengeId = challengeId;

                //deduct bet amount for both players
                if(challengeData.player1Data.betValue > 0)
                {
                    Transactions.Consume("coins", challengeData.player1Data.betValue, SocialEdgePlayer);
                }

                if(challengeData.player2Data.betValue > 0)
                {
                    Transactions.Consume("coins", challengeData.player2Data.betValue, socialEdgePlayer2);
                }
            }
            else if (op == "endChallenge")
            {
                opResult.status = true;
            }

            CacheFlush();
            return opResult;
        }
    }
}
