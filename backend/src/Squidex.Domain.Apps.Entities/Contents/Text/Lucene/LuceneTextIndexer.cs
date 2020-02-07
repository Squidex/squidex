// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Lucene
{
    public sealed class LuceneTextIndexer : ITextIndexer
    {
        private readonly IGrainFactory grainFactory;

        public LuceneTextIndexer(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory);

            this.grainFactory = grainFactory;
        }

        public async Task<List<Guid>?> SearchAsync(string? queryText, IAppEntity app, Guid schemaId, SearchScope scope = SearchScope.Published)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return null;
            }

            var index = grainFactory.GetGrain<ITextIndexerGrain>(schemaId);

            using (Profiler.TraceMethod<LuceneTextIndexer>())
            {
                var context = CreateContext(app, scope);

                return await index.SearchAsync(queryText, context);
            }
        }

        private static SearchContext CreateContext(IAppEntity app, SearchScope scope)
        {
            var languages = new HashSet<string>(app.LanguagesConfig.AllKeys);

            return new SearchContext { Languages = languages, Scope = scope };
        }

        public Task ExecuteAsync(Guid schemaId, params IIndexCommand[] commands)
        {
            var index = grainFactory.GetGrain<ITextIndexerGrain>(schemaId);

            return index.IndexAsync(commands.AsImmutable());
        }
    }
}
