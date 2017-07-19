// ==========================================================================
//  AzureBlobAssetStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Squidex.Infrastructure.Assets;

namespace Squidex.Infrastructure.AzureStorage
{
    public class AzureBlobAssetStore : IAssetStore, IExternalSystem
    {
        private readonly IBlobContainerProvider blobContainerProvider;
        private readonly string containerName;
        private CloudBlobContainer blobContainer;
        private const string AssetVersion = "AssetVersion";
        private const string AssetId = "AssetId";

        public AzureBlobAssetStore(IBlobContainerProvider blobContainerProvider, string containerName)
        {
            Guard.NotNullOrEmpty(containerName, nameof(containerName));
            Guard.NotNull(blobContainerProvider, nameof(blobContainerProvider));

            this.blobContainerProvider = blobContainerProvider;
            this.containerName = containerName;
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream)
        {
            var blobName = GetObjectName(id, suffix);
            var blob = blobContainer.GetBlockBlobReference(blobName);

            if (!await blob.ExistsAsync())
                return;

            // look for the requested version
            // first check if the original blob has the requested version
            if (blob.Metadata.TryGetValue(AssetVersion, out string verionStr))
            {
                if (long.TryParse(verionStr, out long blobVersion))
                {
                    // if not, then look for that snapshot which has the requested version number.
                    if (blobVersion != version)
                    {
                        var snapshotBlob = await FindSnapshotAsync(id, version);
                        blob = snapshotBlob ?? blob;
                    }
                }
            }

            await blob.DownloadToStreamAsync(stream);
        }

        public async Task UploadAsync(string id, long version, string suffix, Stream stream)
        {
            var blobName = GetObjectName(id, suffix);
            var blob = blobContainer.GetBlockBlobReference(blobName);

            if (await blob.ExistsAsync())
            {
                // if it's already exist create a snapshot, and we overwrite the source blob of the snapshot.
                // NOTE: not sure if there is an
                await CreateVersioningSnapshotAsync(blob, id, version);
            }

            if (!blob.Metadata.ContainsKey(AssetVersion))
                blob.Metadata.Add(AssetVersion, version.ToString());
            else
                blob.Metadata[AssetVersion] = version.ToString();

            blob.Metadata[AssetId] = id;
            
            await blob.UploadFromStreamAsync(stream);
            await blob.SetMetadataAsync();
        }

        public async void Connect()
        {
            blobContainer = await blobContainerProvider.GetContainerAsync(containerName);
        }

        private string GetObjectName(string id, string suffix)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (blobContainer == null)
            {
                throw new InvalidOperationException("No connection established yet.");
            }

            var name = $"{id}";

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                name += "_" + suffix;
            }

            return name;
        }

        private async Task CreateVersioningSnapshotAsync(CloudBlockBlob blob, string id, long version)
        {
            var metadata = new Dictionary<string, string>();
            metadata.Add(AssetVersion, version.ToString());
            metadata.Add(AssetId, id);
            await blob.CreateSnapshotAsync(metadata,
                null, null, null);
        }

        private async Task<CloudBlockBlob> FindSnapshotAsync(string id, long requestedVersion)
        {
            CloudBlockBlob resultBlob = null;
            BlobContinuationToken token = null;
            do
            {
                var listingResult = await blobContainer.ListBlobsSegmentedAsync(null, true,
                    BlobListingDetails.Snapshots, 10, token, null, null);
                token = listingResult.ContinuationToken;

                foreach (CloudBlob snapshotBlob in listingResult.Results.Cast<CloudBlob>())
                {
                    if (snapshotBlob.Metadata.TryGetValue(AssetVersion, out string snapshotVersionStr)
                        && snapshotBlob.Metadata.TryGetValue(AssetId, out string snapshotAssetId)
                        && long.TryParse(snapshotVersionStr, out long snapshotVersion)
                        && snapshotVersion == requestedVersion
                        && snapshotAssetId == id)
                    {
                        resultBlob = snapshotBlob as CloudBlockBlob;
                        break;
                    }
                }
            }
            while (token != null);

            return resultBlob;
        }
    }
}
