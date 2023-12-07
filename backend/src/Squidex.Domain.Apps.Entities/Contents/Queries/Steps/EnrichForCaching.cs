// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Contents.Queries.Steps;

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

            foreach (var content in group)
            {
                requestCache.AddDependency(content.UniqueId, content.Version);
                requestCache.AddDependency(schema.UniqueId, schema.Version);
                requestCache.AddDependency(app.UniqueId, app.Version);
            }
        }
    }

    private static bool ShouldEnrich(Context context)
    {
        return !context.NoCacheKeys();
    }
}
