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
            Guard.NotNullOrEmpty(options.Bucket, nameof(options.Bucket));
            Guard.NotNullOrEmpty(options.AccessKey, nameof(options.AccessKey));
            Guard.NotNullOrEmpty(options.SecretKey, nameof(options.SecretKey));

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

        public async Task<long> GetSizeAsync(string fileName, CancellationToken ct = default)
        {
            var key = GetKey(fileName, nameof(fileName));

            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = options.Bucket,
                    Key = key
                };

                var metadata = await s3Client.GetObjectMetadataAsync(request, ct);

                return metadata.ContentLength;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        public async Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            var sourceKey = GetKey(sourceFileName, nameof(sourceFileName));
            var targetKey = GetKey(targetFileName, nameof(targetFileName));

            try
            {
                await EnsureNotExistsAsync(targetKey, targetFileName, ct);

                var request = new CopyObjectRequest
                {
                    SourceBucket = options.Bucket,
                    SourceKey = sourceKey,
                    DestinationBucket = options.Bucket,
                    DestinationKey = targetKey
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

        public async Task DownloadAsync(string fileName, Stream stream, BytesRange range = default, CancellationToken ct = default)
        {
            Guard.NotNull(stream, nameof(stream));

            var key = GetKey(fileName, nameof(fileName));

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = options.Bucket,
                    Key = key
                };

                if (range.IsDefined)
                {
                    request.ByteRange = new ByteRange(range.ToString());
                }

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
            Guard.NotNull(stream, nameof(stream));

            var key = GetKey(fileName, nameof(fileName));

            try
            {
                if (!overwrite)
                {
                    await EnsureNotExistsAsync(key, fileName, ct);
                }

                var request = new TransferUtilityUploadRequest
                {
                    BucketName = options.Bucket,
                    Key = key
                };

                if (!HasContentLength(stream))
                {
                    var tempFileName = Path.GetTempFileName();

                    var tempStream = new FileStream(tempFileName,
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.Delete, 1024 * 16,
                        FileOptions.Asynchronous |
                        FileOptions.DeleteOnClose |
                        FileOptions.SequentialScan);

                    using (tempStream)
                    {
                        await stream.CopyToAsync(tempStream, ct);

                        request.InputStream = tempStream;

                        await transferUtility.UploadAsync(request, ct);
                    }
                }
                else
                {
                    request.InputStream = new SeekFakerStream(stream);

                    request.AutoCloseStream = false;

                    await transferUtility.UploadAsync(request, ct);
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                throw new AssetAlreadyExistsException(fileName);
            }
        }

        public async Task DeleteAsync(string fileName)
        {
            var key = GetKey(fileName, nameof(fileName));

            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = options.Bucket,
                    Key = key
                };

                await s3Client.DeleteObjectAsync(request);
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return;
            }
        }

        private string GetKey(string fileName, string parameterName)
        {
            Guard.NotNullOrEmpty(fileName, parameterName);

            if (!string.IsNullOrWhiteSpace(options.BucketFolder))
            {
                return $"{options.BucketFolder}/{fileName}";
            }
            else
            {
                return fileName;
            }
        }

        private async Task EnsureNotExistsAsync(string key, string fileName, CancellationToken ct)
        {
            try
            {
                await s3Client.GetObjectAsync(options.Bucket, key, ct);
            }
            catch
            {
                return;
            }

            throw new AssetAlreadyExistsException(fileName);
        }

        private static bool HasContentLength(Stream stream)
        {
            try
            {
                return stream.Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}