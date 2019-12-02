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
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class GrainTextIndexer : ITextIndexer, IEventConsumer
    {
        private readonly IGrainFactory grainFactory;

        public string Name
        {
            get { return "TextIndexer2"; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public GrainTextIndexer(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory);

            this.grainFactory = grainFactory;
        }

        public bool Handles(StoredEvent @event)
        {
            return true;
        }

        public Task ClearAsync()
        {
            return TaskHelper.Done;
        }

        public async Task On(Envelope<IEvent> @event)
        {
            if (@event.Payload is ContentEvent contentEvent)
            {
                var index = grainFactory.GetGrain<ITextIndexerGrain>(contentEvent.SchemaId.Id);

                var id = contentEvent.ContentId;

                switch (@event.Payload)
                {
                    case ContentDeleted _:
                        await index.DeleteAsync(id);
                        break;
                    case ContentCreated contentCreated:
                        await index.IndexAsync(Data(id, contentCreated.Data, true));
                        break;
                    case ContentUpdateProposed contentUpdateProposed:
                        await index.IndexAsync(Data(id, contentUpdateProposed.Data, true));
                        break;
                    case ContentUpdated contentUpdated:
                        await index.IndexAsync(Data(id, contentUpdated.Data, false));
                        break;
                    case ContentChangesDiscarded _:
                        await index.CopyAsync(id, false);
                        break;
                    case ContentChangesPublished _:
                        await index.CopyAsync(id, true);
                        break;
                    case ContentStatusChanged contentStatusChanged when contentStatusChanged.Status == Status.Published:
                        await index.CopyAsync(id, true);
                        break;
                }
            }
        }

        private static Update Data(Guid contentId, NamedContentData data, bool onlyDraft)
        {
            var text = new TextContent(data);

            return new Update { Id = contentId, Text = text, OnlyDraft = onlyDraft };
        }

        public async Task<List<Guid>?> SearchAsync(string? queryText, IAppEntity app, Guid schemaId, Scope scope = Scope.Published)
        {
            if (string.IsNullOrWhiteSpace(queryText))
            {
                return null;
            }

            var index = grainFactory.GetGrain<ITextIndexerGrain>(schemaId);

            using (Profiler.TraceMethod<GrainTextIndexer>())
            {
                var context = CreateContext(app, scope);

                return await index.SearchAsync(queryText, context);
            }
        }

        private static SearchContext CreateContext(IAppEntity app, Scope scope)
        {
            var languages = new HashSet<string>(app.LanguagesConfig.Select(x => x.Key));

            return new SearchContext { Languages = languages, Scope = scope };
        }
    }
}
