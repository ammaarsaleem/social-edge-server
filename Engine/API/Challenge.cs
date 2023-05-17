/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using SocialEdgeSDK.Server.Requests;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialEdgeSDK.Server.Api
{
    public static class Challenge
    {
        public static void EndGame(SocialEdgeChallengeContext socialEdgeChallenge, SocialEdgeTournamentContext socialEdgeTournament, SocialEdgePlayerContext socialEdgePlayer, string gameEndReason, string winnerId, string otherPlayerId)
        {
            bool winLoseGame = !string.IsNullOrEmpty(winnerId);
            bool isAbandoned = !winLoseGame && string.IsNullOrEmpty(gameEndReason);
            bool playerWins = socialEdgePlayer.PlayerId == winnerId;
            var playersData = socialEdgeChallenge.ChallengeModel.Challenge.playersData;
            ChallengePlayerModel currentPlayerData = playersData[socialEdgePlayer.PlayerId];
            ChallengePlayerModel otherPlayerData = playersData[otherPlayerId];

            if (winLoseGame || isAbandoned)
            {
                // Set elo score changes first
                currentPlayerData.eloChange = CalcEloChange(currentPlayerData.eloScore, otherPlayerData.eloScore, socialEdgeChallenge.ChallengeModel.Challenge.isRanked, currentPlayerData.isEventMatch)[playerWins ? 0 : 1];
                Tournaments.HandleTournamentMatchEnd(currentPlayerData, socialEdgePlayer, socialEdgeTournament, playerWins, false);

                if (playerWins)
                {
                    WinGame(currentPlayerData, socialEdgePlayer);
                }
                else 
                {
                    LoseGame(currentPlayerData, socialEdgePlayer);
                }

                if (!currentPlayerData.isBot && !otherPlayerData.isBot)
                {
                    UpdateWinLoseGameFriendsInfo(socialEdgePlayer, otherPlayerId, playerWins);
                }
            }
            else
            {
                Tournaments.HandleTournamentMatchEnd(currentPlayerData, socialEdgePlayer, socialEdgeTournament, false, true);
                DrawGame(currentPlayerData, socialEdgePlayer);

                if (!currentPlayerData.isBot && !otherPlayerData.isBot)
                {
                    UpdateDrawGameFriendsInfo(socialEdgePlayer, otherPlayerId);
                }
            }

            currentPlayerData.piggyBankReward = socialEdgePlayer.PlayerEconomy.ProcessPiggyBankReward(currentPlayerData);
            socialEdgePlayer.PlayerModel.Challenge.lastPlayedChallengeId = socialEdgePlayer.PlayerModel.Challenge.currentChallengeId;
            socialEdgePlayer.PlayerModel.Challenge.currentChallengeId = null;
        }

        private static void WinGame(ChallengePlayerModel playerChallengeData, SocialEdgePlayerContext socialEdgePlayer)
        {
            socialEdgePlayer.PlayerModel.Info.gamesWon = socialEdgePlayer.PlayerModel.Info.gamesWon + 1;
            socialEdgePlayer.PlayerModel.Info.eloScore = socialEdgePlayer.PlayerModel.Info.eloScore + playerChallengeData.eloChange;
            
            if (playerChallengeData.isEventMatch)
            {
                socialEdgePlayer.PlayerEconomy.ProcessDailyEvent();
                if (socialEdgePlayer.PlayerModel.Events.dailyEventProgress < socialEdgePlayer.PlayerModel.Events.dailyEventRewards.Count) 
                    socialEdgePlayer.PlayerModel.Events.dailyEventProgress = socialEdgePlayer.PlayerModel.Events.dailyEventProgress + 1;
            }

            if (playerChallengeData.betValue > 0)
            {
                float matchCoinsMultiplyer = float.Parse(Settings.CommonSettings["matchCoinsMultiplyer"].ToString());
                int rewardBet = (int)(playerChallengeData.betValue * matchCoinsMultiplyer);
                socialEdgePlayer.PlayerEconomy.AddVirtualCurrency("CN", rewardBet);
            }

            var todayResults = GetTodaysGameResults(socialEdgePlayer);
            todayResults.won++;
            SetTodaysGamesResults(socialEdgePlayer, todayResults);
        }

        private static void LoseGame(ChallengePlayerModel playerChallengeData, SocialEdgePlayerContext socialEdgePlayer)
        {
            socialEdgePlayer.PlayerModel.Info.gamesLost = socialEdgePlayer.PlayerModel.Info.gamesLost + 1;
            socialEdgePlayer.PlayerModel.Info.eloScore = socialEdgePlayer.PlayerModel.Info.eloScore + playerChallengeData.eloChange;
            
            if (playerChallengeData.isEventMatch) 
            {
                socialEdgePlayer.PlayerEconomy.ProcessDailyEvent();
                socialEdgePlayer.PlayerModel.Events.dailyEventState = "lost";
            }

            var todayResults = GetTodaysGameResults(socialEdgePlayer);
            todayResults.lost++;
            SetTodaysGamesResults(socialEdgePlayer, todayResults);
        }

        private static void UpdateWinLoseGameFriendsInfo(SocialEdgePlayerContext player, string otherPlayerId, bool playerWins)
        {
            if (player.PlayerModel.Friends.friends.ContainsKey(otherPlayerId))
            {
                FriendData friendData = player.PlayerModel.Friends.friends[otherPlayerId];
                
                if(playerWins)
                {
                    friendData.GamesWonInc();
                }
                else
                {
                    friendData.GamesLostInc();
                }
            }
        }

        public static void UpdateDrawGameFriendsInfo(SocialEdgePlayerContext player, string otherPlayerId)
        {
            if (player.PlayerModel.Friends.friends.ContainsKey(otherPlayerId))
            {
                FriendData friendData = player.PlayerModel.Friends.friends[otherPlayerId];
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

        public static void ProcessAbandonedGame(SocialEdgePlayerContext SocialEdgePlayer, SocialEdgeChallengeContext SocialEdgeChallenge, SocialEdgeTournamentContext SocialEdgeTournament, FunctionContext functionContext)
        {
            if (string.IsNullOrEmpty(SocialEdgePlayer.PlayerModel.Challenge.currentChallengeId))
                return;

            ChallengeData challengeData = SocialEdgeChallenge.ChallengeModel.Get(SocialEdgePlayer.PlayerModel.Challenge.currentChallengeId);
            
            // Bail if challenge model removed from db
            if (challengeData == null)
            {
                SocialEdgePlayer.PlayerModel.Challenge.currentChallengeId = null;
                return;
            }
            
            SocialEdgeChallenge.ChallengeModel.Challenge.winnerId = null;
            SocialEdgeChallenge.ChallengeModel.Challenge.gameEndReason = "ABANDONED";

            foreach (var player in challengeData.playersData)
            {
                if (!player.Value.isBot)
                {
                    var socialEdgePlayer = player.Key == SocialEdgePlayer.PlayerId ? SocialEdgePlayer : functionContext.LoadPlayer(player.Key);
                    var otherPlayerId = challengeData.playersData.Where(p => p.Key != socialEdgePlayer.PlayerId).Select(p => p.Key).FirstOrDefault();
                    Challenge.EndGame(SocialEdgeChallenge, SocialEdgeTournament, socialEdgePlayer, null, null, otherPlayerId);
                    Friends.UpdateFriendsMatchTimestamp(otherPlayerId, socialEdgePlayer);
                }
            }
        }

        public static void ProcessChessPuzzle(SocialEdgePlayerContext SocialEdgePlayer)
        {
            SocialEdgePlayer.PlayerModel.Challenge.puzzleIndex = SocialEdgePlayer.PlayerModel.Challenge.puzzleIndex == 0 ? 1 : SocialEdgePlayer.PlayerModel.Challenge.puzzleIndex;
            var puzzle = new ChessPuzzle();
            var puzzleDocumentT = CommonModel.GetPuzzle(SocialEdgePlayer.PlayerModel.Challenge.puzzleIndex);
            puzzleDocumentT.Wait();
            puzzle.fen = puzzleDocumentT.Result.fen;
            puzzle.moves = puzzleDocumentT.Result.moves.Split(' ').ToList();
            puzzle.description = puzzleDocumentT.Result.description;
            SocialEdgePlayer.PlayerModel.Challenge.puzzle = puzzle;
        }

        public static void ProcessCpuStats(SocialEdgePlayerContext SocialEdgePlayer)
        {
            if(SocialEdgePlayer.PlayerModel.Challenge.cpuStats != null)
            {
                var unlockedLevel = SocialEdgePlayer.PlayerModel.Challenge.cpuStats.unlockedLevel;
                var currentLevel = SocialEdgePlayer.PlayerModel.Challenge.cpuStats.currentLevel;
                unlockedLevel = unlockedLevel > 10 ? 10 : unlockedLevel;
                currentLevel = currentLevel > 10 ? 10 : currentLevel;
                SocialEdgePlayer.PlayerModel.Challenge.cpuStats.unlockedLevel = unlockedLevel;
                SocialEdgePlayer.PlayerModel.Challenge.cpuStats.currentLevel = currentLevel;
            }
        }
    }
}
