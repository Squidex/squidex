// ==========================================================================
//  FolderAssetStorage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;

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

        public async Task DownloadAsync(Guid id, long version, string suffix, Stream stream)
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

        public async Task UploadAsync(Guid id, long version, string suffix, Stream stream)
        {
            var file = GetFile(id, version, suffix);

            using (var fileStream = file.OpenWrite())
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        private FileInfo GetFile(Guid id, long version, string suffix)
        {
            var path = Path.Combine(directory.FullName, $"{id}_{version}");

            if (!string.IsNullOrWhiteSpace(suffix))
            {
                path += "_" + suffix;
            }

            return new FileInfo(path);
        }
    }
}
