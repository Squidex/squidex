// ==========================================================================
//  FolderAssetStorage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

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

        public Task<Stream> GetAssetAsync(string name)
        {
            var file = GetFile(name);

            Stream stream = null;

            try
            {
                if (file.Exists)
                {
                    stream = file.OpenRead();
                }
            }
            catch (FileNotFoundException)
            {
                stream = null;
            }

            return Task.FromResult(stream);
        }

        public async Task UploadAssetAsync(string name, Stream stream)
        {
            var file = GetFile(name);

            using (var fileStream = file.OpenWrite())
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        private FileInfo GetFile(string name)
        {
            Guard.ValidFileName(name, nameof(name));

            var file = new FileInfo(Path.Combine(directory.FullName, name));

            return file;
        }
    }
}
