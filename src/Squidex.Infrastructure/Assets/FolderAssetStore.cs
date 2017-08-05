// ==========================================================================
//  FolderAssetStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public sealed class FolderAssetStore : IAssetStore, IExternalSystem
    {
        private readonly ISemanticLog log;
        private readonly DirectoryInfo directory;

        public FolderAssetStore(string path, ISemanticLog log)
        {
            Guard.NotNullOrEmpty(path, nameof(path));
            Guard.NotNull(log, nameof(log));

            this.log = log;

            directory = new DirectoryInfo(path);
        }

        public void Connect()
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
            }
            catch
            {
                if (!directory.Exists)
                {
                    throw new ConfigurationException($"Cannot access directory {directory.FullName}");
                }
            }
        }

        public async Task UploadTemporaryAsync(string name, Stream stream)
        {
            var file = GetFile(name);

            using (var fileStream = file.OpenWrite())
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        public async Task UploadAsync(string id, long version, string suffix, Stream stream)
        {
            var file = GetFile(id, version, suffix);

            using (var fileStream = file.OpenWrite())
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        public async Task DownloadAsync(string id, long version, string suffix, Stream stream)
        {
            var file = GetFile(id, version, suffix);

            try
            {
                using (var fileStream = file.OpenRead())
                {
                    await fileStream.CopyToAsync(stream);
                }
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Asset {id}, {version} not found.", ex);
            }
        }

        public Task CopyTemporaryAsync(string name, string id, long version, string suffix)
        {
            try
            {
                var file = GetFile(name);

                file.CopyTo(GetPath(id, version, suffix));

                return TaskHelper.Done;
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Asset {name} not found.", ex);
            }
        }

        public Task DeleteTemporaryAsync(string name)
        {
            try
            {
                var file = GetFile(name);

                file.Delete();

                return TaskHelper.Done;
            }
            catch (FileNotFoundException ex)
            {
                throw new AssetNotFoundException($"Asset {name} not found.", ex);
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
            return Path.Combine(directory.FullName, string.Join("_", new[] { id, version.ToString(), suffix }.Where(x => !string.IsNullOrWhiteSpace(x))));
        }
    }
}
