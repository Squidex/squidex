// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public sealed class ContentEnricher(IEnumerable<IContentEnricherStep> steps, IAppProvider appProvider) : IContentEnricher
{
    public async Task<EnrichedContent> EnrichAsync(Content content, bool cloneData, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(content);

        var enriched = await EnrichInternalAsync(Enumerable.Repeat(content, 1), cloneData, context, ct);

        return enriched[0];
    }

    public Task<IReadOnlyList<EnrichedContent>> EnrichAsync(IEnumerable<Content> contents, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(contents);
        Guard.NotNull(context);

        return EnrichInternalAsync(contents, false, context, ct);
    }

    public async Task EnrichCachedAsync(IReadOnlyList<EnrichedContent> contents, Context context,
        CancellationToken ct)
    {
        Guard.NotNull(contents);
        Guard.NotNull(context);

        using (Telemetry.Activities.StartActivity("ContentEnricher/EnrichCachedAsync"))
        {
            await RunStepsAsync(steps.Where(x => x.RunOnCachedResults), contents, context, ct);
        }
    }

    private async Task<IReadOnlyList<EnrichedContent>> EnrichInternalAsync(IEnumerable<Content> contents, bool cloneData, Context context,
        CancellationToken ct)
    {
        using (var activity = Telemetry.Activities.StartActivity("ContentEnricher/EnrichInternalAsync"))
        {
            var results = new List<EnrichedContent>();

            foreach (var content in contents)
            {
                var result = SimpleMapper.Map(content, new EnrichedContent());

                // Clone the data to keep the existing value intact (for example when cached in memory).
                if (cloneData)
                {
                    using (Telemetry.Activities.StartActivity("ContentEnricher/CloneData"))
                    {
                        result.Data = result.Data.Clone();
                    }
                }

                results.Add(result);
            }

            await RunStepsAsync(steps, results, context, ct);
            activity?.SetTag("numItems", results.Count);

            return results;
        }
    }

    private async Task RunStepsAsync(IEnumerable<IContentEnricherStep> stepsToRun, IReadOnlyList<EnrichedContent> contents, Context context,
        CancellationToken ct)
    {
        if (context.App == null)
        {
            return;
        }

        foreach (var step in stepsToRun)
        {
            await step.EnrichAsync(context, ct);
        }

        if (contents.Count == 0)
        {
            return;
        }

        var schemaCache = new Dictionary<DomainId, Task<(Schema, ResolvedComponents)>>();

        Task<(Schema, ResolvedComponents)> GetSchema(DomainId id)
        {
            return schemaCache.GetOrAdd(id, async x =>
            {
                var schema = await appProvider.GetSchemaAsync(context.App.Id, x, false, ct)
                    ?? throw new DomainObjectNotFoundException(x.ToString());

                var components = await appProvider.GetComponentsAsync(schema, ct: ct);

                return (schema, components);
            });
        }

        foreach (var step in stepsToRun)
        {
            ct.ThrowIfCancellationRequested();

            using (Telemetry.Activities.StartActivity(step.ToString()!))
            {
                await step.EnrichAsync(context, contents, GetSchema, ct);
            }
        }
    }
}
