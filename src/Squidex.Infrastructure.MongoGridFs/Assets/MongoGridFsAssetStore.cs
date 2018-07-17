// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.GridFS;

namespace Squidex.Infrastructure.Assets
{
    public class MongoGridFsAssetStore : IAssetStore, IInitializable
    {
        public const int ChunkSizeBytes = 255 * 1024;
        private const int BufferSize = 81920;

        private readonly string path;
        private readonly IGridFSBucket<string> bucket;
        private readonly DirectoryInfo directory;

        public MongoGridFsAssetStore(IGridFSBucket<string> bucket, string path)
        {
            Guard.NotNull(bucket, nameof(bucket));
            Guard.NotNullOrEmpty(path, nameof(path));

            this.bucket = bucket;
            this.path = path;

            directory = new DirectoryInfo(path);
        }

        public void Initialize()
        {
            try
            {
                // test bucket
                bucket.Database.ListCollections();

                if (!directory.Exists)
                {
                    directory.Create();
                }
            }
            catch (MongoException ex)
            {
                throw new ConfigurationException(
                    $"Cannot connect to Mongo GridFS bucket '${bucket.Options.BucketName}'.", ex);
            }
            catch (IOException ex)
            {
                if (!directory.Exists)
                {
                    throw new ConfigurationException($"Cannot access directory '{directory.FullName}'", ex);
                }
            }
        }

        public string GenerateSourceUrl(string id, long version, string suffix)
        {
            var file = GetFile(id, version, suffix);

            return file.FullName;
        }

        public async Task CopyAsync(string name, string id, long version, string suffix,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var file = GetFile(name);
                var toFile = GetFile(id, version, suffix);

                file.CopyTo(toFile.FullName);

                using (var readStream = await bucket.OpenDownloadStreamAsync(file.Name, cancellationToken: ct))
                {
                    using (var writeStream =
                        await bucket.OpenUploadStreamAsync(toFile.Name, toFile.Name, cancellationToken: ct))
                    {
                        var buffer = new byte[ChunkSizeBytes];
                        int bytesRead;
                        while ((bytesRead = await readStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                        {
                            await writeStream.WriteAsync(buffer, 0, bytesRead, ct);
                        }

                        await writeStream.CloseAsync(ct);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Asset {name} not found.", ex);
            }
            catch (GridFSException ex)
            {
                throw new AssetNotFoundException($"Asset {name} not found.", ex);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream,
            CancellationToken ct = default(CancellationToken))
        {
            var file = GetFile(id, version, suffix);

            try
            {
                if (file.Exists)
                {
                    using (var fileStream = file.OpenRead())
                    {
                        await fileStream.CopyToAsync(stream, BufferSize, ct);
                    }
                }
                else
                {
                    // file not found locally
                    // read from GridFS
                    using (var readStream = await bucket.OpenDownloadStreamAsync(file.Name, cancellationToken: ct))
                    {
                        using (var fileStream = file.OpenWrite())
                        {
                            var buffer = new byte[BufferSize];
                            int bytesRead;
                            while ((bytesRead = await readStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                                await stream.WriteAsync(buffer, 0, bytesRead, ct);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new AssetNotFoundException($"Asset {id}, {version} not found.", ex);
            }
        }

        public Task UploadAsync(string name, Stream stream, CancellationToken ct = default(CancellationToken))
            => UploadFileCoreAsync(GetFile(name), stream, ct);

        public Task UploadAsync(string id, long version, string suffix, Stream stream,
            CancellationToken ct = default(CancellationToken))
            => UploadFileCoreAsync(GetFile(id, version, suffix), stream, ct);

        public Task DeleteAsync(string name)
            => DeleteCoreAsync(GetFile(name));

        public Task DeleteAsync(string id, long version, string suffix)
            => DeleteCoreAsync(GetFile(id, version, suffix));

        private async Task DeleteCoreAsync(FileInfo file, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                file.Delete();
                await bucket.DeleteAsync(file.Name, ct);
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Asset {file.Name} not found.", ex);
            }
            catch (GridFSException ex)
            {
                throw new GridFSException(
                    $"Cannot delete file {file.Name} into Mongo GridFS bucket '{bucket.Options.BucketName}'.", ex);
            }
        }

        private async Task UploadFileCoreAsync(FileInfo file, Stream stream,
            CancellationToken ct = default(CancellationToken))
        {
            try
            {
                // upload file to GridFS first
                await bucket.UploadFromStreamAsync(file.Name, file.Name, stream, cancellationToken: ct);

                // reset stream position
                stream.Position = 0;

                // create file locally
                // even if this stage will fail, file will be recreated on the next Download call
                using (var fileStream = file.OpenWrite())
                {
                    await stream.CopyToAsync(fileStream, BufferSize, ct);
                }
            }
            catch (IOException ex)
            {
                throw new IOException($"Cannot write file '{file.Name}' into directory '{directory.FullName}'.", ex);
            }
            catch (GridFSException ex)
            {
                throw new GridFSException(
                    $"Cannot upload file {file.Name} into Mongo GridFS bucket '{bucket.Options.BucketName}'.",
                    ex);
            }
        }

        private FileInfo GetFile(string id, long version, string suffix)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return GetFile(GetPath(id, version, suffix));
        }

        private FileInfo GetFile(string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            return new FileInfo(GetPath(name));
        }

        private string GetPath(string name)
        {
            return Path.Combine(directory.FullName, name);
        }

        private string GetPath(string id, long version, string suffix)
        {
            return Path.Combine(directory.FullName,
                string.Join("_",
                    new[] { id, version.ToString(), suffix }.ToList().Where(x => !string.IsNullOrWhiteSpace(x))));
        }
    }
}