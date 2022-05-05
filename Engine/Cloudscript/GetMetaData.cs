using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using SocialEdge.Server.Models;
using SocialEdge.Server.DataService;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using SocialEdge.Server.Common.Utils;

namespace SocialEdge.Server.Requests
{
    public class GetMetaData  : FunctionContext
    {
        public GetMetaData(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("GetMetaData")]
        public async Task<GetMetaDataResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            await FunctionContextInit(req, log);
            await FnPlayerContext.ValidateCache(FetchBits.ALL);

            try
            {
                // Prepare client response
                BsonDocument liveTournamentsT = await SocialEdgeEnvironment.DataService.GetCollection("liveTournaments").FindOneById("625feb0f0cf3edd2a788b4be");
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.shop.catalogResult = SocialEdgeEnvironment.TitleContext.CatalogItems;
                metaDataResponse.shop.storeResult = SocialEdgeEnvironment.TitleContext.StoreItems;
                metaDataResponse.titleData = SocialEdgeEnvironment.TitleContext.TitleData;
                metaDataResponse.friends = FnPlayerContext.Friends;
                metaDataResponse.friendsProfiles = FnPlayerContext.FriendsProfiles;
                metaDataResponse.dataObjects = FnPlayerContext.PublicDataJson;
                metaDataResponse.inbox = FnPlayerContext.InboxJson;
                metaDataResponse.chat = FnPlayerContext.ChatJson;
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
