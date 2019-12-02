// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class IndexHolderFactory : DisposableObjectBase
    {
        private readonly Dictionary<Guid, IndexHolder> indices = new Dictionary<Guid, IndexHolder>();
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
                lock (indices)
                {
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
            }
        }

        public IndexHolder Acquire(Guid schemaId)
        {
            IndexHolder? index;

            lock (indices)
            {
                if (indices.TryGetValue(schemaId, out index))
                {
                    log.LogWarning(w => w
                        .WriteProperty("message", "Unreleased index found.")
                        .WriteProperty("schemaId", schemaId.ToString()));

                    index.Dispose();
                }

                index = new IndexHolder(directoryFactory, schemaId);

                indices[schemaId] = index;
            }

            index.Open();

            return index;
        }

        public void Release(Guid id)
        {
            lock (indices)
            {
                indices.Remove(id);
            }
        }
    }
}
