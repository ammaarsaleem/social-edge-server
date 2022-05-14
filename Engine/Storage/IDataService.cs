/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

namespace SocialEdgeSDK.Server.DataService
{
    public interface IDataService
    {
        ICollection GetCollection(string name);
        ICache GetCache();
    }
}