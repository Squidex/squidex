// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Squidex.Infrastructure.Assets
{
    public class AzureBlobAssetStore : IAssetStore, IInitializable
    {
        private const string AssetVersion = "AssetVersion";
        private const string AssetId = "AssetId";
        private readonly string containerName;
        private readonly string connectionString;
        private CloudBlobContainer blobContainer;

        public AzureBlobAssetStore(string connectionString, string containerName)
        {
            Guard.NotNullOrEmpty(containerName, nameof(containerName));
            Guard.NotNullOrEmpty(connectionString, nameof(connectionString));

            this.connectionString = connectionString;
            this.containerName = containerName;
        }

        public void Initialize()
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);

                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobReference = blobClient.GetContainerReference(containerName);

                blobReference.CreateIfNotExistsAsync().Wait();

                blobContainer = blobReference;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Cannot connect to blob container '{containerName}'.", ex);
            }
        }

        public string GenerateSourceUrl(string id, long version, string suffix)
        {
            var blobName = GetObjectName(id, version, suffix);

            return new Uri(blobContainer.StorageUri.PrimaryUri, $"/{containerName}/{blobName}").ToString();
        }

        public async Task CopyAsync(string name, string id, long version, string suffix, CancellationToken ct = default(CancellationToken))
        {
            var blobName = GetObjectName(id, version, suffix);
            var blobRef = blobContainer.GetBlobReference(blobName);

            var tempBlob = blobContainer.GetBlockBlobReference(name);

            try
            {
                await blobRef.StartCopyAsync(tempBlob.Uri, null, null, null, null, ct);

                while (blobRef.CopyState.Status == CopyStatus.Pending)
                {
                    ct.ThrowIfCancellationRequested();

                    await Task.Delay(50);
                    await blobRef.FetchAttributesAsync(null, null, null, ct);
                }

                if (blobRef.CopyState.Status != CopyStatus.Success)
                {
                    throw new StorageException($"Copy of temporary file failed: {blobRef.CopyState.Status}");
                }
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                throw new AssetNotFoundException($"Asset {name} not found.", ex);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            var blobName = GetObjectName(id, version, suffix);
            var blobRef = blobContainer.GetBlockBlobReference(blobName);

            try
            {
                await blobRef.DownloadToStreamAsync(stream, null, null, null, ct);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                throw new AssetNotFoundException($"Asset {id}, {version} not found.", ex);
            }
        }

        public async Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            var blobName = GetObjectName(id, version, suffix);
            var blobRef = blobContainer.GetBlockBlobReference(blobName);

            blobRef.Metadata[AssetVersion] = version.ToString();
            blobRef.Metadata[AssetId] = id;

            await blobRef.UploadFromStreamAsync(stream, null, null, null, ct);
            await blobRef.SetMetadataAsync();
        }

        public Task UploadAsync(string name, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            var tempBlob = blobContainer.GetBlockBlobReference(name);

            return tempBlob.UploadFromStreamAsync(stream, null, null, null, ct);
        }

        public Task DeleteAsync(string name)
        {
            var tempBlob = blobContainer.GetBlockBlobReference(name);

            return tempBlob.DeleteIfExistsAsync();
        }

        public Task DeleteAsync(string id, long version, string suffix)
        {
            var tempBlob = blobContainer.GetBlockBlobReference(GetObjectName(id, version, suffix));

            return tempBlob.DeleteIfExistsAsync();
        }

        private string GetObjectName(string id, long version, string suffix)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (blobContainer == null)
            {
                throw new InvalidOperationException("No connection established yet.");
            }

            var name = $"{id}_{version}";

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                name += "_" + suffix;
            }

            return name;
        }
    }
}
