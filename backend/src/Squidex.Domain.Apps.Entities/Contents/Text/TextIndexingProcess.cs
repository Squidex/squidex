// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexingProcess : IEventConsumer
    {
        private const string NotFound = "<404>";
        private readonly ITextIndex textIndexer;
        private readonly ITextIndexerState textIndexerState;

        public string Name
        {
            get { return "TextIndexer4"; }
        }

        public string EventsFilter
        {
            get { return "^content-"; }
        }

        public ITextIndex TextIndexer
        {
            get { return textIndexer; }
        }

        public TextIndexingProcess(ITextIndex textIndexer, ITextIndexerState textIndexerState)
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

        public async Task ClearAsync()
        {
            await textIndexer.ClearAsync();
            await textIndexerState.ClearAsync();
        }

        public async Task On(Envelope<IEvent> @event)
        {
            switch (@event.Payload)
            {
                case ContentCreated created:
                    await CreateAsync(created);
                    break;
                case ContentUpdated updated:
                    await UpdateAsync(updated);
                    break;
                case ContentStatusChanged statusChanged when statusChanged.Status == Status.Published:
                    await PublishAsync(statusChanged);
                    break;
                case ContentStatusChanged statusChanged:
                    await UnpublishAsync(statusChanged);
                    break;
                case ContentDraftCreated draftCreated:
                    await CreateDraftAsync(draftCreated);
                    break;
                case ContentDraftDeleted draftDelted:
                    await DeleteDraftAsync(draftDelted);
                    break;
                case ContentDeleted deleted:
                    await DeleteAsync(deleted);
                    break;
            }
        }

        private async Task CreateAsync(ContentCreated @event)
        {
            var state = new TextContentState
            {
                ContentId = @event.ContentId
            };

            state.GenerateDocIdCurrent();

            await IndexAsync(@event,
                new UpsertIndexEntry
                {
                    ContentId = @event.ContentId,
                    DocId = state.DocIdCurrent,
                    ServeAll = true,
                    ServePublished = false,
                    Texts = @event.Data.ToTexts(),
                });

            await textIndexerState.SetAsync(state);
        }

        private async Task CreateDraftAsync(ContentDraftCreated @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                state.GenerateDocIdNew();

                await textIndexerState.SetAsync(state);
            }
        }

        private async Task UpdateAsync(ContentUpdated @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                if (state.DocIdNew != null)
                {
                    await IndexAsync(@event,
                        new UpsertIndexEntry
                        {
                            ContentId = @event.ContentId,
                            DocId = state.DocIdNew,
                            ServeAll = true,
                            ServePublished = false,
                            Texts = @event.Data.ToTexts()
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
                    var isPublished = state.DocIdCurrent == state.DocIdForPublished;

                    await IndexAsync(@event,
                        new UpsertIndexEntry
                        {
                            ContentId = @event.ContentId,
                            DocId = state.DocIdCurrent,
                            ServeAll = true,
                            ServePublished = isPublished,
                            Texts = @event.Data.ToTexts()
                        });

                    state.DocIdForPublished = state.DocIdNew;
                }

                await textIndexerState.SetAsync(state);
            }
        }

        private async Task UnpublishAsync(ContentStatusChanged @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null && state.DocIdForPublished != null)
            {
                await IndexAsync(@event,
                    new UpdateIndexEntry
                    {
                        DocId = state.DocIdForPublished,
                        ServeAll = true,
                        ServePublished = false
                    });

                state.DocIdForPublished = null;

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
                    await IndexAsync(@event,
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdNew,
                            ServeAll = true,
                            ServePublished = true
                        },
                        new DeleteIndexEntry
                        {
                            DocId = state.DocIdCurrent
                        });

                    state.DocIdForPublished = state.DocIdNew;
                    state.DocIdCurrent = state.DocIdNew;
                }
                else
                {
                    await IndexAsync(@event,
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

            if (state != null && state.DocIdNew != null)
            {
                await IndexAsync(@event,
                    new UpdateIndexEntry
                    {
                        DocId = state.DocIdCurrent,
                        ServeAll = true,
                        ServePublished = true
                    },
                    new DeleteIndexEntry
                    {
                        DocId = state.DocIdNew,
                    });

                state.DocIdNew = null;

                await textIndexerState.SetAsync(state);
            }
        }

        private async Task DeleteAsync(ContentDeleted @event)
        {
            var state = await textIndexerState.GetAsync(@event.ContentId);

            if (state != null)
            {
                await IndexAsync(@event,
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

        private Task IndexAsync(ContentEvent @event, params IndexCommand[] commands)
        {
            return textIndexer.ExecuteAsync(@event.AppId, @event.SchemaId, commands);
        }
    }
}
