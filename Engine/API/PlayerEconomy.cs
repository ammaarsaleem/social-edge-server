/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using PlayFab.ProfilesModels;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Api;
using SocialEdgeSDK.Server.Common;
using SocialEdgeSDK.Server.Models;
using MongoDB.Bson.Serialization.Attributes;
using System.Linq;

namespace SocialEdgeSDK.Server.Context
{
    public class PlayerEconomy
    {
        [BsonIgnore] private SocialEdgePlayerContext socialEdgePlayer;

        public PlayerEconomy(SocialEdgePlayerContext _socialEdgePlayer)
        {
            socialEdgePlayer = _socialEdgePlayer;
        }

        public static void HandlePiggyBankRewardPerPlayer(SocialEdgePlayerContext socialEdgePlayer)
        {
            // if ((!matchData.tournamentId || matchData.tournamentId === "") && !matchData.isEventMatch) {
            //     //Spark.getLog().debug("matchData: " + JSON.stringify(matchData));
            //     return;
            // }
            string playerId = socialEdgePlayer.PlayerId;
            long currentTime = Utils.UTCNow();
            Int32 piggyBankBalance = socialEdgePlayer.PlayerModel.Economy.piggyBankGems;
            long piggyBankExpiryTimestamp = socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp;
            Int32 playerLeague = socialEdgePlayer.PlayerModel.Info.league;

            dynamic commonSettings = Settings.CommonSettings;

            if (piggyBankExpiryTimestamp != 0 && currentTime >= piggyBankExpiryTimestamp)
            {
                socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = 0;
                socialEdgePlayer.PlayerModel.Economy.piggyBankGems = 0;
                piggyBankBalance = 0;
            }

            if (playerLeague >= commonSettings["piggyBankUnlocksAtLeague"] && piggyBankBalance < commonSettings["piggyBankMaxCap"])
            {
                Int32 piggyBankRewardPerGameSetting = (Int32)commonSettings["piggyBankRewardPerGame"];
                Int32 piggyBankRewardPerGame = piggyBankExpiryTimestamp > currentTime ? piggyBankRewardPerGameSetting * 2 : piggyBankRewardPerGameSetting;

                Int32 piggyBankMaxCap = (Int32)commonSettings["piggyBankMaxCap"];
                Int32 piggyLimitAvailable = piggyBankMaxCap - piggyBankBalance;
                Int32 piggyBankReward = piggyLimitAvailable >= piggyBankRewardPerGame ? piggyBankRewardPerGame : piggyLimitAvailable;

                socialEdgePlayer.PlayerModel.Economy.piggyBankGems = socialEdgePlayer.PlayerModel.Economy.piggyBankGems + piggyBankReward;
                //matchData.piggyBankReward = piggyBankReward;

                if (socialEdgePlayer.PlayerModel.Economy.piggyBankGems >= piggyBankMaxCap)
                {

                    long piggyBankExpiry = (int)commonSettings["piggyBankExpirationInDays"] * 24 * 60 * 60 * 1000;
                    socialEdgePlayer.PlayerModel.Economy.piggyBankExpiryTimestamp = currentTime + piggyBankExpiry;
                }
            }
        }
        public string ProcessDynamicDisplayBundle()
        {
            _setupDynamicBundleTier();

            if (socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier == null || socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier == "")
            {
                socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier = "A";
            }

            if (socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount > 0)
            {
                socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount = socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount + 1;

                if (socialEdgePlayer.PlayerModel.Economy.outOfGemsSessionCount > (int)Settings.CommonSettings["dynamicBundleSwitchAfterSessions"])
                {
                    socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier = "B";
                }
            }

            string tempItemId = Settings.DynamicDisplayBundles[socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier][socialEdgePlayer.PlayerModel.Economy.dynamicBundleDisplayTier].ToString();
            string shortCode = Utils.GetShortCode(tempItemId);
            return shortCode;
        }

        private void _setupDynamicBundleTier()
        {
            if (socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier == null || socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier == "")
            {
                socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier = "T1";

                foreach (var dataItem in Settings.DynamicPurchaseTiers)
                {
                    string tierValue = dataItem.Value.ToString();
                    string dynamicBundlePurchaseTier = socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier;
                    string itemId = Utils.AppendItemId(dataItem.Name.ToString());
                    if (HasItemIdInInventory(itemId) && Utils.compareTier(tierValue, dynamicBundlePurchaseTier) == 1)
                    {
                        socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier = tierValue;
                    }
                }
            }
        }

        public dynamic GetDynamicGemSpotBundle()
        {
            _setupDynamicBundleTier();
            dynamic dynamicBundlePurchaseTier = Settings.DynamicGemSpotBundles[socialEdgePlayer.PlayerModel.Economy.dynamicBundlePurchaseTier];
            // Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(dynamicBundlePurchaseTier.ToString());
            // dictionary["pack1"] = Utils.GetShortCode(dictionary["pack1"].ToString());;
            // dictionary["pack2"] = Utils.GetShortCode(dictionary["pack2"].ToString());;
            // dictionary["bundle"] = Utils.GetShortCode(dictionary["bundle"].ToString());;
            return dynamicBundlePurchaseTier;
        }

        public bool HasItemIdInInventory(string itemId)
        {
            ItemInstance object1 = socialEdgePlayer.Inventory.FirstOrDefault(i => i.ItemId == itemId);
            return object1 != null && object1.RemainingUses > 0;
        }

        public ItemInstance GetInventoryItem(SocialEdgePlayerContext socialEdgePlayer, string intanceId)
        {
            return socialEdgePlayer.Inventory.FirstOrDefault(i => i.ItemInstanceId == intanceId);
        }

        public void AddVirtualCurrency(string currencyType, int amount)
        {
            var taskT = Player.AddVirtualCurrency(socialEdgePlayer.PlayerId, amount, currencyType);
        }
        
        public void SubtractVirtualCurrency(string currencyType, int amount)
        {
            var taskT = Player.SubtractVirtualCurrency(socialEdgePlayer.PlayerId, amount, currencyType);
        }
    }
}