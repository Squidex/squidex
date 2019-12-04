// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class AssetStoreDirectoryFactory : IDirectoryFactory
    {
        private const string ArchiveFile = "Archive.zip";
        private const string LockFile = "write.lock";
        private readonly IAssetStore assetStore;

        public AssetStoreDirectoryFactory(IAssetStore assetStore)
        {
            Guard.NotNull(assetStore);

            this.assetStore = assetStore;
        }

        public async Task<LuceneDirectory> CreateAsync(Guid schemaId)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "LocalIndices", schemaId.ToString()));

            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            directoryInfo.Create();

            using (var fileStream = GetArchiveStream(directoryInfo))
            {
                try
                {
                    await assetStore.DownloadAsync(directoryInfo.Name, 0, string.Empty, fileStream);

                    fileStream.Position = 0;

                    using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Read, true))
                    {
                        zipArchive.ExtractToDirectory(directoryInfo.FullName);
                    }
                }
                catch (AssetNotFoundException)
                {
                }
            }

            var directory = FSDirectory.Open(directoryInfo);

            return directory;
        }

        public async Task WriteAsync(IndexWriter writer, SnapshotDeletionPolicy snapshotter)
        {
            Guard.NotNull(writer);
            Guard.NotNull(writer);

            var directoryInfo = ((FSDirectory)writer.Directory).Directory;

            var commit = snapshotter.Snapshot();
            try
            {
                using (var fileStream = GetArchiveStream(directoryInfo))
                {
                    using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var fileName in commit.FileNames)
                        {
                            var file = new FileInfo(Path.Combine(directoryInfo.FullName, fileName));

                            try
                            {
                                if (!file.Name.Equals(ArchiveFile, StringComparison.OrdinalIgnoreCase) &&
                                    !file.Name.Equals(LockFile, StringComparison.OrdinalIgnoreCase))
                                {
                                    zipArchive.CreateEntryFromFile(file.FullName, file.Name);
                                }
                            }
                            catch (IOException)
                            {
                                continue;
                            }
                        }
                    }

                    fileStream.Position = 0;

                    await assetStore.UploadAsync(directoryInfo.Name, 0, string.Empty, fileStream, true);
                }
            }
            finally
            {
                snapshotter.Release(commit);
            }
        }

        private static FileStream GetArchiveStream(DirectoryInfo directoryInfo)
        {
            var path = Path.Combine(directoryInfo.FullName, ArchiveFile);

            return new FileStream(
                path,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.None,
                4096,
                FileOptions.DeleteOnClose);
        }
    }
}
