/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

#define USE_REDIS

using Azure.Storage.Blobs;
using MongoDB.Driver;

namespace SocialEdgeSDK.Server.DataService
{
    public interface IDataService
    {
        IMongoDatabase GetDatabase();
        ICollection<T> GetCollection<T>(string name);
        IBlobStorage GetBlobStorage(string containerName);
        BlobContainerClient GetContainerClient(string containerName);

#if USE_REDIS
        ICache GetCache();
#endif

    }
}