// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace Squidex.Infrastructure.Assets
{
    public sealed class AmazonS3AssetStore : DisposableObjectBase, IAssetStore, IInitializable
    {
        private const int BufferSize = 81920;
        private readonly string accessKey;
        private readonly string secretKey;
        private readonly string bucketName;
        private readonly string? bucketFolder;
        private readonly RegionEndpoint bucketRegion;
        private TransferUtility transferUtility;
        private IAmazonS3 s3Client;

        public AmazonS3AssetStore(string regionName, string bucketName, string? bucketFolder, string accessKey, string secretKey)
        {
            Guard.NotNullOrEmpty(bucketName);
            Guard.NotNullOrEmpty(accessKey);
            Guard.NotNullOrEmpty(secretKey);

            this.bucketName = bucketName;
            this.bucketFolder = bucketFolder;
            this.accessKey = accessKey;
            this.secretKey = secretKey;

            bucketRegion = RegionEndpoint.GetBySystemName(regionName);
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                s3Client?.Dispose();

                transferUtility?.Dispose();
            }
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                s3Client = new AmazonS3Client(
                    accessKey,
                    secretKey,
                    bucketRegion);

                transferUtility = new TransferUtility(s3Client);

                var exists = await s3Client.DoesS3BucketExistAsync(bucketName);

                if (!exists)
                {
                    throw new ConfigurationException($"Cannot connect to Amazon S3 bucket '${bucketName}'.");
                }
            }
            catch (AmazonS3Exception ex)
            {
                throw new ConfigurationException($"Cannot connect to Amazon S3 bucket '${bucketName}'.", ex);
            }
        }

        public string? GeneratePublicUrl(string fileName)
        {
            return null;
        }

        public async Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(sourceFileName);
            Guard.NotNullOrEmpty(targetFileName);

            try
            {
                await EnsureNotExistsAsync(targetFileName, ct);

                var request = new CopyObjectRequest
                {
                    SourceBucket = bucketName,
                    SourceKey = GetKey(sourceFileName),
                    DestinationBucket = bucketName,
                    DestinationKey = GetKey(targetFileName)
                };

                await s3Client.CopyObjectAsync(request, ct);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new AssetAlreadyExistsException(targetFileName);
            }
        }

        public async Task DownloadAsync(string fileName, Stream stream, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName);
            Guard.NotNull(stream);

            try
            {
                var request = new GetObjectRequest { BucketName = bucketName, Key = GetKey(fileName) };

                using (var response = await s3Client.GetObjectAsync(request, ct))
                {
                    await response.ResponseStream.CopyToAsync(stream, BufferSize, ct);
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        public async Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            Guard.NotNullOrEmpty(fileName);
            Guard.NotNull(stream);

            try
            {
                if (!overwrite)
                {
                    await EnsureNotExistsAsync(fileName, ct);
                }

                var request = new TransferUtilityUploadRequest
                {
                    AutoCloseStream = false,
                    BucketName = bucketName,
                    InputStream = stream,
                    Key = GetKey(fileName)
                };

                await transferUtility.UploadAsync(request, ct);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new AssetAlreadyExistsException(fileName);
            }
        }

        public async Task DeleteAsync(string fileName)
        {
            Guard.NotNullOrEmpty(fileName);

            try
            {
                var request = new DeleteObjectRequest { BucketName = bucketName, Key = fileName };

                await s3Client.DeleteObjectAsync(request);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
        }

        private string GetKey(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(bucketFolder))
            {
                return $"{bucketFolder}/{fileName}";
            }
            else
            {
                return fileName;
            }
        }

        private async Task EnsureNotExistsAsync(string fileName, CancellationToken ct)
        {
            try
            {
                await s3Client.GetObjectAsync(bucketName, GetKey(fileName), ct);
            }
            catch
            {
                return;
            }

            throw new AssetAlreadyExistsException(fileName);
        }
    }
}