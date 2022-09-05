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
using PlayFab.Samples;
using SocialEdgeSDK.Server.Common;

namespace SocialEdgeSDK.Server.Requests
{
    public class PingResult
    {
        public long serverReceiptTimestamp;
        public long clientSendTimestamp;
        public long maintenanceWarningTimeStamp;
        public string maintenanceWarningMessage;
        public string maintenanceWarningBgColor;
        public bool maintenanceFlag;
        public bool maintenanceWarningFlag;
    }
        
    public class Ping : FunctionContext
    {
        public Ping(ITitleContext titleContext, IDataService dataService) { Base(titleContext, dataService); }

        [FunctionName("Ping")]
        public PingResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            // log.LogInformation("C# HTTP trigger function processed a request.");
            InitContext<FunctionExecutionContext<dynamic>>(req, log);
            var result = new PingResult();
            result.clientSendTimestamp = (long)Args["data"]["clientSendTimestamp"].Value;
            result.serverReceiptTimestamp = Utils.UTCNow();

            var commonSettings = Settings.CommonSettings;
            var maintenanceWarningTimeStamp = long.Parse(commonSettings["maintenanceWarningTimeStamp"].ToString());

            if(maintenanceWarningTimeStamp > Utils.UTCNow())
            {
                result.maintenanceWarningTimeStamp = maintenanceWarningTimeStamp;
                result.maintenanceWarningFlag = true;
                result.maintenanceWarningMessage = commonSettings["maintenanceWarningMessage"].ToString();
                result.maintenanceWarningBgColor = commonSettings["maintenanceWarningBgColor"].ToString();
            }
            else if(maintenanceWarningTimeStamp > 0)
            {
                result.maintenanceFlag = true;
            }

            return result;
        }
    }
}
