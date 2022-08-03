/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;

namespace SocialEdgeSDK.Server.Api
{
    public static class Challenge
    {
        public static void EndGame(SocialEdgeChallengeContext socialEdgeChallenge, SocialEdgeTournamentContext socialEdgeTournament,
                                SocialEdgePlayerContext player1, SocialEdgePlayerContext player2, 
                                string gameEndReason, string winnerId)
        {
            bool winLoseGame = !string.IsNullOrEmpty(winnerId);
            bool isAbandoned = !winLoseGame && string.IsNullOrEmpty(gameEndReason);
            socialEdgeChallenge.ChallengeModel.Challenge.gameEndReason = gameEndReason;

            if (winLoseGame)
            {
                // Set elo score changes first
                ChallengePlayerModel winnerChallengeModel = socialEdgeChallenge.ChallengeModel.Challenge.player1Data.playerId == winnerId ?
                                                             socialEdgeChallenge.ChallengeModel.Challenge.player1Data : socialEdgeChallenge.ChallengeModel.Challenge.player2Data;
                ChallengePlayerModel loserChallengeModel = socialEdgeChallenge.ChallengeModel.Challenge.player1Data.playerId == winnerId ?
                                                             socialEdgeChallenge.ChallengeModel.Challenge.player2Data : socialEdgeChallenge.ChallengeModel.Challenge.player1Data;
                winnerChallengeModel.eloChange = CalcEloChange(winnerChallengeModel.eloScore, loserChallengeModel.eloScore, true)[0];
                loserChallengeModel.eloChange = CalcEloChange(loserChallengeModel.eloScore, winnerChallengeModel.eloScore, true)[1];

                SocialEdgePlayerContext winnerPlayer = winnerId == player1.PlayerId ? player1 : player2;
                SocialEdgePlayerContext loserPlayer = winnerId == player1.PlayerId ? player2 : player1;
                Tournaments.HandleTournamentMatchEnd(socialEdgeChallenge, winnerPlayer, socialEdgeTournament, true);
                Tournaments.HandleTournamentMatchEnd(socialEdgeChallenge, loserPlayer, socialEdgeTournament, false);
                WinLoseGame(socialEdgeChallenge.ChallengeModel.Challenge, winnerPlayer, loserPlayer);

            }
            else
            {
                Tournaments.HandleTournamentMatchEnd(socialEdgeChallenge, player1, socialEdgeTournament, false);
                Tournaments.HandleTournamentMatchEnd(socialEdgeChallenge, player2, socialEdgeTournament, false);
                DrawGame(socialEdgeChallenge.ChallengeModel.Challenge, player1, player2);
            }

            if (!isAbandoned)
            {
                socialEdgeChallenge.ChallengeModel.Challenge.player1Data.piggyBankReward = 0;
                socialEdgeChallenge.ChallengeModel.Challenge.player2Data.piggyBankReward = 0;
                // TODO HandlePiggyBankReward(player1, player2);
            }
        }

        private static void WinLoseGame(ChallengeData challengeData, SocialEdgePlayerContext winnerPlayer, SocialEdgePlayerContext loserPlayer)
        {
            challengeData.winnerId = winnerPlayer.PlayerId;

            ChallengePlayerModel winnerChallengePlayerModel = challengeData.player1Data.playerId == challengeData.winnerId ? challengeData.player1Data : challengeData.player2Data;
            ChallengePlayerModel loserChallengePlayerModel = challengeData.player1Data.playerId == challengeData.winnerId ? challengeData.player2Data : challengeData.player1Data;
            
            winnerPlayer.PlayerModel.Info.gamesWon++;
            loserPlayer.PlayerModel.Info.gamesLost++;
            winnerPlayer.PlayerModel.Info.eloScore += winnerChallengePlayerModel.eloChange;
            loserPlayer.PlayerModel.Info.eloScore += loserChallengePlayerModel.eloChange;

            if (winnerPlayer.PlayerModel.Friends.friends.ContainsKey(loserPlayer.PlayerId))
            {
                FriendData friendData = winnerPlayer.PlayerModel.Friends.friends[loserPlayer.PlayerId];
                friendData.gamesWon++;
            }

            if (loserPlayer.PlayerModel.Friends.friends.ContainsKey(winnerPlayer.PlayerId))
            {
                FriendData friendData = loserPlayer.PlayerModel.Friends.friends[winnerPlayer.PlayerId];
                friendData.gamesWon++;
            }

            if (winnerChallengePlayerModel.isEventMatch)
            {
                // TODO ProcessDailyEventExpiry(winnerPlayer);
                if (winnerPlayer.PlayerModel.Events.dailyEventProgress < SocialEdge.TitleContext.GetTitleDataProperty("DailyEventRewards").length) 
                    winnerPlayer.PlayerModel.Events.dailyEventProgress++;
            }

            if (loserChallengePlayerModel.isEventMatch) 
            {
                // TODO ProcessDailyEventExpiry(loserPlayer);
                loserPlayer.PlayerModel.Events.dailyEventState = "lost";
            }

            if (winnerChallengePlayerModel.betValue > 0)
            {
                double coinsMultiplyer = winnerChallengePlayerModel.coinsMultiplyer;// TODO get from settings
                winnerPlayer.VirtualCurrency["CN"] +=  (int)(winnerChallengePlayerModel.betValue * coinsMultiplyer);
            }
        }

        public static void DrawGame(ChallengeData challengeData, SocialEdgePlayerContext player1, SocialEdgePlayerContext player2)
        {
            player1.PlayerModel.Info.gamesDrawn++;
            player2.PlayerModel.Info.gamesDrawn++;

            if (player1.PlayerModel.Friends.friends.ContainsKey(player2.PlayerId))
            {
                FriendData friendData = player1.PlayerModel.Friends.friends[player2.PlayerId];
                friendData.gamesDrawn++;
            }

            if (player2.PlayerModel.Friends.friends.ContainsKey(player1.PlayerId))
            {
                FriendData friendData = player2.PlayerModel.Friends.friends[player1.PlayerId];
                friendData.gamesDrawn++;
            }

            if (challengeData.player1Data.betValue > 0)
            {
                // TODO get from settings
                double coinsMultiplyer = challengeData.player1Data.coinsMultiplyer;
                player1.VirtualCurrency["CN"] +=  (int)(challengeData.player1Data.betValue * coinsMultiplyer);
            }

            if (challengeData.player2Data.betValue > 0)
            {
                // TODO get from settings
                double coinsMultiplyer = challengeData.player2Data.coinsMultiplyer;
                player2.VirtualCurrency["CN"] +=  (int)(challengeData.player2Data.betValue * coinsMultiplyer);
            }
        }

        private static int[] CalcEloChange(int elo1, int elo2, bool isRanked)
        {
            int[] ret = new int[2];
            ret[0] = ret[1] = 0;

            if (isRanked)
            {
                float diff = elo1 - elo2;

                // Win
                ret[0] = diff <= 0 ? 10 : 5;
                // Loss
                ret[1] = diff <= 0 ? -5 : -10;
            }

            return ret;
        }
    }
}
