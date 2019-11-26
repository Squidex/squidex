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
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class GrainTextIndexer : ITextIndexer, IEventConsumer
    {
        private readonly IGrainFactory grainFactory;

        public string Name
        {
            get { return "TextIndexer"; }
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
                        await index.IndexAsync(GetUpdate(id, contentCreated.Data));
                        break;
                    case ContentUpdated contentUpdated:
                        await index.IndexAsync(GetUpdate(id, contentUpdated.Data));
                        break;
                    case ContentChangesPublished _:
                    case ContentStatusChanged contentStatusChanged when contentStatusChanged.Status == Status.Published:
                        await index.CopyAsync(id, true);
                        break;
                }
            }
        }

        private static J<Update> GetUpdate(Guid contentId, NamedContentData data)
        {
            return new Update { Id = contentId, Data = data };
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
