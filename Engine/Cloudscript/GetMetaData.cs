/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SocialEdgeSDK.Server.Context;
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Api;
using PlayFab.Samples;
using PlayFab.ServerModels;
using PlayFab.ProfilesModels;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class GetMetaDataResult
    {
        public PlayerDataModel playerDataModel;
        public GetPlayerCombinedInfoResultPayload playerCombinedInfoResultPayload;
        public string publicDataObjs;
        public Dictionary<string, TournamentLiveData> liveTournaments;
        public string chat;
        public int inboxCount;
        public Dictionary<string, InboxDataMessage> inbox;
        public List<FriendInfo> friends;
        public List<EntityProfileBody> friendsProfiles;
        public List<PublicProfileEx> friendsProfilesEx;
        public string dynamicBundleToDisplay;
        public Dictionary<string, string> dynamicGemSpotBundle;
        public bool appVersionValid;
        public List<BlobFileInfo> contentData;
        public int todayGamesCount;
        public int todayActivePlayersCount;
    }

    public class GetMetaData : FunctionContext
    {
        public GetMetaData(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("GetMetaData")]
        public  GetMetaDataResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var data = Args["data"];
            BsonDocument args = BsonDocument.Parse(data["parameters"].ToString());
            bool isNewlyCreated = SocialEdgePlayer.PublicData.isInitialized == false;
            var clientVersion = args["clientVersion"].ToString();
            int playerTimeZoneSlot = args["timeZone"].ToInt32();
            string deviceId = args["deviceId"].ToString();
            string fbId     = args["fbId"].ToString();
            string appleId  = args["appleId"].ToString();
            bool isResume = args["isResume"].ToBoolean();

            if (isNewlyCreated)
            {
                Player.NewPlayerInit(SocialEdgePlayer, SocialEdgeTournament, deviceId, fbId, appleId);
                SocialEdgePlayer.CombinedInfo.PlayerProfile.DisplayName = SocialEdgePlayer.DisplayName;
            }
            else
            {
                SocialEdgePlayer.PlayerModel.Prefetch(PlayerModelFields.ALL);
            }

            Inbox.Validate(SocialEdgePlayer);
            SocialEdgePlayer.PlayerModel.Tournament.playerTimeZoneSlot = playerTimeZoneSlot;
            Tournaments.UpdateTournaments(SocialEdgePlayer, SocialEdgeTournament);
            SocialEdgePlayer.PlayerEconomy.ProcessEconomyInit();
            PlayerSearch.Register(SocialEdgePlayer);
            Challenge.ProcessAbandonedGame(SocialEdgePlayer, SocialEdgeChallenge, SocialEdgeTournament, this);
            Friends.SyncFriendsList(SocialEdgePlayer);
            SocialEdgePlayer.PlayerModel.Meta.clientVersion = clientVersion;

            try
            {
                GetMetaDataResult result = new GetMetaDataResult();
                result.friends = SocialEdgePlayer.Friends;
                result.friendsProfilesEx = SocialEdgePlayer.FriendsProfilesEx;
                result.inbox = SocialEdgePlayer.Inbox;
                result.appVersionValid = Utils.CompareVersions(Settings.MetaSettings["minimumClientVersion"].ToString(), clientVersion);
                result.inboxCount = InboxModel.Count(SocialEdgePlayer);
                result.liveTournaments = SocialEdgeTournament.TournamentLiveModel.Fetch();
                result.dynamicBundleToDisplay = SocialEdgePlayer.PlayerEconomy.ProcessDynamicDisplayBundle();
                result.dynamicGemSpotBundle = SocialEdgePlayer.PlayerEconomy.GetDynamicGemSpotBundle();
                result.contentData = SocialEdge.DataService.GetBlobStorage(Constants.Constant.CONTAINER_DLC).GetContentList();
                result.playerDataModel = SocialEdgePlayer.PlayerModel;
                result.todayGamesCount = SocialEdge.GetTodayGamesCount();
                result.todayActivePlayersCount = SocialEdge.GetTodayActivePlayersCount();

                if (isNewlyCreated == true || isResume == true)
                {
                    result.playerCombinedInfoResultPayload = SocialEdgePlayer.CombinedInfo;
                }

                if (isNewlyCreated == true)
                {
                    result.playerCombinedInfoResultPayload.PlayerProfile.AvatarUrl = SocialEdgePlayer.MiniProfile.ToJson();
                    result.playerCombinedInfoResultPayload.PlayerProfile.DisplayName = SocialEdgePlayer.DisplayName;
                    SocialEdgePlayer.PublicData.isInitialized = true;
                }

                CacheFlush();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }  
}
