/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PlayFab.Samples;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Api;

namespace SocialEdgeSDK.Server.Requests
{
    public class TournamentsOpResult
    {
        public bool status;
    }

    public class TournamentsOp : FunctionContext
    {
        public TournamentsOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("TournamentsOp")]
        public TournamentsOpResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            TournamentsOpResult opResult = new TournamentsOpResult();
            var op = data["op"];

            if (op == "join")
            {
                var result = Tournaments.Join(SocialEdgePlayer, SocialEdgeTournament, data["tournamentShortCode"].ToString(), (int)data["score"]);
/*
                if (result.hasOwnProperty('errorCode')) {
                    Spark.setScriptData('error', result);
                }
                else {
                    var playerData = PlayerModel.get(sparkPlayer);
                    Spark.setScriptData('tournaments', playerData.priv.activeTournaments);
                    Spark.setScriptData('tournamentId', result);
                }
            */
            }
            SocialEdgePlayer.CacheFlush();
            
            return opResult;

        }
    }
}
