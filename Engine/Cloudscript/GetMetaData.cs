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
using SocialEdgeSDK.Server.Api;
using PlayFab.Samples;
using Azure.Storage.Blobs;

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
            SocialEdgePlayer.CacheFill(CachePlayerDataSegments.META);
            BsonDocument args = BsonDocument.Parse(Args);
            var isNewlyCreated = args.Contains("isNewlyCreated") ? args["isNewlyCreated"].AsBoolean : false;
            Inbox.Validate(SocialEdgePlayer);

            //Player.NewPlayerInit(SocialEdgePlayer);
            //SocialEdgePlayer.CacheFlush();
            //SocialEdgeTournament.CacheFlush(); 
            //GetMetaDataResult metaDataResponse1 = new GetMetaDataResult();    
            //return metaDataResponse1;


            try
            {
                // Prepare client response
                BsonDocument liveTournamentsT = await SocialEdge.DataService.GetCollection<BsonDocument>("liveTournaments").FindOneById("62b435e786859fe679e7b946");
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.shop.catalogResult = SocialEdge.TitleContext.CatalogItems;
                metaDataResponse.shop.storeResult = SocialEdge.TitleContext.StoreItems;
                metaDataResponse.titleData = SocialEdge.TitleContext.TitleData;
                metaDataResponse.friends = SocialEdgePlayer.Friends;
                metaDataResponse.friendsProfiles = SocialEdgePlayer.FriendsProfiles;
                metaDataResponse.publicDataObjs = SocialEdgePlayer.PublicDataObjsJson;
                metaDataResponse.playerDataModel = SocialEdgePlayer.PlayerModel.Fetch();
                metaDataResponse.inbox = SocialEdgePlayer.InboxJson;
                metaDataResponse.chat = SocialEdgePlayer.ChatJson;
                metaDataResponse.appVersionValid = true; // TODO
                metaDataResponse.inboxCount = InboxModel.Count(SocialEdgePlayer);
                metaDataResponse.contentData = GetContentList();

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
                SocialEdgeTournament.CacheFlush();
                
                return metaDataResponse;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string GetContentList()
        {
            string result = null;
            BlobContainerClient containerClient = SocialEdge.DataService.GetContainerClient(Constants.Constant.CONTAINER_DLC);
            var blobs = containerClient.GetBlobs();

            if(blobs != null)
            {
                Dictionary<string, ContentResult> data = new Dictionary<string, ContentResult>();
                
                foreach (var item in blobs)
                {
                    ContentResult dataItem =  new ContentResult(); 
                    dataItem.shortCode = item.Name;
                    dataItem.size = item.Properties.ContentLength.Value;
                    dataItem.modifiedOn = item.Properties.LastModified.Value.ToUnixTimeMilliseconds();                    

                    if(!data.ContainsKey(item.Name)){
                        data.Add(item.Name, dataItem);
                    }
                }

                var blobsListJson = data.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                result = blobsListJson.ToString();
            }

            SocialEdge.Log.LogInformation("GetContentList RESULT : " + result);
            return result;
        }       
    }  
}
