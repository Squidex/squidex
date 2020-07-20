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
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FolderAssetStore : IAssetStore, IInitializable
    {
        private const int BufferSize = 81920;
        private readonly ISemanticLog log;
        private readonly DirectoryInfo directory;

        public FolderAssetStore(string path, ISemanticLog log)
        {
            Guard.NotNullOrEmpty(path, nameof(path));
            Guard.NotNull(log, nameof(log));

            this.log = log;

            directory = new DirectoryInfo(path);
        }

        public Task InitializeAsync(CancellationToken ct = default)
        {
            try
            {
                if (!directory.Exists)
                {
                    directory.Create();
                }

                log.LogInformation(w => w
                    .WriteProperty("action", "FolderAssetStoreConfigured")
                    .WriteProperty("path", directory.FullName));

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Cannot access directory {directory.FullName}", ex);
            }
        }

        public string? GeneratePublicUrl(string fileName)
        {
            return null;
        }

        public Task<long> GetSizeAsync(string fileName, CancellationToken ct = default)
        {
            var file = GetFile(fileName, nameof(fileName));

            try
            {
                return Task.FromResult(file.Length);
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        public Task CopyAsync(string sourceFileName, string targetFileName, CancellationToken ct = default)
        {
            var targetFile = GetFile(targetFileName, nameof(targetFileName));
            var sourceFile = GetFile(sourceFileName, nameof(sourceFileName));

            try
            {
                sourceFile.CopyTo(targetFile.FullName);

                return Task.CompletedTask;
            }
            catch (IOException) when (targetFile.Exists)
            {
                throw new AssetAlreadyExistsException(targetFileName);
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
        }

        public async Task DownloadAsync(string fileName, Stream stream, BytesRange range, CancellationToken ct = default)
        {
            Guard.NotNull(stream, nameof(stream));

            var file = GetFile(fileName, nameof(fileName));

            try
            {
                using (var fileStream = file.OpenRead())
                {
                    await fileStream.CopyToAsync(stream, range, ct);
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException(fileName, ex);
            }
        }

        public async Task UploadAsync(string fileName, Stream stream, bool overwrite = false, CancellationToken ct = default)
        {
            Guard.NotNull(stream, nameof(stream));

            var file = GetFile(fileName, nameof(fileName));

            try
            {
                using (var fileStream = file.Open(overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream, BufferSize, ct);
                }
            }
            catch (IOException) when (file.Exists)
            {
                throw new AssetAlreadyExistsException(file.Name);
            }
        }

        public Task DeleteAsync(string fileName)
        {
            var file = GetFile(fileName, nameof(fileName));

            file.Delete();

            return Task.CompletedTask;
        }

        private FileInfo GetFile(string fileName, string parameterName)
        {
            Guard.NotNullOrEmpty(fileName, parameterName);

            return new FileInfo(GetPath(fileName));
        }

        private string GetPath(string name)
        {
            return Path.Combine(directory.FullName, name);
        }
    }
}
