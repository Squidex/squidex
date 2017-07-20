// ==========================================================================
//  AzureBlobAssetStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;
using Squidex.Infrastructure.Assets;

namespace Squidex.Infrastructure.Azure.Storage
{
    public class AzureBlobAssetStore : IAssetStore, IExternalSystem
    {
        private readonly IStorageAccountManager azureStorageAccount;
        private readonly string containerName;
        private CloudBlobContainer blobContainer;
        private const string AssetVersion = "AssetVersion";
        private const string AssetId = "AssetId";

        public AzureBlobAssetStore(IStorageAccountManager azureStorageAccount, string containerName)
        {
            Guard.NotNullOrEmpty(containerName, nameof(containerName));
            Guard.NotNull(azureStorageAccount, nameof(azureStorageAccount));

            this.azureStorageAccount = azureStorageAccount;
            this.containerName = containerName;
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream)
        {
            var blobName = GetObjectName(id, version, suffix);
            var blob = blobContainer.GetBlockBlobReference(blobName);

            if (!await blob.ExistsAsync())
                return;

            await blob.DownloadToStreamAsync(stream);
        }

        public async Task UploadAsync(string id, long version, string suffix, Stream stream)
        {
            var blobName = GetObjectName(id, version, suffix);
            var blob = blobContainer.GetBlockBlobReference(blobName);

            blob.Metadata[AssetVersion] = version.ToString();
            blob.Metadata[AssetId] = id;
            
            await blob.UploadFromStreamAsync(stream);
            await blob.SetMetadataAsync();
        }

        public void Connect()
        {
            try
            {
                blobContainer = azureStorageAccount.GetContainer(containerName);
            }
            catch (Exception)
            {
                throw new ConfigurationException($"Cannot connect to blob container '{containerName}'.");
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
