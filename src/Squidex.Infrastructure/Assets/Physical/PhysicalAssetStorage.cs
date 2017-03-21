// ==========================================================================
//  PhysicalAssetStorage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets.Physical
{
    public sealed class PhysicalAssetStorage : IAssetStorage, IExternalSystem
    {
        private readonly DirectoryInfo directory;

        public PhysicalAssetStorage(string path)
        {
            Guard.NotNullOrEmpty(path, nameof(path));

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
            }
            catch
            {
                if (!directory.Exists)
                {
                    throw new ConfigurationException($"Cannot access directory {directory.FullName}");
                }
            }
        }

        public Task<Stream> GetAssetAsync(Guid id, string tags = null)
        {
            var file = GetFile(id, tags);

            Stream stream = null;

            if (file.Exists)
            {
                stream = file.OpenRead();
            }

            return Task.FromResult(stream);
        }

        public async Task UploadAssetAsync(Guid id, Stream stream, string tags = null)
        {
            var file = GetFile(id, tags);

            using (var fileStream = file.OpenWrite())
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        private FileInfo GetFile(Guid id, string tags)
        {
            var fileName = id.ToString();

            if (!string.IsNullOrWhiteSpace(tags))
            {
                fileName += tags;
            }

            Guard.ValidFileName(fileName, tags);

            var file = new FileInfo(Path.Combine(directory.FullName, fileName));

            return file;
        }
    }
}
