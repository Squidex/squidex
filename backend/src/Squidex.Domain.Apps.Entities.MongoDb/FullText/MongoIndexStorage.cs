// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Lucene.Net.Index;
using MongoDB.Driver.GridFS;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.MongoDb.FullText
{
    public sealed class MongoIndexStorage : IIndexStorage
    {
        private readonly IGridFSBucket<string> bucket;

        public MongoIndexStorage(IGridFSBucket<string> bucket)
        {
            this.bucket = bucket;
        }

        public Task<LuceneDirectory> CreateDirectoryAsync(Guid schemaId)
        {
            var folderName = schemaId.ToString();

            var tempFolder = Path.Combine(Path.GetTempPath(), "Indices", folderName);
            var tempDirectory = new DirectoryInfo(tempFolder);

            var directory = new MongoDirectory(bucket, folderName, tempDirectory);

            return Task.FromResult<LuceneDirectory>(directory);
        }

        public Task WriteAsync(LuceneDirectory directory, SnapshotDeletionPolicy snapshotter)
        {
            return Task.CompletedTask;
        }
    }
}
