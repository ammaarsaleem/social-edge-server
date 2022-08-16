/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
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
        public string dynamicGemSpotBundle;
        public bool appVersionValid;
        public string contentData;
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
            BsonDocument args = BsonDocument.Parse(Args);
            var isNewlyCreated = args.Contains("isNewlyCreated") ? args["isNewlyCreated"].AsBoolean : false;
            
            if(isNewlyCreated)
            {
                Player.NewPlayerInit(SocialEdgePlayer);
            }
            else
            {
                SocialEdgePlayer.PlayerModel.Prefetch(PlayerModelFields.ALL);
            }

            SocialEdgePlayer.CacheFill(CachePlayerDataSegments.META);
            Inbox.Validate(SocialEdgePlayer);
            Tournaments.UpdateTournaments(SocialEdgePlayer, SocialEdgeTournament);
            SocialEdgePlayer.PlayerEconomy.ProcessEconomyInit();

            //Friends.AddFriend("C70A814270978695", "UNBLOCKED", "SOCIAL", SocialEdgePlayer.PlayerId);
            
            try
            {
                GetMetaDataResult result = new GetMetaDataResult();
                result.friends = SocialEdgePlayer.Friends;
                result.friendsProfiles = SocialEdgePlayer.FriendsProfiles;
                result.friendsProfilesEx = SocialEdgePlayer.FriendsProfilesEx;
                result.inbox = SocialEdgePlayer.Inbox;
                result.chat = SocialEdgePlayer.ChatJson;
                result.appVersionValid = true; // TODO
                result.inboxCount = InboxModel.Count(SocialEdgePlayer);
                result.liveTournaments = SocialEdgeTournament.TournamentLiveModel.Fetch();
                result.dynamicBundleToDisplay = SocialEdgePlayer.PlayerEconomy.ProcessDynamicDisplayBundle();
                result.dynamicGemSpotBundle = SocialEdgePlayer.PlayerEconomy.GetDynamicGemSpotBundle().ToString();
                result.contentData = SocialEdge.DataService.GetBlobStorage(Constants.Constant.CONTAINER_DLC)
                                                    .GetContentList()
                                                    .ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson})
                                                    .ToString();
                result.playerDataModel = SocialEdgePlayer.PlayerModel;

                if (isNewlyCreated == true)
                {
                    result.playerCombinedInfoResultPayload = SocialEdgePlayer.CombinedInfo;
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
