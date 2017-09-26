// ==========================================================================
//  GoogleCloudAssetStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
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

        public string GenerateSourceUrl(string id, long version, string suffix)
        {
            var objectName = GetObjectName(id, version, suffix);

            return $"https://storage.cloud.google.com/{bucketName}/{objectName}";
        }

        public Task UploadTemporaryAsync(string name, Stream stream)
        {
            return storageClient.UploadObjectAsync(bucketName, name, "application/octet-stream", stream);
        }

        public async Task UploadAsync(string id, long version, string suffix, Stream stream)
        {
            var objectName = GetObjectName(id, version, suffix);

            await storageClient.UploadObjectAsync(bucketName, objectName, "application/octet-stream", stream);
        }

        public async Task CopyTemporaryAsync(string name, string id, long version, string suffix)
        {
            var objectName = GetObjectName(id, version, suffix);

            try
            {
                await storageClient.CopyObjectAsync(bucketName, name, bucketName, objectName);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                throw new AssetNotFoundException($"Asset {name} not found.", ex);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream)
        {
            var objectName = GetObjectName(id, version, suffix);

            try
            {
                await storageClient.DownloadObjectAsync(bucketName, objectName, stream);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                throw new AssetNotFoundException($"Asset {id}, {version} not found.", ex);
            }
        }

        public async Task DeleteTemporaryAsync(string name)
        {
            try
            {
                await storageClient.DeleteObjectAsync(bucketName, name);
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
        }

        private string GetObjectName(string id, long version, string suffix)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            if (storageClient == null)
            {
                throw new InvalidOperationException("No connection established yet.");
            }

            var name = GetFileName(id, version, suffix);

            return name;
        }

        private static string GetFileName(string id, long version, string suffix)
        {
            return string.Join("_", new[] { id, version.ToString(), suffix }.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
