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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentEnricher : IContentEnricher
    {
        private readonly IEnumerable<IContentEnricherStep> steps;
        private readonly IAppProvider appProvider;

        public ContentEnricher(IEnumerable<IContentEnricherStep> steps, IAppProvider appProvider)
        {
            this.steps = steps;

            this.appProvider = appProvider;
        }

        public async Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, bool cloneData, Context context,
            CancellationToken ct)
        {
            Guard.NotNull(content, nameof(content));

            var enriched = await EnrichInternalAsync(Enumerable.Repeat(content, 1), cloneData, context, ct);

            return enriched[0];
        }

        public Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, Context context,
            CancellationToken ct)
        {
            Guard.NotNull(contents, nameof(contents));
            Guard.NotNull(context, nameof(context));

            return EnrichInternalAsync(contents, false, context, ct);
        }

        private async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichInternalAsync(IEnumerable<IContentEntity> contents, bool cloneData, Context context,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("ContentEnricher/EnrichInternalAsync"))
            {
                var results = new List<ContentEntity>();

                if (context.App != null)
                {
                    foreach (var step in steps)
                    {
                        await step.EnrichAsync(context, ct);
                    }
                }

                if (contents.Any())
                {
                    foreach (var content in contents)
                    {
                        var result = SimpleMapper.Map(content, new ContentEntity());

                        if (cloneData)
                        {
                            result.Data = result.Data.Clone();
                        }

                        results.Add(result);
                    }

                    if (context.App != null)
                    {
                        var schemaCache = new Dictionary<DomainId, Task<(ISchemaEntity, ResolvedComponents)>>();

                        Task<(ISchemaEntity, ResolvedComponents)> GetSchema(DomainId id)
                        {
                            return schemaCache.GetOrAdd(id, async x =>
                            {
                                var schema = await appProvider.GetSchemaAsync(context.App.Id, x, false, ct);

                                if (schema == null)
                                {
                                    throw new DomainObjectNotFoundException(x.ToString());
                                }

                                var components = await appProvider.GetComponentsAsync(schema, ct);

                                return (schema, components);
                            });
                        }

                        foreach (var step in steps)
                        {
                            ct.ThrowIfCancellationRequested();

                            using (Telemetry.Activities.StartActivity(step.ToString()!))
                            {
                                await step.EnrichAsync(context, results, GetSchema, ct);
                            }
                        }
                    }
                }

                return results;
            }
        }
    }
}
