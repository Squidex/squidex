// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

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

        public Task InitializeAsync(CancellationToken ct = default(CancellationToken))
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

                return TaskHelper.Done;
            }
            catch (Exception ex)
            {
                throw new ConfigurationException($"Cannot access directory {directory.FullName}", ex);
            }
        }

        public string GenerateSourceUrl(string id, long version, string suffix)
        {
            var file = GetFile(id, version, suffix);

            return file.FullName;
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            var file = GetFile(id, version, suffix);

            try
            {
                using (var fileStream = file.OpenRead())
                {
                    await fileStream.CopyToAsync(stream, BufferSize, ct);
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Id={id}, Version={version}", ex);
            }
        }

        public Task CopyAsync(string sourceFileName, string id, long version, string suffix, CancellationToken ct = default(CancellationToken))
        {
            var targetFile = GetFile(id, version, suffix);

            try
            {
                var file = GetFile(sourceFileName);

                file.CopyTo(targetFile.FullName);

                return TaskHelper.Done;
            }
            catch (IOException) when (targetFile.Exists)
            {
                throw new AssetAlreadyExistsException(targetFile.Name);
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException(sourceFileName, ex);
            }
        }

        public Task UploadAsync(string id, long version, string suffix, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadCoreAsync(GetFile(id, version, suffix), stream, ct);
        }

        public Task UploadAsync(string fileName, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            return UploadCoreAsync(GetFile(fileName), stream, ct);
        }

        public Task DeleteAsync(string id, long version, string suffix)
        {
            return DeleteFileCoreAsync(GetFile(id, version, suffix));
        }

        public Task DeleteAsync(string fileName)
        {
            return DeleteFileCoreAsync(GetFile(fileName));
        }

        private static Task DeleteFileCoreAsync(FileInfo file)
        {
            file.Delete();

            return TaskHelper.Done;
        }

        private static async Task UploadCoreAsync(FileInfo file, Stream stream, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                using (var fileStream = file.Open(FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream, BufferSize, ct);
                }
            }
            catch (IOException) when (file.Exists)
            {
                throw new AssetAlreadyExistsException(file.Name);
            }
        }

        private FileInfo GetFile(string id, long version, string suffix)
        {
            Guard.NotNullOrEmpty(id, nameof(id));

            return GetFile(GetPath(id, version, suffix));
        }

        private FileInfo GetFile(string fileName)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));

            return new FileInfo(GetPath(fileName));
        }

        private string GetPath(string name)
        {
            return Path.Combine(directory.FullName, name);
        }

        private string GetPath(string id, long version, string suffix)
        {
            return Path.Combine(directory.FullName, string.Join("_", new[] { id, version.ToString(), suffix }.Where(x => !string.IsNullOrWhiteSpace(x))));
        }
    }
}
