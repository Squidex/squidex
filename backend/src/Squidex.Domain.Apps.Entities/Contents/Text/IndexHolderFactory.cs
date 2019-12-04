// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class IndexHolderFactory : DisposableObjectBase
    {
        private readonly Dictionary<Guid, IndexHolder> indices = new Dictionary<Guid, IndexHolder>();
        private readonly SemaphoreSlim lockObject = new SemaphoreSlim(1);
        private readonly IDirectoryFactory directoryFactory;
        private readonly ISemanticLog log;

        public IndexHolderFactory(IDirectoryFactory directoryFactory, ISemanticLog log)
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
                try
                {
                    lockObject.Wait();

                    if (indices.Count > 0)
                    {
                        log.LogWarning(w => w
                            .WriteProperty("message", "Unreleased indices found.")
                            .WriteProperty("count", indices.Count));

                        foreach (var index in indices)
                        {
                            index.Value.Dispose();
                        }

                        indices.Clear();
                    }
                }
                finally
                {
                    lockObject.Release();
                }
            }
        }

        public async Task<IndexHolder> AcquireAsync(Guid schemaId)
        {
            IndexHolder? index;

            try
            {
                await lockObject.WaitAsync();

                if (indices.TryGetValue(schemaId, out index))
                {
                    log.LogWarning(w => w
                        .WriteProperty("message", "Unreleased index found.")
                        .WriteProperty("schemaId", schemaId.ToString()));

                    index.Dispose();
                }

                var directory = await directoryFactory.CreateAsync(schemaId);

                index = new IndexHolder(directory, directoryFactory);

                indices[schemaId] = index;
            }
            finally
            {
                lockObject.Release();
            }

            index.Open();

            return index;
        }

        public void Release(Guid id)
        {
            try
            {
                lockObject.Wait();

                indices.Remove(id);
            }
            finally
            {
                lockObject.Release();
            }
        }
    }
}
