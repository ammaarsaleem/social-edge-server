/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

namespace SocialEdgeSDK.Server.Common.Models{
    public class Result
    {
        public bool isSuccess = false;
        public string error = string.Empty;
        public Result(bool success, string err)
        {
            isSuccess = success;
            error = err;
        }
    }
}