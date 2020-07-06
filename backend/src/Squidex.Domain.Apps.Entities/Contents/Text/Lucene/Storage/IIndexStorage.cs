// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public interface IIndexStorage
    {
        Task<Directory> CreateDirectoryAsync(DomainId ownerId);

        Task WriteAsync(Directory directory, SnapshotDeletionPolicy snapshotter);

        Task ClearAsync();
    }
}
