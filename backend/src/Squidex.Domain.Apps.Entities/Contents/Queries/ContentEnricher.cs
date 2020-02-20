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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

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
            Guard.NotNull(steps);
            Guard.NotNull(contentQuery);

            this.steps = steps;

            this.contentQuery = contentQuery;
        }

        public async Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, Context context)
        {
            Guard.NotNull(content);

            var enriched = await EnrichAsync(Enumerable.Repeat(content, 1), context);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, Context context)
        {
            Guard.NotNull(contents);
            Guard.NotNull(context);

            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var results = new List<ContentEntity>();

                foreach (var step in steps)
                {
                    await step.EnrichAsync(context);
                }

                if (contents.Any())
                {
                    foreach (var content in contents)
                    {
                        var result = SimpleMapper.Map(content, new ContentEntity());

                        results.Add(result);
                    }

                    var schemaCache = new Dictionary<Guid, Task<ISchemaEntity>>();

                    Task<ISchemaEntity> GetSchema(Guid id)
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

                return results;
            }
        }
    }
}
