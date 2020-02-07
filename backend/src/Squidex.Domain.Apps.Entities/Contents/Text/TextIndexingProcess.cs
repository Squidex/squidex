// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexingProcess : IEventConsumer
    {
        private const string NotFound = "<404>";
        private readonly ITextIndexer textIndexer;
        private readonly ITextIndexerState textIndexerState;

        public string Name
        {
            get { return "TextIndexer2"; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public ITextIndexer TextIndexer
        {
            get { return textIndexer; }
        }

        public TextIndexingProcess(ITextIndexer textIndexer, ITextIndexerState textIndexerState)
        {
            Guard.NotNull(textIndexer);
            Guard.NotNull(textIndexerState);

            this.textIndexer = textIndexer;
            this.textIndexerState = textIndexerState;
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
                case ContentDraftCreated deleteVersion:
                    await CreateDraftAsync(deleteVersion);
                    break;
                case ContentDraftDeleted deleteVersion:
                    await DeleteDraftAsync(deleteVersion);
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
                DocIdNew = null
            };

            var texts = @event.Data.ToTexts();

            await textIndexer.ExecuteAsync(@event.SchemaId.Id,
                new UpsertIndexEntry
                {
                    ContentId = @event.ContentId,
                    DocId = state.DocIdCurrent,
                    ServeAll = true,
                    ServePublished = false,
                    Texts = texts,
                });

            await textIndexerState.SetAsync(state);
        }

        private async Task CreateDraftAsync(ContentDraftCreated @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                state.DocIdNew = Guid.NewGuid().ToString();

                await textIndexerState.SetAsync(state);
            }
        }

        private async Task UpdateAsync(ContentUpdated @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                var texts = @event.Data.ToTexts();

                if (state.DocIdNew != null)
                {
                    await textIndexer.ExecuteAsync(@event.SchemaId.Id,
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

                    state.DocIdForPublished = state.DocIdCurrent;
                }
                else
                {
                    await textIndexer.ExecuteAsync(@event.SchemaId.Id,
                        new UpsertIndexEntry
                        {
                            ContentId = @event.ContentId,
                            DocId = state.DocIdCurrent,
                            ServeAll = true,
                            ServePublished = state.DocIdCurrent == state.DocIdForPublished,
                            Texts = texts
                        });

                    state.DocIdForPublished = state.DocIdNew;
                }

                await textIndexerState.SetAsync(state);
            }
        }

        private async Task PublishAsync(ContentStatusChanged @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                if (state.DocIdNew != null)
                {
                    await textIndexer.ExecuteAsync(@event.SchemaId.Id,
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

                    state.DocIdForPublished = state.DocIdNew;
                }
                else
                {
                    await textIndexer.ExecuteAsync(@event.SchemaId.Id,
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdCurrent,
                            ServeAll = true,
                            ServePublished = true
                        });

                    state.DocIdForPublished = state.DocIdCurrent;
                }

                state.DocIdNew = null;

                await textIndexerState.SetAsync(state);
            }
        }

        private async Task DeleteDraftAsync(ContentDraftDeleted @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                await textIndexer.ExecuteAsync(@event.SchemaId.Id,
                    new UpdateIndexEntry
                    {
                        DocId = state.DocIdCurrent,
                        ServeAll = true,
                        ServePublished = true
                    },
                    new DeleteIndexEntry
                    {
                        DocId = state.DocIdNew ?? NotFound,
                    });

                state.DocIdForPublished = state.DocIdCurrent;
                state.DocIdNew = null;

                await textIndexerState.SetAsync(state);
            }
        }

        private async Task DeleteAsync(ContentDeleted @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                await textIndexer.ExecuteAsync(@event.SchemaId.Id,
                    new DeleteIndexEntry
                    {
                        DocId = state.DocIdCurrent
                    },
                    new DeleteIndexEntry
                    {
                        DocId = state.DocIdNew ?? NotFound,
                    });

                await textIndexerState.RemoveAsync(state.ContentId);
            }
        }
    }
}
