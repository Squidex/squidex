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
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class GrainTextIndexer : ITextIndexer
    {
        private readonly IGrainFactory grainFactory;

        public GrainTextIndexer(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));

            this.grainFactory = grainFactory;
        }

        public Task DeleteAsync(Guid schemaId, Guid id)
        {
            var index = grainFactory.GetGrain<ITextIndexerGrain>(schemaId);

            return index.DeleteAsync(id);
        }

        public async Task IndexAsync(Guid schemaId, Guid id, NamedContentData data, NamedContentData dataDraft)
        {
            var index = grainFactory.GetGrain<ITextIndexerGrain>(schemaId);

            if (data != null)
            {
                await index.IndexAsync(id, new IndexData { });
            }

            if (dataDraft != null)
            {
                await index.IndexAsync(id, new IndexData { IsDraft = true });
            }
        }

        public async Task<List<Guid>> SearchAsync(string queryText, IAppEntity app, ISchemaEntity schema, bool useDraft = false)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return null;
            }

            var index = grainFactory.GetGrain<ITextIndexerGrain>(schema.Id);

            var languages = app.LanguagesConfig.Select(x => x.Key).ToList();

            var context = new SearchContext
            {
                AppVersion = app.Version,
                AppLanguages = languages,
                SchemaVersion = schema.Version,
                IsDraft = useDraft
            };

            return await index.SearchAsync(queryText, context);
        }
    }
}
