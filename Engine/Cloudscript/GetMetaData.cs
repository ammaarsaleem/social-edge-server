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
        public GetTitleDataResult titleData;
        public List<EntityProfileBody> friendsProfiles;
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

            SocialEdgePlayer.CacheFill(CachePlayerDataSegments.META);
            SocialEdgePlayer.PlayerModel.Prefetch(new List<string>(){PlayerModelFields.ECONOMY, PlayerModelFields.TOURNAMENT});
            Inbox.Validate(SocialEdgePlayer);
            Tournaments.UpdateTournaments(SocialEdgePlayer, SocialEdgeTournament);


            try
            {
                GetMetaDataResult result = new GetMetaDataResult();
                result.titleData = SocialEdge.TitleContext.TitleData;
                result.friends = SocialEdgePlayer.Friends;
                result.friendsProfiles = SocialEdgePlayer.FriendsProfiles;
                result.inbox = SocialEdgePlayer.Inbox;
                result.chat = SocialEdgePlayer.ChatJson;
                result.appVersionValid = true; // TODO
                result.inboxCount = InboxModel.Count(SocialEdgePlayer);
                result.liveTournaments = SocialEdgeTournament.TournamentLiveModel.Fetch();
                result.dynamicBundleToDisplay = SocialEdgePlayer.PlayerEconomy.ProcessDynamicDisplayBundle();
                result.dynamicGemSpotBundle = SocialEdgePlayer.PlayerEconomy.GetDynamicGemSpotBundle().ToString();
                result.contentData = SocialEdge.DataService.GetBlobStorage(Constants.Constant.CONTAINER_DLC)
                                                    .ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson})
                                                    .ToString();

                if (isNewlyCreated == true)
                {
                    result.playerCombinedInfoResultPayload = SocialEdgePlayer.CombinedInfo;
                }

                CacheFlush();
                // Force a fetch of player model after all data is written out so all fields of playermodel cache are filled.
                result.playerDataModel = SocialEdgePlayer.PlayerModel.Fetch();
                return result;
            }
            catch (Exception e)
            {
                throw e;
            }
        }    
    }  
}
