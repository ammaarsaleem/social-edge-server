/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialEdgeSDK.Server.DataService
{
    public interface IBlobStorage
    {
        Task<bool> Save(string fileName, byte[] stream);
        Uri GetServiceSasUriForBlob(string fileName, int expireMins, string storedPolicyName = null);
        List<BlobFileInfo> GetContentList();
    }
}