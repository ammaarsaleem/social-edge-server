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
            log.LogInformation("I am here 1");

            InitContext<FunctionExecutionContext<dynamic>>(req, log);

            log.LogInformation("I am here 2");

            //SocialEdgePlayer.CacheFill(CacheSegment.META);

            log.LogInformation("I am here new 3 . ." + SocialEdgePlayer.AvatarInfo.ToString());

            BsonDocument args = BsonDocument.Parse(Args);
            var isNewlyCreated = args.Contains("isNewlyCreated") ? args["isNewlyCreated"].AsBoolean : false;
            log.LogInformation("I am here 4");


            try
            {
                Inbox.Validate(SocialEdgePlayer);

                log.LogInformation("I am here 6");

                // Prepare client response
                BsonDocument liveTournamentsT = await SocialEdge.DataService.GetCollection("liveTournaments").FindOneById("62b435e786859fe679e7b946");
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                metaDataResponse.shop = new GetShopResult();
                metaDataResponse.shop.catalogResult = SocialEdge.TitleContext.CatalogItems;
                metaDataResponse.shop.storeResult = SocialEdge.TitleContext.StoreItems;
                metaDataResponse.titleData = SocialEdge.TitleContext.TitleData;
                log.LogInformation("I am here 7 . . . . " + metaDataResponse.ToString());

                metaDataResponse.friends = SocialEdgePlayer.Friends;
                metaDataResponse.friendsProfiles = SocialEdgePlayer.FriendsProfiles;
               // metaDataResponse.publicDataObjs = SocialEdgePlayer.PublicDataObjsJson;
                // metaDataResponse.inbox = SocialEdgePlayer.InboxJson;
                // metaDataResponse.chat = SocialEdgePlayer.ChatJson;
                 metaDataResponse.appVersionValid = true; // TODO
                // metaDataResponse.inboxCount = InboxModel.Count(SocialEdgePlayer);
                metaDataResponse.contentData = GetContentList();

                log.LogInformation("I am here 8");


                if (isNewlyCreated == true)
                {
                    metaDataResponse.playerCombinedInfoResultPayload = SocialEdgePlayer.CombinedInfo;
                }

                log.LogInformation("I am here 9");

                // TODO
                var liveTournamentsJson = liveTournamentsT["tournament"].ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                List<string> liveTournamentsList = new List<string>();
                liveTournamentsList.Add(liveTournamentsJson);
                var liveTournamentsListJson = liveTournamentsList.ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.RelaxedExtendedJson});
                metaDataResponse.liveTournaments = liveTournamentsListJson.ToString();

                log.LogInformation("I am here 10");


                SocialEdgePlayer.CacheFlush();
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
