/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SocialEdgeSDK.Server.Db
{
    public interface IDbHelper
    {
        Task<bool> RegisterPlayer(string playFabId, string name, DateTime loginTime);
        Task<Dictionary<string,object>> SearchPlayerByName(string name);
    }
}