﻿// ==========================================================================
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
            Guard.NotNullOrEmpty(containerName);
            Guard.NotNullOrEmpty(connectionString);

            this.connectionString = connectionString;
            this.containerName = containerName;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
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

        public string? GeneratePublicUrl(string fileName)
        {
            Guard.NotNullOrEmpty(fileName);

            if (blobContainer.Properties.PublicAccess != BlobContainerPublicAccessType.Blob)
            {
                var blob = blobContainer.GetBlockBlobReference(fileName);

                return blob.Uri.ToString();
            }

            return null;
        }

        public async Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(sourceFileName);
            Guard.NotNullOrEmpty(targetFileName);

            try
            {
                var sourceBlob = blobContainer.GetBlockBlobReference(sourceFileName);

                var targetBlob = blobContainer.GetBlobReference(targetFileName);

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
                throw new AssetAlreadyExistsException(targetFileName);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
        }

        public async Task DownloadAsync(string fileName, Stream stream, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName);

            try
            {
                var blob = blobContainer.GetBlockBlobReference(fileName);

                await blob.DownloadToStreamAsync(stream, null, null, null, ct);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404)
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        public async Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName);

            try
            {
                var tempBlob = blobContainer.GetBlockBlobReference(fileName);

                await tempBlob.UploadFromStreamAsync(stream, overwrite ? null : AccessCondition.GenerateIfNotExistsCondition(), null, null, ct);
            }
            catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 409)
            {
                throw new AssetAlreadyExistsException(fileName);
            }
        }

        public Task DeleteAsync(string fileName)
        {
            Guard.NotNullOrEmpty(fileName);

            var blob = blobContainer.GetBlockBlobReference(fileName);

            return blob.DeleteIfExistsAsync();
        }
    }
}
