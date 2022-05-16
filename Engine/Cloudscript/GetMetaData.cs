/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
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
using PlayFab.Samples;

namespace SocialEdgeSDK.Server.Requests
{
    public class GetMetaData : FunctionContext
    {
        public GetMetaData(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("GetMetaData")]
        public async Task<GetMetaDataResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            SocialEdgePlayer.CacheFill(CacheSegment.META);
            BsonDocument args = BsonDocument.Parse(Args);
            var isNewlyCreated = args.Contains("isNewlyCreated") ? args["isNewlyCreated"].AsBoolean : false;

            try
            {
                // Prepare client response
                BsonDocument liveTournamentsT = await SocialEdge.DataService.GetCollection("liveTournaments").FindOneById("625feb0f0cf3edd2a788b4be");
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.shop.catalogResult = SocialEdge.TitleContext.CatalogItems;
                metaDataResponse.shop.storeResult = SocialEdge.TitleContext.StoreItems;
                metaDataResponse.titleData = SocialEdge.TitleContext.TitleData;
                metaDataResponse.friends = SocialEdgePlayer.Friends;
                metaDataResponse.friendsProfiles = SocialEdgePlayer.FriendsProfiles;
                metaDataResponse.publicDataObjs = SocialEdgePlayer.PublicDataObjsJson;
                metaDataResponse.inbox = SocialEdgePlayer.InboxJson;
                metaDataResponse.chat = SocialEdgePlayer.ChatJson;
                metaDataResponse.appVersionValid = true; // TODO
                metaDataResponse.inboxCount = InboxModel.Count(SocialEdgePlayer);

                if (isNewlyCreated == true)
                {
                    metaDataResponse.playerCombinedInfoResultPayload = SocialEdgePlayer.CombinedInfo;
                }

                // TODO
                var liveTournamentsJson = liveTournamentsT["tournament"].ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                List<string> liveTournamentsList = new List<string>();
                liveTournamentsList.Add(liveTournamentsJson);
                var liveTournamentsListJson = liveTournamentsList.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                metaDataResponse.liveTournaments = liveTournamentsListJson.ToString();

                SocialEdgePlayer.CacheFlush();
                return metaDataResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }  
}
