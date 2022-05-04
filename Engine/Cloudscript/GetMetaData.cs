using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using PlayFab.Samples;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Models;
using SocialEdge.Server.DataService;
using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace SocialEdge.Server.Requests
{
    public class GetMetaData
    {
        ITitleContext _titleContext;
        IDataService _dataService;

        public GetMetaData(ITitleContext titleContext, IDataService dataService)
        {
            _titleContext = titleContext;
            _dataService = dataService;
        }

        /// </summary>
        /// <param name="playerId">the playerId of the user to fetch data</param>
        /// <returns></returns>
        [FunctionName("GetMetaData")]
        public async Task<GetMetaDataResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            SocialEdgeEnvironment.Init(req, log, _titleContext, _dataService);
            var context = Newtonsoft.Json.JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            PlayerContext playerContext = new PlayerContext(context);
            var maskT = await playerContext.ValidateCache(FetchBits.ALL);

            try
            {
                BsonDocument liveTournamentsT = await _dataService.GetCollection("liveTournaments").FindOneById("625feb0f0cf3edd2a788b4be");
                // Prepare client response
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.shop.catalogResult = _titleContext.CatalogItems;
                metaDataResponse.shop.storeResult = _titleContext.StoreItems;
                metaDataResponse.titleData = _titleContext.TitleData;
                metaDataResponse.friendsProfiles = playerContext.FriendsProfiles;
                metaDataResponse.friends = playerContext.Friends;
                metaDataResponse.dataObjects = playerContext.PublicDataJson;
                metaDataResponse.inbox = playerContext.InboxJson;
                metaDataResponse.chat = playerContext.ChatJson;
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
