// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public interface IDirectoryFactory
    {
        Task<Directory> CreateAsync(Guid schemaId);

        Task WriteAsync(IndexWriter writer, SnapshotDeletionPolicy snapshotter);
    }
}
