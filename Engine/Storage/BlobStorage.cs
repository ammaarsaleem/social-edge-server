/// @license Propriety <http://license.url>
/// @copyright Copyright (C) Everplay - All rights reserved
/// Unauthorized copying of this file, via any medium is strictly prohibited
/// Proprietary and confidential

using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;

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
        
        // Code Referece:
        // https://docs.microsoft.com/en-us/azure/storage/blobs/sas-service-create?tabs=dotnet
        //
        public Uri GetServiceSasUriForBlob(string fileName, int expireMins, string storedPolicyName = null)
        {
            var blobClient = _blobContainerClient.GetBlobClient(fileName);

            // Check whether this BlobClient object has been authorized with Shared Key.
            if (blobClient.CanGenerateSasUri)
            {
                // Create a SAS token that's valid for a limited time.
                BlobSasBuilder sasBuilder = new BlobSasBuilder()
                {
                    BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                    BlobName = blobClient.Name,
                    Resource = "b"
                };

                if (storedPolicyName == null)
                {
                    sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expireMins);
                    sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);
                }
                else
                {
                    sasBuilder.Identifier = storedPolicyName;
                }

                Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
                return sasUri;
            }
            else
            {
                Console.WriteLine(@"BlobClient must be authorized with Shared Key 
                                credentials to create a service SAS.");
                return null;
            }
        }        
    }
}