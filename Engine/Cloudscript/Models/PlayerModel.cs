/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using PlayFab.ServerModels;
using System.Collections.Generic;

namespace SocialEdgeSDK.Server.Models
{
    public class PlayerModelA
    {
        public GetPlayerCombinedInfoResultPayload combinedInfo;
        public Dictionary<string, PlayFab.ProfilesModels.EntityDataObject> customSettings;

    }
}