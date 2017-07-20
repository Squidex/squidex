// ==========================================================================
//  GoogleCloudAssetStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;

namespace Squidex.Infrastructure.Assets
{
    public sealed class GoogleCloudAssetStore : IAssetStore, IExternalSystem
    {
        private readonly string bucketName;
        private StorageClient storageClient;

        public GoogleCloudAssetStore(string bucketName)
        {
            Guard.NotNullOrEmpty(bucketName, nameof(bucketName));

            this.bucketName = bucketName;
        }

        public void Connect()
        {
            try
            {
                storageClient = StorageClient.Create();

                storageClient.GetBucket(bucketName);
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Cannot connect to google cloud bucket '${bucketName}'.", ex);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream)
        {
            var objectName = GetObjectName(id, version, suffix);

            try
            {
                await storageClient.DownloadObjectAsync(bucketName, objectName, stream);
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    throw new AssetNotFoundException($"Asset {id}, {version} not found.", ex);
                }
                throw;
            }
        }

        public async Task UploadAsync(string id, long version, string suffix, Stream stream)
        {
            var objectName = GetObjectName(id, version, suffix);

            await storageClient.UploadObjectAsync(bucketName, objectName, "application/octet-stream", stream);
        }

        private string GetObjectName(string id, long version, string suffix)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (storageClient == null)
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
