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

namespace SocialEdge.Server.Requests
{
    public class GetMetaData
    {
        IDbHelper _dbHelper;
        ITitleContext _titleContext;

        IDataService _dataService;

        public GetMetaData(IDbHelper dbHelper, ITitleContext titleContext, IDataService dataService)
        {
            _dbHelper = dbHelper;
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
            SocialEdgeEnvironment.Init(req);
            var context = JsonConvert.DeserializeObject<FunctionExecutionContext<dynamic>>(await req.Content.ReadAsStringAsync());
            dynamic args = context.FunctionArgument;
            string playerId = context.CallerEntityProfile.Lineage.MasterPlayerAccountId;

            var objs = context.CallerEntityProfile.Objects;
            var DBIds = objs["DBIds"];
            var dbIdsDict = JsonConvert.DeserializeObject<dynamic>(DBIds.EscapedDataObject);

            try
            {
                // Fetch Inbox
                BsonDocument inboxT = await GetCollectionDoc(dbIdsDict["inbox"].Value.ToString(), "inbox");
                // Fetch Chat
                BsonDocument chatT = await GetCollectionDoc(dbIdsDict["chat"].Value.ToString(), "chat");
                // Fetch Live Tournaments
                BsonDocument liveTournamentsT = await GetCollectionDoc("625feb0f0cf3edd2a788b4be", "liveTournaments");

                // Fetch friends
                var getTitleTokenT = await Player.GetTitleEntityToken();
                var getFriendsT = await Player.GetFriendsList(playerId);
                var friends = getFriendsT.Result.Friends;
                var getFriendProfilesT = await Player.GetFriendProfiles(friends, getTitleTokenT.Result.EntityToken);

                PlayFab.DataModels.GetObjectsRequest getObjectsReq = new PlayFab.DataModels.GetObjectsRequest();
                getObjectsReq.Entity = new PlayFab.DataModels.EntityKey();
                getObjectsReq.AuthenticationContext = new PlayFabAuthenticationContext();
                getObjectsReq.AuthenticationContext.EntityToken = getTitleTokenT.Result.EntityToken;
                getObjectsReq.Entity.Id = context.CallerEntityProfile.Entity.Id;
                getObjectsReq.Entity.Type = "title_player_account";
                var getObjectsResT = await PlayFabDataAPI.GetObjectsAsync(getObjectsReq);


                // Prepare client response
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.friends = new GetFriendsListResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.titleData = new GetTitleDataResult();
                metaDataResponse.dataObjects = new PlayFab.DataModels.GetObjectsResponse();
                metaDataResponse.friends = getFriendsT.Result;
                metaDataResponse.shop.catalogResult = _titleContext.CatalogItems;
                metaDataResponse.shop.storeResult = _titleContext.StoreItems;
                metaDataResponse.titleData = _titleContext.TitleData;
                metaDataResponse.friendsProfiles = getFriendProfilesT.Result;
                metaDataResponse.dataObjects = getObjectsResT.Result;

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

                var gameSettings = _titleContext.GetTitleDataProperty("GameSettings");
                var metaSettings = _titleContext.GetTitleDataProperty("Meta", gameSettings);
                int c = _titleContext.GetTitleDataProperty("backendAppVersion", metaSettings);

                return metaDataResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<BsonDocument> GetCollectionDoc(string docId, string collectionName)
        {
            ICollection collection = _dataService.GetCollection(collectionName);
            var result = await collection.FindOneById(docId);
            return result;
        }
    }  
}
