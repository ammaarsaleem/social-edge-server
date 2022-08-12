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
using  SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Requests
{
    public class TournamentsOpResult
    {
        public string op;
        public bool status;
        public string tournamentId;
        public List<TournamentLeaderboardEntry> leaderboardEntries;
        public TournamentData tournamentData;

        public Dictionary<string, ActiveTournament> activeTournaments;
        public int league;
        public int trophies;
        public int trophies2;
        public string inbox;
        public int inboxCount;
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
            opResult.op = op;

            if (op == "getTournamentLeaderboard")
            {
                string tournamentId = data["tournamentId"].ToString();

                Tournaments.FillAvailablePoolEntries(tournamentId, SocialEdgePlayer, SocialEdgeTournament);

                List<TournamentLeaderboardEntry> entries = Tournaments.GetLeaderboard(SocialEdgePlayer, SocialEdgeTournament, tournamentId);
                opResult.tournamentId = data["tournamentId"];
                opResult.leaderboardEntries = entries;
                opResult.tournamentData = SocialEdgeTournament.TournamentModel.Get(tournamentId);
                opResult.status = true;
            }
            else if (op == "updateTournaments")
            {
                Tournaments.UpdateTournaments(SocialEdgePlayer, SocialEdgeTournament);

                opResult.activeTournaments = SocialEdgePlayer.PlayerModel.Tournament.activeTournaments;
                opResult.league = SocialEdgePlayer.MiniProfile.League;
                opResult.trophies = SocialEdgePlayer.PlayerModel.Info.trophies;
                opResult.trophies2 = SocialEdgePlayer.PlayerModel.Info.trophies2;
                opResult.inbox = SocialEdgePlayer.InboxJson;
                opResult.inboxCount = InboxModel.Count(SocialEdgePlayer);
                opResult.status = true;
            }

            SocialEdgePlayer.CacheFlush();
            SocialEdgeTournament.CacheFlush();
            return opResult;
        }
    }
}
