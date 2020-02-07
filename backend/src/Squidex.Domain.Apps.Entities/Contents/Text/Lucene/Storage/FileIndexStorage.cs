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
using Lucene.Net.Store;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene.Storage
{
    public sealed class FileIndexStorage : IIndexStorage
    {
        public Task<LuceneDirectory> CreateDirectoryAsync(Guid schemaId)
        {
            var folderName = $"Indexes/{schemaId}";
            var folderPath = Path.Combine(Path.GetTempPath(), folderName);

            return Task.FromResult<LuceneDirectory>(FSDirectory.Open(folderPath));
        }

        public Task WriteAsync(LuceneDirectory directory, SnapshotDeletionPolicy snapshotter)
        {
            return Task.CompletedTask;
        }
    }
}
