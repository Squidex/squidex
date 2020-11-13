// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public sealed class ContentEnricher : IContentEnricher
    {
        private readonly IEnumerable<IContentEnricherStep> steps;
        private readonly Lazy<IContentQueryService> contentQuery;

        private IContentQueryService ContentQuery
        {
            get { return contentQuery.Value; }
        }

        public ContentEnricher(IEnumerable<IContentEnricherStep> steps, Lazy<IContentQueryService> contentQuery)
        {
            Guard.NotNull(steps, nameof(steps));
            Guard.NotNull(contentQuery, nameof(contentQuery));

            this.steps = steps;

            this.contentQuery = contentQuery;
        }

        public async Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, bool cloneData, Context context)
        {
            Guard.NotNull(content, nameof(content));

            var enriched = await EnrichInternalAsync(Enumerable.Repeat(content, 1), cloneData, context);

            return enriched[0];
        }

        public Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, Context context)
        {
            Guard.NotNull(contents, nameof(contents));
            Guard.NotNull(context, nameof(context));

            return EnrichInternalAsync(contents, false, context);
        }

        private async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichInternalAsync(IEnumerable<IContentEntity> contents, bool cloneData, Context context)
        {
            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var results = new List<ContentEntity>();

                if (context.App != null)
                {
                    foreach (var step in steps)
                    {
                        await step.EnrichAsync(context);
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
                        var schemaCache = new Dictionary<DomainId, Task<ISchemaEntity>>();

                        Task<ISchemaEntity> GetSchema(DomainId id)
                        {
                            return schemaCache.GetOrAdd(id, x => ContentQuery.GetSchemaOrThrowAsync(context, x.ToString()));
                        }

                        foreach (var step in steps)
                        {
                            using (Profiler.TraceMethod(step.ToString()!))
                            {
                                await step.EnrichAsync(context, results, GetSchema);
                            }
                        }
                    }
                }

                return results;
            }
        }
    }
}
