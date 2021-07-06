// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps
{
    public sealed class EnrichForCaching : IContentEnricherStep
    {
        private readonly IRequestCache requestCache;

        public EnrichForCaching(IRequestCache requestCache)
        {
            this.requestCache = requestCache;
        }

        public Task EnrichAsync(Context context,
            CancellationToken ct)
        {
            context.AddCacheHeaders(requestCache);

            return Task.CompletedTask;
        }

        public async Task EnrichAsync(Context context, IEnumerable<ContentEntity> contents, ProvideSchema schemas,
            CancellationToken ct)
        {
            var app = context.App;

            foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
            {
                ct.ThrowIfCancellationRequested();

                var (schema, _) = await schemas(group.Key);

                foreach (var content in group)
                {
                    requestCache.AddDependency(content.UniqueId, content.Version);
                    requestCache.AddDependency(schema.UniqueId, schema.Version);
                    requestCache.AddDependency(app.UniqueId, app.Version);
                }
            }
        }
    }
}
