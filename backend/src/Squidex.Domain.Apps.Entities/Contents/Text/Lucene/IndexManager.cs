// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public sealed partial class IndexManager : DisposableObjectBase
    {
        private readonly Dictionary<Guid, IndexHolder> indices = new Dictionary<Guid, IndexHolder>();
        private readonly SemaphoreSlim lockObject = new SemaphoreSlim(1);
        private readonly IIndexStorage indexStorage;
        private readonly ISemanticLog log;

        public IndexManager(IIndexStorage indexStorage, ISemanticLog log)
        {
            Guard.NotNull(indexStorage);
            Guard.NotNull(log);

            this.indexStorage = indexStorage;

            this.log = log;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                ReleaseAllAsync().Wait();
            }
        }

        public Task ClearAsync()
        {
            return indexStorage.ClearAsync();
        }

        public async Task<IIndex> AcquireAsync(Guid ownerId)
        {
            IndexHolder? indexHolder;

            try
            {
                await lockObject.WaitAsync();

                if (indices.TryGetValue(ownerId, out indexHolder))
                {
                    log.LogWarning(w => w
                        .WriteProperty("message", "Unreleased index found.")
                        .WriteProperty("ownerId", ownerId.ToString()));

                    await CommitInternalAsync(indexHolder, true);
                }

                indexHolder = new IndexHolder(ownerId);
                indices[ownerId] = indexHolder;
            }
            finally
            {
                lockObject.Release();
            }

            var directory = await indexStorage.CreateDirectoryAsync(ownerId);

            indexHolder.Open(directory);

            return indexHolder;
        }

        public async Task ReleaseAsync(IIndex index)
        {
            Guard.NotNull(index);

            var indexHolder = (IndexHolder)index;

            try
            {
                lockObject.Wait();

                indexHolder.Release();
                indices.Remove(indexHolder.Id);
            }
            finally
            {
                lockObject.Release();
            }

            await CommitInternalAsync(indexHolder, true);
        }

        public Task CommitAsync(IIndex index)
        {
            Guard.NotNull(index);

            return CommitInternalAsync(index, false);
        }

        private async Task CommitInternalAsync(IIndex index, bool dispose)
        {
            if (index is IndexHolder holder)
            {
                if (dispose)
                {
                    holder.Dispose();
                }
                else
                {
                    holder.Commit();
                }

                await indexStorage.WriteAsync(holder.GetUnsafeWriter().Directory, holder.Snapshotter);
            }
        }

        private async Task ReleaseAllAsync()
        {
            var current = indices.Values.ToList();

            try
            {
                lockObject.Wait();

                indices.Clear();
            }
            finally
            {
                lockObject.Release();
            }

            if (current.Count > 0)
            {
                log.LogWarning(w => w
                    .WriteProperty("message", "Unreleased indices found.")
                    .WriteProperty("count", indices.Count));

                foreach (var index in current)
                {
                    await CommitInternalAsync(index, true);
                }
            }
        }
    }
}
