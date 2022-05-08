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
using SocialEdgeSDK.Server.Models;
using SocialEdgeSDK.Server.DataService;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SocialEdgeSDK.Server.Context;

namespace SocialEdgeSDK.Server.Requests
{
    public class GetMetaData  : FunctionContext
    {
        public GetMetaData(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("GetMetaData")]
        public async Task<GetMetaDataResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext(req, log);
            SocialEdgePlayer.ValidateCache(FetchBits.META);

            //await Transactions.Grant(new Dictionary<string, int>(){{"SkinSlate", 1}}, FnPlayerContext);
//            Inbox.Collect("ed94-7995-4925-90b8", FnPlayerContext);
            //InboxModel.Count(FnPlayerContext.Inbox);
            //int qty = await Transactions.GrantTrophies(1, FnPlayerContext); 
            //Inbox.Collect("ed94-7995-4925-90b8", FnPlayerContext);
            //var coins = SocialEdgePlayer.VirtualCurrency["CN"];
            //var gems = SocialEdgePlayer.VirtualCurrency["GM"];

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
                metaDataResponse.inboxCount = 2; // TODO

                // TODO
                var liveTournamentsJson = liveTournamentsT["tournament"].ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                List<string> liveTournamentsList = new List<string>();
                liveTournamentsList.Add(liveTournamentsJson);
                var liveTournamentsListJson = liveTournamentsList.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                metaDataResponse.liveTournaments = liveTournamentsListJson.ToString();




                return metaDataResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }  
}
