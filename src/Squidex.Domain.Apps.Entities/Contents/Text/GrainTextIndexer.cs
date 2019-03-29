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

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class GrainTextIndexer : ITextIndexer, IEventConsumer
    {
        private readonly RetryWindow retryWindow = new RetryWindow(TimeSpan.FromMinutes(5), 5);
        private readonly IGrainFactory grainFactory;
        private readonly ISemanticLog log;

        public string Name
        {
            get { return "TextIndexer"; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public GrainTextIndexer(IGrainFactory grainFactory, ISemanticLog log)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(log, nameof(log));

            this.grainFactory = grainFactory;

            this.log = log;
        }

        public bool Handles(StoredEvent @event)
        {
            return true;
        }

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public async Task IndexAsync(Guid schemaId, Guid id, NamedContentData dataDraft, NamedContentData data)
        {
            var index = grainFactory.GetGrain<ITextIndexerGrain>(schemaId);

            using (Profiler.TraceMethod<GrainTextIndexer>())
            {
                try
                {
                    await index.IndexAsync(id, new IndexData { DataDraft = dataDraft, Data = data }, false);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "UpdateTextEntry")
                        .WriteProperty("status", "Failed"));
                }
            }
        }

        public async Task On(Envelope<IEvent> @event)
        {
            try
            {
                if (@event.Payload is ContentEvent contentEvent)
                {
                    var index = grainFactory.GetGrain<ITextIndexerGrain>(contentEvent.SchemaId.Id);

                    var id = contentEvent.ContentId;

                    switch (@event.Payload)
                    {
                        case ContentDeleted contentDeleted:
                            await index.DeleteAsync(id);
                            break;
                        case ContentCreated contentCreated:
                            await index.IndexAsync(id, Data(contentCreated.Data), true);
                            break;
                        case ContentUpdateProposed contentCreated:
                            await index.IndexAsync(id, Data(contentCreated.Data), true);
                            break;
                        case ContentUpdated contentUpdated:
                            await index.IndexAsync(id, Data(contentUpdated.Data), false);
                            break;
                        case ContentChangesDiscarded contentChangesDiscarded:
                            await index.CopyAsync(id, false);
                            break;
                        case ContentChangesPublished contentChangesPublished:
                        case ContentStatusChanged contentStatusChanged when contentStatusChanged.Status == Status.Published:
                            await index.CopyAsync(id, true);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (retryWindow.CanRetryAfterFailure())
                {
                    log.LogError(ex, w => w
                        .WriteProperty("action", "DeleteTextEntry")
                        .WriteProperty("status", "Failed"));
                }
                else
                {
                    throw;
                }
            }
        }

        private J<IndexData> Data(NamedContentData data)
        {
            return new IndexData { DataDraft = data };
        }

        public async Task<List<Guid>> SearchAsync(string queryText, IAppEntity app, Guid schemaId, Scope scope = Scope.Published)
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
