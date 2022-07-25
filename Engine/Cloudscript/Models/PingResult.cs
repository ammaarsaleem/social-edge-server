/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

namespace SocialEdgeSDK.Server.Models{
    public class PingResult
    {
        public long serverReceiptTimestamp;
        public long clientSendTimestamp;
    }

    public class ContentResult
    {
        public string shortCode;
        public long size;
        public long modifiedOn;
    }

     public class RemotePurchaseResult
    {
        public int responseCode = 0;
        public string responseMessage = "Success";
        public string itemId;
        public bool isAdded;
        public long removeAdsTimeStamp;
    }
}