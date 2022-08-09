/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.Common;
using System;
using System.Linq;

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
                winnerChallengeModel.eloChange = CalcEloChange(winnerChallengeModel.eloScore, loserChallengeModel.eloScore, 
                                                                    socialEdgeChallenge.ChallengeModel.Challenge.isRanked, winnerChallengeModel.isEventMatch)[0];
                loserChallengeModel.eloChange = CalcEloChange(loserChallengeModel.eloScore, winnerChallengeModel.eloScore, 
                                                                    socialEdgeChallenge.ChallengeModel.Challenge.isRanked, loserChallengeModel.isEventMatch)[1];

                SocialEdgePlayerContext winnerPlayer = winnerId == player1.PlayerId ? player1 : player2;
                SocialEdgePlayerContext loserPlayer = winnerId == player1.PlayerId ? player2 : player1;

                if (winnerPlayer != null)
                {
                    Tournaments.HandleTournamentMatchEnd(winnerChallengeModel, winnerPlayer, socialEdgeTournament, true, false);
                    WinGame(socialEdgeChallenge.ChallengeModel.Challenge, winnerPlayer);
                }

                if (loserPlayer != null)
                {
                    Tournaments.HandleTournamentMatchEnd(loserChallengeModel, loserPlayer, socialEdgeTournament, false, false);
                    LoseGame(socialEdgeChallenge.ChallengeModel.Challenge, loserPlayer);
                }

                if (winnerPlayer != null && loserPlayer != null)
                    UpdateWinLoseGameFriendsInfo(winnerPlayer, loserPlayer);
            }
            else
            {
                if (player1 != null)
                {
                    Tournaments.HandleTournamentMatchEnd(socialEdgeChallenge.ChallengeModel.Challenge.player1Data, player1, socialEdgeTournament, false, true);
                    DrawGame(socialEdgeChallenge.ChallengeModel.Challenge.player1Data, player1);
                }
                
                if (player2 != null)
                {
                    Tournaments.HandleTournamentMatchEnd(socialEdgeChallenge.ChallengeModel.Challenge.player2Data, player2, socialEdgeTournament, false, true);
                    DrawGame(socialEdgeChallenge.ChallengeModel.Challenge.player2Data, player2);
                }

                if (player1 != null && player2 != null)
                {
                    UpdateDrawGameFriendsInfo(player1, player2);
                }

            }

            if (!isAbandoned)
            {
                socialEdgeChallenge.ChallengeModel.Challenge.player1Data.piggyBankReward = 0;
                socialEdgeChallenge.ChallengeModel.Challenge.player2Data.piggyBankReward = 0;
                // TODO HandlePiggyBankReward(player1, player2);
            }

            if (player1 != null)
                player1.PlayerModel.Challenge.currentChallengeId = null;

            if (player2 != null)
                player2.PlayerModel.Challenge.currentChallengeId = null;
        }

        private static void WinGame(ChallengeData challengeData, SocialEdgePlayerContext winnerPlayer)
        {
            challengeData.winnerId = winnerPlayer.PlayerId;
            ChallengePlayerModel winnerChallengePlayerModel = challengeData.player1Data.playerId == challengeData.winnerId ? challengeData.player1Data : challengeData.player2Data;
            winnerPlayer.PlayerModel.Info.gamesWon = winnerPlayer.PlayerModel.Info.gamesWon + 1;
            winnerPlayer.PlayerModel.Info.eloScore = winnerPlayer.PlayerModel.Info.eloScore + winnerChallengePlayerModel.eloChange;
            
            if (winnerChallengePlayerModel.isEventMatch)
            {
                winnerPlayer.PlayerEconomy.ProcessDailyEvent();
                if (winnerPlayer.PlayerModel.Events.dailyEventProgress < winnerPlayer.PlayerModel.Events.dailyEventRewards.Count) 
                    winnerPlayer.PlayerModel.Events.dailyEventProgress = winnerPlayer.PlayerModel.Events.dailyEventProgress + 1;
            }

            if (winnerChallengePlayerModel.betValue > 0)
            {
                float matchCoinsMultiplyer = float.Parse(Settings.CommonSettings["matchCoinsMultiplyer"].ToString());
                int rewardBet = (int)(winnerChallengePlayerModel.betValue * matchCoinsMultiplyer);
                winnerPlayer.PlayerEconomy.AddVirtualCurrency("CN", rewardBet);
            }

            var todayResults = GetTodaysGameResults(winnerPlayer);
            todayResults.won++;
            SetTodaysGamesResults(winnerPlayer, todayResults);
        }

        private static void LoseGame(ChallengeData challengeData, SocialEdgePlayerContext loserPlayer)
        {
            ChallengePlayerModel loserChallengePlayerModel = challengeData.player1Data.playerId == challengeData.winnerId ? challengeData.player2Data : challengeData.player1Data;
            loserPlayer.PlayerModel.Info.gamesLost = loserPlayer.PlayerModel.Info.gamesLost + 1;
            loserPlayer.PlayerModel.Info.eloScore = loserPlayer.PlayerModel.Info.eloScore + loserChallengePlayerModel.eloChange;
            
            if (loserChallengePlayerModel.isEventMatch) 
            {
                loserPlayer.PlayerEconomy.ProcessDailyEvent();
                loserPlayer.PlayerModel.Events.dailyEventState = "lost";
            }

            var todayResults = GetTodaysGameResults(loserPlayer);
            todayResults.lost++;
            SetTodaysGamesResults(loserPlayer, todayResults);
        }

        private static void UpdateWinLoseGameFriendsInfo(SocialEdgePlayerContext winnerPlayer, SocialEdgePlayerContext loserPlayer)
        {
            if (winnerPlayer.PlayerModel.Friends.friends.ContainsKey(loserPlayer.PlayerId))
            {
                FriendData friendData = winnerPlayer.PlayerModel.Friends.friends[loserPlayer.PlayerId];
                friendData.GamesWonInc();
            }

            if (loserPlayer.PlayerModel.Friends.friends.ContainsKey(winnerPlayer.PlayerId))
            {
                FriendData friendData = loserPlayer.PlayerModel.Friends.friends[winnerPlayer.PlayerId];
                friendData.GamesWonInc();
            }
        }

        public static void UpdateDrawGameFriendsInfo(SocialEdgePlayerContext player1, SocialEdgePlayerContext player2)
        {
            if (player1.PlayerModel.Friends.friends.ContainsKey(player2.PlayerId))
            {
                FriendData friendData = player1.PlayerModel.Friends.friends[player2.PlayerId];
                friendData.GamesDrawnInc();
            }

            if (player2.PlayerModel.Friends.friends.ContainsKey(player1.PlayerId))
            {
                FriendData friendData = player2.PlayerModel.Friends.friends[player1.PlayerId];
                friendData.GamesDrawnInc();
            }
        }

        public static void DrawGame(ChallengePlayerModel challengePlayerModel, SocialEdgePlayerContext player)
        {
            player.PlayerModel.Info.gamesDrawn = player.PlayerModel.Info.gamesDrawn + 1;

            if (challengePlayerModel.betValue > 0)
                player.PlayerEconomy.AddVirtualCurrency("CN", (int)challengePlayerModel.betValue);

            var todayResults = GetTodaysGameResults(player);
            todayResults.drawn++;
            SetTodaysGamesResults(player, todayResults);
        }

        private static int[] CalcEloChange(int elo1, int elo2, bool isRanked, bool isEventMatch)
        {
            int[] ret = new int[2];
            ret[0] = ret[1] = 0;

            if (isRanked && !isEventMatch)
            {
                float diff = elo1 - elo2;

                // Win
                ret[0] = diff <= 0 ? 10 : 5;
                // Loss
                ret[1] = diff < 0 ? -5 : -10;
            }

            return ret;
        }

        private static GameResults GetTodaysGameResults(SocialEdgePlayerContext playerContext)
        {
            var dateKey = DateTime.Now.ToShortDateString();
            var gamesPlayedPerDay = playerContext.PlayerModel.Info.gamesPlayedPerDay;
            
            if(gamesPlayedPerDay.ContainsKey(dateKey))
            {
                return gamesPlayedPerDay[dateKey];
            }

            var gameResults = new GameResults();
            playerContext.PlayerModel.Info.gamesPlayedPerDay.Add(dateKey, gameResults);

            //its only meant to store data for last three days
            //removing the first element the dictionary
            if(gamesPlayedPerDay.Count >= 4)
            {
                var firstElement = gamesPlayedPerDay.Keys.First();
                gamesPlayedPerDay.Remove(firstElement);
            }

            return gameResults;
        }

        private static void SetTodaysGamesResults(SocialEdgePlayerContext playerContext, GameResults gameResults)
        {
            var dateKey = DateTime.Now.ToShortDateString();
            playerContext.PlayerModel.Info.gamesPlayedPerDay[dateKey] = gameResults;
        }
    }
}
