// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Cloud.Storage.V1;

namespace Squidex.Infrastructure.Assets
{
    public sealed class GoogleCloudAssetStore : IAssetStore, IInitializable
    {
        private static readonly UploadObjectOptions IfNotExists = new UploadObjectOptions { IfGenerationMatch = 0 };
        private static readonly CopyObjectOptions IfNotExistsCopy = new CopyObjectOptions { IfGenerationMatch = 0 };
        private readonly string bucketName;
        private StorageClient storageClient;

        public GoogleCloudAssetStore(string bucketName)
        {
            Guard.NotNullOrEmpty(bucketName, nameof(bucketName));

            this.bucketName = bucketName;
        }

        public async Task InitializeAsync(CancellationToken ct = default(CancellationToken))
        {
            try
            {
                storageClient = StorageClient.Create();

                await storageClient.GetBucketAsync(bucketName, cancellationToken: ct);
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

        public async Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default(CancellationToken))
        {
            var objectName = GetObjectName(id, version, suffix);

            try
            {
                await storageClient.CopyObjectAsync(bucketName, sourceFileName, bucketName, objectName, IfNotExistsCopy, ct);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new AssetAlreadyExistsException(objectName);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            var objectName = GetObjectName(id, version, suffix);

            try
            {
                await storageClient.DownloadObjectAsync(bucketName, objectName, stream, cancellationToken: ct);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
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

        private async Task UploadCoreAsync(string objectName, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                await storageClient.UploadObjectAsync(bucketName, objectName, "application/octet-stream", stream, IfNotExists, ct);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new AssetAlreadyExistsException(objectName);
            }
        }

        private async Task DeleteCoreAsync(string objectName)
        {
            try
            {
                await storageClient.DeleteObjectAsync(bucketName, objectName);
            }
            catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return;
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
