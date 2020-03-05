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
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Infrastructure;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoIndexStorage : IIndexStorage
    {
        private const string ArchiveFile = "Archive.zip";
        private const string LockFile = "write.lock";
        private readonly IGridFSBucket<string> bucket;

        public MongoIndexStorage(IGridFSBucket<string> bucket)
        {
            Guard.NotNull(bucket);

            this.bucket = bucket;
        }

        public async Task<LuceneDirectory> CreateDirectoryAsync(Guid ownerId)
        {
            var fileId = $"index_{ownerId}";

            var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), fileId));

            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            directoryInfo.Create();

            try
            {
                using (var stream = await bucket.OpenDownloadStreamAsync(fileId))
                {
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                    {
                        foreach (var entry in zipArchive.Entries)
                        {
                            var file = new FileInfo(Path.Combine(directoryInfo.FullName, entry.Name));

                            using (var entryStream = entry.Open())
                            {
                                using (var fileStream = file.OpenWrite())
                                {
                                    await entryStream.CopyToAsync(fileStream);
                                }
                            }
                        }
                    }
                }
            }
            catch (GridFSFileNotFoundException)
            {
            }

            var directory = FSDirectory.Open(directoryInfo);

            return directory;
        }

        public Task ClearAsync()
        {
            return bucket.DropAsync();
        }

        public async Task WriteAsync(LuceneDirectory directory, SnapshotDeletionPolicy snapshotter)
        {
            Guard.NotNull(directory);
            Guard.NotNull(snapshotter);

            var directoryInfo = ((FSDirectory)directory).Directory;

            var commit = snapshotter.Snapshot();
            try
            {
                var fileId = directoryInfo.Name;

                try
                {
                    await bucket.DeleteAsync(fileId);
                }
                catch (GridFSFileNotFoundException)
                {
                }

                using (var stream = await bucket.OpenUploadStreamAsync(fileId, fileId))
                {
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true))
                    {
                        foreach (var fileName in commit.FileNames)
                        {
                            var file = new FileInfo(Path.Combine(directoryInfo.FullName, fileName));

                            try
                            {
                                if (!file.Name.Equals(ArchiveFile, StringComparison.OrdinalIgnoreCase) &&
                                    !file.Name.Equals(LockFile, StringComparison.OrdinalIgnoreCase))
                                {
                                    using (var fileStream = file.OpenRead())
                                    {
                                        var entry = zipArchive.CreateEntry(fileStream.Name);

                                        using (var entryStream = entry.Open())
                                        {
                                            await fileStream.CopyToAsync(entryStream);
                                        }
                                    }
                                }
                            }
                            catch (IOException)
                            {
                                continue;
                            }
                        }
                    }
                }
            }
            finally
            {
                snapshotter.Release(commit);
            }
        }
    }
}
