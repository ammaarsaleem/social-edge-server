/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using PlayFab.Samples;
using PlayFab.ServerModels;
using SocialEdgeSDK.Server.DataService;
using SocialEdgeSDK.Server.Context;
using MongoDB.Bson;

namespace SocialEdgeSDK.Server.Requests
{
    public class AppAdminOpResult
    {
        public bool status;
        public string statusMsg;
    }

    public class AppAdminOp : FunctionContext
    {
        public AppAdminOp(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("AppAdminOp")]
        public async Task<AppAdminOpResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestMessage req, ILogger log)
        {
            // Expected Input Parameters in format:
            // {"FunctionArgument" : { "op" : "<operation>"}}

            InitContext(req, log);
            AppAdminOpResult result = new AppAdminOpResult();

            // Bail if 'op' not defined
            string op = Args.ContainsKey("op") ? Args["op"].ToString() : null;
            if (op == null)
                return result; 

            
            if (op == "updateSettings")
            {
                result.status = SocialEdge.TitleContext.AdminFetchBackBufferAndSwap();
            }
            else if (op == "updateMaintenanceMode")
            {
                 Dictionary<string, string> titleData = SocialEdge.TitleContext.TitleData.Data;
                 var testing = titleData["Testing"];
                 SocialEdge.Log.LogInformation("testing RESULT : " + testing.ToJson());
            }

            SocialEdge.Log.LogInformation("AppAdminOp comleted!");

            return result;
        }
    }
}
