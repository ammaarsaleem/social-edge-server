using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using PlayFab.Samples;
using PlayFab;
using SocialEdge.Server.Common.Utils;
using SocialEdge.Server.Models;
using SocialEdge.Server.Constants;
using SocialEdge.Server.Db;
using SocialEdge.Server.Api;
using SocialEdge.Server.DataService;
using PlayFab.ProfilesModels;
using MongoDB.Driver;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

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
            SocialEdgeEnvironment.Init(req, _titleContext, _dataService);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            var objs = context.CallerEntityProfile.Objects;
            var DBIds = objs["DBIds"];
            var dbIdsDict = JObject.Parse(DBIds.EscapedDataObject);

            try
            {
                // Initiate fetch title token and friends list tasks
                var getTitleTokenT = await Player.GetTitleEntityToken();
                var getFriendsT = await Player.GetFriendsList(playerId);

                BsonDocument inboxT = await _dataService.GetCollection("inbox").FindOneById(dbIdsDict["inbox"].ToString());
                BsonDocument chatT = await _dataService.GetCollection("chat").FindOneById(dbIdsDict["chat"].ToString());
                BsonDocument liveTournamentsT = await _dataService.GetCollection("liveTournaments").FindOneById("625feb0f0cf3edd2a788b4be");

                // Initiate fetch friend profiles and public data tasks
                var friends = getFriendsT.Result.Friends;
                var getFriendProfilesT = await Player.GetFriendProfiles(friends, getTitleTokenT.Result.EntityToken);
                var getObjectsResT = await Player.GetPublicData(getTitleTokenT.Result.EntityToken, context.CallerEntityProfile.Entity.Id);

                // Prepare client response
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.friends = new GetFriendsListResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.titleData = new GetTitleDataResult();
                metaDataResponse.dataObjects = new PlayFab.DataModels.GetObjectsResponse();
                metaDataResponse.shop.catalogResult = _titleContext.CatalogItems;
                metaDataResponse.shop.storeResult = _titleContext.StoreItems;
                metaDataResponse.titleData = _titleContext.TitleData;

                // Process task dependendies last
                metaDataResponse.friendsProfiles = getFriendProfilesT.Result;
                metaDataResponse.dataObjects = getObjectsResT.Result;
                metaDataResponse.friends = getFriendsT.Result;
                var inboxJson = inboxT["inboxData"].ToJson();
                var chatJson = chatT["ChatData"].ToJson();
                List<string> liveTournamentsList = new List<string>();
                var liveTournamentsJson = liveTournamentsT["tournament"].ToJson();
                liveTournamentsList.Add(liveTournamentsJson);
                var liveTournamentsListJson = liveTournamentsList.ToJson();

                metaDataResponse.appVersionValid = true; // TODO
                metaDataResponse.inboxCount = 2; // TODO
                metaDataResponse.inbox = inboxJson;
                metaDataResponse.chat = chatJson;
                metaDataResponse.liveTournaments = liveTournamentsListJson;

                return metaDataResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }  
}
