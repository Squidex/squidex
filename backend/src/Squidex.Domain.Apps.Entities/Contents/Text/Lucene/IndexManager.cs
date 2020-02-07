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
        private readonly IIndexStorage directoryFactory;
        private readonly ISemanticLog log;

        public IndexManager(IIndexStorage directoryFactory, ISemanticLog log)
        {
            Guard.NotNull(directoryFactory);
            Guard.NotNull(log);

            this.directoryFactory = directoryFactory;

            this.log = log;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                ReleaseAllAsync().Wait();
            }
        }

        public async Task<IIndex> AcquireAsync(Guid schemaId)
        {
            IndexHolder? indexHolder;

            try
            {
                await lockObject.WaitAsync();

                if (indices.TryGetValue(schemaId, out indexHolder))
                {
                    log.LogWarning(w => w
                        .WriteProperty("message", "Unreleased index found.")
                        .WriteProperty("schemaId", schemaId.ToString()));

                    await CommitInternalAsync(indexHolder, true);
                }

                indexHolder = new IndexHolder(schemaId);
                indices[schemaId] = indexHolder;
            }
            finally
            {
                lockObject.Release();
            }

            var directory = await directoryFactory.CreateDirectoryAsync(schemaId);

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

                await directoryFactory.WriteAsync(holder.GetUnsafeWriter().Directory, holder.Snapshotter);
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
