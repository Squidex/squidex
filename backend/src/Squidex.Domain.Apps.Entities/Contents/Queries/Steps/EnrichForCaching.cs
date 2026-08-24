// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

public sealed class EnrichForCaching(IRequestCache requestCache) : IContentEnricherStep
{
    public Task EnrichAsync(Context context,
        CancellationToken ct)
    {
        // Sometimes we just want to skip this for performance reasons.
        if (!ShouldEnrich(context))
        {
            return Task.CompletedTask;
        }

        context.AddCacheHeaders(requestCache);

        return Task.CompletedTask;
    }

    public async Task EnrichAsync(Context context, IEnumerable<EnrichedContent> contents, ProvideSchema schemas,
        CancellationToken ct)
    {
        // Sometimes we just want to skip this for performance reasons.
        if (!ShouldEnrich(context))
        {
            return;
        }

        var app = context.App;

        // Group by schema, so we only fetch the schema once.
        foreach (var group in contents.GroupBy(x => x.SchemaId.Id))
        {
            ct.ThrowIfCancellationRequested();

            var (schema, _) = await schemas(group.Key);

            // The app and the schema are the same for all contents of the group, so they are added
            // once per group and not once per content. They are added inside the loop, so that a
            // result without contents also has no dependencies and therefore no etag, as before.
            requestCache.AddDependency(app.UniqueId, app.Version);
            requestCache.AddDependency(schema.UniqueId, schema.Version);

            foreach (var content in group)
            {
                requestCache.AddDependency(content.UniqueId, content.Version);
            }
        }
    }

    private static bool ShouldEnrich(Context context)
    {
        return !context.NoCacheKeys();
    }
}
