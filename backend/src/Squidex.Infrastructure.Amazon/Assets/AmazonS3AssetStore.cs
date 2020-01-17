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
        private readonly AmazonS3Options options;
        private TransferUtility transferUtility;
        private IAmazonS3 s3Client;

        public AmazonS3AssetStore(AmazonS3Options options)
        {
            Guard.NotNullOrEmpty(options.Bucket);
            Guard.NotNullOrEmpty(options.AccessKey);
            Guard.NotNullOrEmpty(options.SecretKey);

            this.options = options;
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
                var amazonS3Config = new AmazonS3Config { ForcePathStyle = options.ForcePathStyle };

                if (!string.IsNullOrWhiteSpace(options.ServiceUrl))
                {
                    amazonS3Config.ServiceURL = options.ServiceUrl;
                }
                else
                {
                    amazonS3Config.RegionEndpoint = RegionEndpoint.GetBySystemName(options.RegionName);
                }

                s3Client = new AmazonS3Client(options.AccessKey, options.SecretKey, amazonS3Config);

                transferUtility = new TransferUtility(s3Client);

                var exists = await s3Client.DoesS3BucketExistAsync(options.Bucket);

                if (!exists)
                {
                    throw new ConfigurationException($"Cannot connect to Amazon S3 bucket '{options.Bucket}'.");
                }
            }
            catch (AmazonS3Exception ex)
            {
                throw new ConfigurationException($"Cannot connect to Amazon S3 bucket '{options.Bucket}'.", ex);
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
                    SourceBucket = options.Bucket,
                    SourceKey = GetKey(sourceFileName),
                    DestinationBucket = options.Bucket,
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
                var request = new GetObjectRequest { BucketName = options.Bucket, Key = GetKey(fileName) };

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
                    Key = GetKey(fileName)
                };

                ConfigureDefaults(request);

                // Amazon S3 requires a seekable stream, but does not seek anything.
                request.InputStream = new SeekFakerStream(stream);

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
                var request = new DeleteObjectRequest { BucketName = options.Bucket, Key = fileName };

                await s3Client.DeleteObjectAsync(request);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
        }

        private string GetKey(string fileName)
        {
            if (!string.IsNullOrWhiteSpace(options.BucketFolder))
            {
                return $"{options.BucketFolder}/{fileName}";
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
                await s3Client.GetObjectAsync(options.Bucket, GetKey(fileName), ct);
            }
            catch
            {
                return;
            }

            throw new AssetAlreadyExistsException(fileName);
        }

        private void ConfigureDefaults(TransferUtilityUploadRequest request)
        {
            request.AutoCloseStream = false;
            request.BucketName = options.Bucket;
        }
    }
}