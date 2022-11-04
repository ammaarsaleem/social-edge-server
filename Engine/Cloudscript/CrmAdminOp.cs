using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;
using MongoDB.Bson;
using SocialEdgeSDK.Server.Models;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class CrmAdminOp : FunctionContext
    {
        public CrmAdminOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("CrmAdminOp")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            InitContext(req, log);
            string userId    = Args["userId"];
            string userTag   = Args["userTag"];
            string requestId = Args["requestId"];

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("code", 1);
            data.Add("error", "invalid requestId : " + requestId);

            if(requestId == "searchPlayer")
            {
                data.Clear();
                data =  CommonModel.CRM_SearchPlayer(userId, userTag);
            }
            else if(requestId == "updatePlayer" && Args.ContainsKey("jsonData"))
            {
                var jsonData = Args["jsonData"];

                if(jsonData["gems"] != null)
                {
                    int amount = (int)jsonData["gems"];
                    data.Clear();
                    data = CommonModel.CRM_UpdatePlayer(userId, amount, "GM");
                }
                else if(jsonData["coins"] != null)
                {
                    int amount = (int)jsonData["coins"];
                    data.Clear();
                    data = CommonModel.CRM_UpdatePlayer(userId, amount, "CN");
                }
            }
            else if(requestId == "getTitle")
            {
                data.Clear();
                data.Add("CatalogItem", SocialEdge.TitleContext.GetCatalogItemDictionary());
                data.Add("TitleData", SocialEdge.TitleContext.GetTitleDataDict());

            }

            data.Add("request", Args);
            
            return new OkObjectResult(data);
        }
    }
}
