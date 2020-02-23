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
    public sealed class LuceneTextIndex : ITextIndex
    {
        private readonly IGrainFactory grainFactory;
        private readonly IndexManager indexManager;

        public LuceneTextIndex(IGrainFactory grainFactory, IndexManager indexManager)
        {
            Guard.NotNull(grainFactory);
            Guard.NotNull(indexManager);

            this.grainFactory = grainFactory;

            this.indexManager = indexManager;
        }

        public Task ClearAsync()
        {
            return indexManager.ClearAsync();
        }

        public async Task<List<Guid>?> SearchAsync(string? queryText, IAppEntity app, SearchFilter? filter, SearchScope scope)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return null;
            }

            var index = grainFactory.GetGrain<ILuceneTextIndexGrain>(app.Id);

            using (Profiler.TraceMethod<LuceneTextIndex>())
            {
                var context = CreateContext(app, scope);

                return await index.SearchAsync(queryText, filter, context);
            }
        }

        private static SearchContext CreateContext(IAppEntity app, SearchScope scope)
        {
            var languages = new HashSet<string>(app.LanguagesConfig.AllKeys);

            return new SearchContext { Languages = languages, Scope = scope };
        }

        public Task ExecuteAsync(NamedId<Guid> appId, NamedId<Guid> schemaId, params IndexCommand[] commands)
        {
            var index = grainFactory.GetGrain<ILuceneTextIndexGrain>(appId.Id);

            return index.IndexAsync(schemaId, commands.AsImmutable());
        }
    }
}
