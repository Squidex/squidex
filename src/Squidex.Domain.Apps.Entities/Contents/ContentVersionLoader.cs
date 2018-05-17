// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentVersionLoader : IContentVersionLoader
    {
        private readonly IStore<Guid> store;
        private readonly FieldRegistry registry;

        public ContentVersionLoader(IStore<Guid> store, FieldRegistry registry)
        {
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(registry, nameof(registry));

            this.store = store;

            this.registry = registry;
        }

        public async Task<IContentEntity> LoadAsync(Guid id, long version)
        {
            using (Profiler.TraceMethod<ContentVersionLoader>())
            {
                var content = new ContentState();

                var persistence = store.WithEventSourcing<ContentGrain, Guid>(id, e =>
                {
                    if (content.Version < version)
                    {
                        content = content.Apply(e);
                        content.Version++;
                    }
                });

                await persistence.ReadAsync();

                if (content.Version != version)
                {
                    throw new DomainObjectNotFoundException(id.ToString(), typeof(IContentEntity));
                }

                return content;
            }
        }
    }
}
