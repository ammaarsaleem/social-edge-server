/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace SocialEdgeSDK.Server.DataService
{
    public class BlobStorage : IBlobStorage
    {
        private readonly BlobContainerClient _blobContainerClient;

        public BlobStorage(BlobContainerClient blobContainerClient)
        {
            _blobContainerClient = blobContainerClient;
        }

        public async Task<bool> Save(string fileName, byte[] stream)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            var binaryData = new BinaryData(stream);
            var T = await blobClient.UploadAsync(binaryData, true);
            return T.Value != null;
        }
    }
}