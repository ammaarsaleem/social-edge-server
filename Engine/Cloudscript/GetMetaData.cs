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
        public  GetMetaDataResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            BsonDocument args = BsonDocument.Parse(Args);
            var isNewlyCreated = args.Contains("isNewlyCreated") ? args["isNewlyCreated"].AsBoolean : false;

            //Inbox.Validate(SocialEdgePlayer);
            //InboxModel.Init(SocialEdgePlayer.InboxId);
            // TEST : CREATE NEW PLAYER
            //Player.NewPlayerInit(SocialEdgePlayer);
            //SocialEdgePlayer.CacheFlush();
            //return new GetMetaDataResult();
            // TEST : CREATE NEW PLAYER

            SocialEdgePlayer.CacheFill(CachePlayerDataSegments.META);
            Inbox.Validate(SocialEdgePlayer);
            Tournaments.UpdateTournaments(SocialEdgePlayer, SocialEdgeTournament);

             //TEST Piggybank
            //string data1 = Player.GetDynamicDisplayBundle(SocialEdgePlayer);
            //dynamic data2 =  Player.GetDynamicGemSpotBundle(SocialEdgePlayer);

            try
            {
                // Prepare client response
                GetMetaDataResult metaDataResponse = new GetMetaDataResult();
                //metaDataResponse.shop = new GetShopResult();
                //metaDataResponse.shop.catalogResult = SocialEdge.TitleContext.CatalogItems;
                //metaDataResponse.shop.storeResult = SocialEdge.TitleContext.StoreItems;
                metaDataResponse.titleData = SocialEdge.TitleContext.TitleData;
                metaDataResponse.friends = SocialEdgePlayer.Friends;
                metaDataResponse.friendsProfiles = SocialEdgePlayer.FriendsProfiles;
                //metaDataResponse.publicDataObjs = SocialEdgePlayer.PublicDataObjsJson;
                metaDataResponse.inbox = SocialEdgePlayer.Inbox;
                metaDataResponse.chat = SocialEdgePlayer.ChatJson;
                metaDataResponse.appVersionValid = true; // TODO
                metaDataResponse.inboxCount = InboxModel.Count(SocialEdgePlayer);
                metaDataResponse.contentData = GetContentList();
                metaDataResponse.liveTournaments = SocialEdgeTournament.TournamentLiveModel.Fetch();
                metaDataResponse.dynamicBundleToDisplay = SocialEdgePlayer.PlayerEconomy.ProcessDynamicDisplayBundle();
                metaDataResponse.dynamicGemSpotBundle = SocialEdgePlayer.PlayerEconomy.GetDynamicGemSpotBundle().ToString();

                if (isNewlyCreated == true)
                {
                    metaDataResponse.playerCombinedInfoResultPayload = SocialEdgePlayer.CombinedInfo;
                }

                CacheFlush();
                // Force a fetch of player model after all data is written out so all fields of playermodel cache are filled.
                metaDataResponse.playerDataModel = SocialEdgePlayer.PlayerModel.Fetch();
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
