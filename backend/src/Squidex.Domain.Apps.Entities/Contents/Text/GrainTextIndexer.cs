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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
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
        private readonly ITextIndexerState indexState;

        public string Name
        {
            get { return "TextIndexer2"; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public GrainTextIndexer(IGrainFactory grainFactory, ITextIndexerState indexState)
        {
            Guard.NotNull(grainFactory);
            Guard.NotNull(indexState);

            this.grainFactory = grainFactory;

            this.indexState = indexState;
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
            switch (@event.Payload)
            {
                case ContentCreated contentCreated:
                    await CreateAsync(contentCreated);
                    break;
                case ContentUpdated contentUpdated:
                    await UpdateAsync(contentUpdated);
                    break;
                case ContentStatusChanged contentStatusChanged when contentStatusChanged.Status == Status.Published:
                    await PublishAsync(contentStatusChanged);
                    break;
                case ContentVersionCreated deleteVersion:
                    await CreateNewVersionAsync(deleteVersion);
                    break;
                case ContentVersionDeleted deleteVersion:
                    await DeleteVersionAsync(deleteVersion);
                    break;
                case ContentDeleted contentDeleted:
                    await DeleteAsync(contentDeleted);
                    break;
            }
        }

        private async Task CreateAsync(ContentCreated @event)
        {
            var state = new ContentState
            {
                ContentId = @event.ContentId,
                DocIdCurrent = Guid.NewGuid().ToString(),
                DocIdNew = Guid.NewGuid().ToString()
            };

            var texts = @event.Data.ToTexts();

            await IndexAsync(@event.SchemaId.Id,
                new UpsertIndexEntry
                {
                    ContentId = @event.ContentId,
                    DocId = state.DocIdCurrent,
                    ServeAll = true,
                    ServePublished = false,
                    Texts = texts,
                },
                new UpsertIndexEntry
                {
                    ContentId = @event.ContentId,
                    DocId = state.DocIdNew,
                    ServeAll = false,
                    ServePublished = false,
                    Texts = texts,
                });

            state.DocIdForAll = state.DocIdCurrent;

            await indexState.SetAsync(state);
        }

        private async Task UpdateAsync<TCommand>(TCommand @event, Func<ContentState, TCommand, Task> updater) where TCommand : ContentEvent
        {
            var state = await indexState.GetAsync(@event.ContentId);

            if (state != null)
            {
                await updater(state, @event);

                await indexState.SetAsync(state);
            }
        }

        private Task CreateNewVersionAsync(ContentVersionCreated @event)
        {
            return UpdateAsync(@event, (state, e) =>
            {
                state.HasNew = true;

                return TaskHelper.Done;
            });
        }

        private Task UpdateAsync(ContentUpdated @event)
        {
            return UpdateAsync(@event, async (state, e) =>
            {
                var texts = e.Data.ToTexts();

                if (state.HasNew)
                {
                    await IndexAsync(e.SchemaId.Id,
                        new UpsertIndexEntry
                        {
                            ContentId = @event.ContentId,
                            DocId = state.DocIdNew,
                            ServeAll = true,
                            ServePublished = false,
                            Texts = texts
                        },
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdCurrent,
                            ServeAll = false,
                            ServePublished = true
                        });

                    state.DocIdForAll = state.DocIdNew;
                    state.DocIdForPublished = state.DocIdCurrent;
                }
                else
                {
                    await IndexAsync(@event.SchemaId.Id,
                        new UpsertIndexEntry
                        {
                            ContentId = @event.ContentId,
                            DocId = state.DocIdCurrent,
                            ServeAll = true,
                            ServePublished = state.DocIdCurrent == state.DocIdForPublished,
                            Texts = texts
                        },
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdNew,
                            ServeAll = false,
                            ServePublished = false
                        });

                    state.DocIdForAll = state.DocIdCurrent;
                    state.DocIdForPublished = state.DocIdNew;
                }
            });
        }

        private Task PublishAsync(ContentStatusChanged @event)
        {
            return UpdateAsync(@event, async (state, e) =>
            {
                if (state.HasNew)
                {
                    await IndexAsync(e.SchemaId.Id,
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdCurrent,
                            ServeAll = false,
                            ServePublished = false
                        },
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdNew,
                            ServeAll = true,
                            ServePublished = true
                        });

                    state.DocIdForAll = state.DocIdNew;
                    state.DocIdForPublished = state.DocIdNew;
                }
                else
                {
                    await IndexAsync(e.SchemaId.Id,
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdCurrent,
                            ServeAll = true,
                            ServePublished = true
                        },
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdNew,
                            ServeAll = false,
                            ServePublished = false
                        });

                    state.DocIdForAll = state.DocIdCurrent;
                    state.DocIdForPublished = state.DocIdCurrent;
                }

                state.HasNew = false;
            });
        }

        private Task DeleteVersionAsync(ContentVersionDeleted @event)
        {
            return UpdateAsync(@event, async (state, e) =>
            {
                await IndexAsync(e.SchemaId.Id,
                    new UpdateIndexEntry
                    {
                        DocId = state.DocIdCurrent,
                        ServeAll = true,
                        ServePublished = true
                    },
                    new UpdateIndexEntry
                    {
                        DocId = state.DocIdNew,
                        ServeAll = false,
                        ServePublished = false
                    });

                state.DocIdForAll = state.DocIdCurrent;
                state.DocIdForPublished = state.DocIdCurrent;

                state.HasNew = false;
            });
        }

        private async Task DeleteAsync(ContentDeleted @event)
        {
            var state = await indexState.GetAsync(@event.ContentId);

            if (state != null)
            {
                await IndexAsync(@event.SchemaId.Id,
                    new DeleteIndexEntry
                    {
                        ContentId = @event.ContentId
                    });

                await indexState.RemoveAsync(state.ContentId);
            }
        }

        private async Task IndexAsync(Guid schemaId, params IIndexCommand[] commands)
        {
            var index = grainFactory.GetGrain<ITextIndexerGrain>(schemaId);

            await index.IndexAsync(commands.AsImmutable());
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
            var languages = new HashSet<string>(app.LanguagesConfig.AllKeys);

            return new SearchContext { Languages = languages, Scope = scope };
        }
    }
}
