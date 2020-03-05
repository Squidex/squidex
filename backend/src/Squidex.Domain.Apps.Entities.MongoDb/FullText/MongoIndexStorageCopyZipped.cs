// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Store;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoIndexStorageCopyZipped : IIndexStorage
    {
        private const string ArchiveFile = "Archive.zip";
        private const string LockFile = "write.lock";
        private readonly IGridFSBucket<string> bucket;

        public MongoIndexStorageCopyZipped(IGridFSBucket<string> bucket)
        {
            Guard.NotNull(bucket);

            this.bucket = bucket;
        }

        public async Task<LuceneDirectory> CreateDirectoryAsync(Guid ownerId)
        {
            var name = $"index_{ownerId}";

            var directoryInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), name));

            if (directoryInfo.Exists)
            {
                directoryInfo.Delete(true);
            }

            directoryInfo.Create();

            using (var archiveStream = GetArchiveStream(directoryInfo))
            {
                try
                {
                    using (var stream = await bucket.OpenDownloadStreamAsync(name))
                    {
                        using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, true))
                        {
                            foreach (var entry in zipArchive.Entries)
                            {
                                var file = new FileInfo(Path.Combine(directoryInfo.FullName, entry.FullName));

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
                foreach (var file in await FindFilesAsync(directoryInfo.Name))
                {
                    await bucket.DeleteAsync(file.Id);
                }

                foreach (var fileName in commit.FileNames)
                {
                    var fileInfo = new FileInfo(Path.Combine(directoryInfo.FullName, fileName));

                    using (var fileStream = fileInfo.OpenRead())
                    {
                        var fileId = GetFileId(directoryInfo.Name, fileName);

                        await bucket.UploadFromStreamAsync(fileId, fileName, fileStream);
                    }
                }
            }
            finally
            {
                snapshotter.Release(commit);
            }
        }

        private string GetFileId(string directory, string name)
        {
            return $"{directory}/{name}";
        }

        private Task<List<GridFSFileInfo<string>>> FindFilesAsync(string directory)
        {
            var filter = Builders<GridFSFileInfo<string>>.Filter.Regex(x => x.Id, new BsonRegularExpression($"^{directory}/"));

            return bucket.Find(filter).ToListAsync();
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
