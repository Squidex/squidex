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

        public async Task InitializeAsync(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);

                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobReference = blobClient.GetContainerReference(containerName);

                await blobReference.CreateIfNotExistsAsync();

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

        public async Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default(CancellationToken))
        {
            var targetName = GetObjectName(id, version, suffix);
            var targetBlob = blobContainer.GetBlobReference(targetName);

            var sourceBlob = blobContainer.GetBlockBlobReference(sourceFileName);

            try
            {
                await targetBlob.StartCopyAsync(sourceBlob.Uri, null, AccessCondition.GenerateIfNotExistsCondition(), null, null, ct);

                while (targetBlob.CopyState.Status == CopyStatus.Pending)
                {
                    ct.ThrowIfCancellationRequested();

                    await Task.Delay(50, ct);
                    await targetBlob.FetchAttributesAsync(null, null, null, ct);
                }

                if (targetBlob.CopyState.Status != CopyStatus.Success)
                {
                    throw new StorageException($"Copy of temporary file failed: {targetBlob.CopyState.Status}");
                }
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 409)
            {
                throw new AssetAlreadyExistsException(targetName);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            var blob = blobContainer.GetBlockBlobReference(GetObjectName(id, version, suffix));

            try
            {
                await blob.DownloadToStreamAsync(stream, null, null, null, ct);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                throw new AssetNotFoundException($"Id={id}, Version={version}", ex);
            }
        }

        public Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadCoreAsync(GetObjectName(id, version, suffix), stream, ct);
        }

        public Task UploadAsync(string fileName, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadCoreAsync(fileName, stream, ct);
        }

        public Task DeleteAsync(string id, long version, string suffix)
        {
            return DeleteCoreAsync(GetObjectName(id, version, suffix));
        }

        public Task DeleteAsync(string fileName)
        {
            return DeleteCoreAsync(fileName);
        }

        private Task DeleteCoreAsync(string blobName)
        {
            var blob = blobContainer.GetBlockBlobReference(blobName);

            return blob.DeleteIfExistsAsync();
        }

        private async Task UploadCoreAsync(string blobName, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var tempBlob = blobContainer.GetBlockBlobReference(blobName);

                await tempBlob.UploadFromStreamAsync(stream, AccessCondition.GenerateIfNotExistsCondition(), null, null, ct);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 409)
            {
                throw new AssetAlreadyExistsException(blobName);
            }
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
