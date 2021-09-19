// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class TextIndexingProcess : IEventConsumer
    {
        private const string NotFound = "<404>";
        private readonly IJsonSerializer jsonSerializer;
        private readonly ITextIndex textIndex;
        private readonly ITextIndexerState textIndexerState;

        public int BatchSize
        {
            get => 1000;
        }

        public int BatchDelay
        {
            get => 1000;
        }

        public string Name
        {
            get => "TextIndexer5";
        }

        public string EventsFilter
        {
            get => "^content-";
        }

        public ITextIndex TextIndex
        {
            get => textIndex;
        }

        private sealed class Updates
        {
            private readonly Dictionary<DomainId, TextContentState> states;
            private readonly IJsonSerializer jsonSerializer;
            private readonly Dictionary<DomainId, TextContentState> updates = new Dictionary<DomainId, TextContentState>();
            private readonly Dictionary<string, IndexCommand> commands = new Dictionary<string, IndexCommand>();

            public Updates(Dictionary<DomainId, TextContentState> states, IJsonSerializer jsonSerializer)
            {
                this.states = states;

                this.jsonSerializer = jsonSerializer;
            }

            public async Task WriteAsync(ITextIndex textIndex, ITextIndexerState textIndexerState)
            {
                if (commands.Count > 0)
                {
                    await textIndex.ExecuteAsync(commands.Values.ToArray());
                }

                if (updates.Count > 0)
                {
                    await textIndexerState.SetAsync(updates.Values.ToList());
                }
            }

            public void On(Envelope<IEvent> @event)
            {
                switch (@event.Payload)
                {
                    case ContentCreated created:
                        Create(created, created.Data);
                        break;
                    case ContentUpdated updated:
                        Update(updated, updated.Data);
                        break;
                    case ContentStatusChanged statusChanged when statusChanged.Status == Status.Published:
                        Publish(statusChanged);
                        break;
                    case ContentStatusChanged statusChanged:
                        Unpublish(statusChanged);
                        break;
                    case ContentDraftDeleted draftDelted:
                        DeleteDraft(draftDelted);
                        break;
                    case ContentDeleted deleted:
                        Delete(deleted);
                        break;
                    case ContentDraftCreated draftCreated:
                        {
                            CreateDraft(draftCreated);

                            if (draftCreated.MigratedData != null)
                            {
                                Update(draftCreated, draftCreated.MigratedData);
                            }
                        }

                        break;
                }
            }

            private void Create(ContentEvent @event, ContentData data)
            {
                var uniqueId = DomainId.Combine(@event.AppId, @event.ContentId);

                var state = new TextContentState
                {
                    AppId = @event.AppId.Id,
                    UniqueContentId = uniqueId
                };

                state.GenerateDocIdCurrent();

                Index(@event,
                    new UpsertIndexEntry
                    {
                        ContentId = @event.ContentId,
                        DocId = state.DocIdCurrent,
                        GeoObjects = data.ToGeo(jsonSerializer),
                        ServeAll = true,
                        ServePublished = false,
                        Texts = data.ToTexts(),
                        IsNew = true
                    });

                states[state.UniqueContentId] = state;

                updates[state.UniqueContentId] = state;
            }

            private void CreateDraft(ContentEvent @event)
            {
                var uniqueId = DomainId.Combine(@event.AppId, @event.ContentId);

                if (states.TryGetValue(uniqueId, out var state))
                {
                    state.GenerateDocIdNew();

                    updates[state.UniqueContentId] = state;
                }
            }

            private void Unpublish(ContentEvent @event)
            {
                var uniqueId = DomainId.Combine(@event.AppId, @event.ContentId);

                if (states.TryGetValue(uniqueId, out var state) && state.DocIdForPublished != null)
                {
                    Index(@event,
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdForPublished,
                            ServeAll = true,
                            ServePublished = false
                        });

                    state.DocIdForPublished = null;

                    updates[state.UniqueContentId] = state;
                }
            }

            private void Update(ContentEvent @event, ContentData data)
            {
                var uniqueId = DomainId.Combine(@event.AppId, @event.ContentId);

                if (states.TryGetValue(uniqueId, out var state))
                {
                    if (state.DocIdNew != null)
                    {
                        Index(@event,
                            new UpsertIndexEntry
                            {
                                ContentId = @event.ContentId,
                                DocId = state.DocIdNew,
                                GeoObjects = data.ToGeo(jsonSerializer),
                                ServeAll = true,
                                ServePublished = false,
                                Texts = data.ToTexts()
                            });

                        Index(@event,
                            new UpdateIndexEntry
                            {
                                DocId = state.DocIdCurrent,
                                ServeAll = false,
                                ServePublished = true
                            });
                    }
                    else
                    {
                        var isPublished = state.DocIdCurrent == state.DocIdForPublished;

                        Index(@event,
                            new UpsertIndexEntry
                            {
                                ContentId = @event.ContentId,
                                DocId = state.DocIdCurrent,
                                GeoObjects = data.ToGeo(jsonSerializer),
                                ServeAll = true,
                                ServePublished = isPublished,
                                Texts = data.ToTexts()
                            });
                    }
                }
            }

            private void Publish(ContentEvent @event)
            {
                var uniqueId = DomainId.Combine(@event.AppId, @event.ContentId);

                if (states.TryGetValue(uniqueId, out var state))
                {
                    if (state.DocIdNew != null)
                    {
                        Index(@event,
                            new UpdateIndexEntry
                            {
                                DocId = state.DocIdNew,
                                ServeAll = true,
                                ServePublished = true
                            });

                        Index(@event,
                            new DeleteIndexEntry
                            {
                                DocId = state.DocIdCurrent
                            });

                        state.DocIdForPublished = state.DocIdNew;
                        state.DocIdCurrent = state.DocIdNew;
                    }
                    else
                    {
                        Index(@event,
                            new UpdateIndexEntry
                            {
                                DocId = state.DocIdCurrent,
                                ServeAll = true,
                                ServePublished = true
                            });

                        state.DocIdForPublished = state.DocIdCurrent;
                    }

                    state.DocIdNew = null;

                    updates[state.UniqueContentId] = state;
                }
            }

            private void DeleteDraft(ContentEvent @event)
            {
                var uniqueId = DomainId.Combine(@event.AppId, @event.ContentId);

                if (states.TryGetValue(uniqueId, out var state) && state.DocIdNew != null)
                {
                    Index(@event,
                        new UpdateIndexEntry
                        {
                            DocId = state.DocIdCurrent,
                            ServeAll = true,
                            ServePublished = true
                        });

                    Index(@event,
                        new DeleteIndexEntry
                        {
                            DocId = state.DocIdNew
                        });

                    state.DocIdNew = null;

                    updates[state.UniqueContentId] = state;
                }
            }

            private void Delete(ContentEvent @event)
            {
                var uniqueId = DomainId.Combine(@event.AppId, @event.ContentId);

                if (states.TryGetValue(uniqueId, out var state))
                {
                    Index(@event,
                        new DeleteIndexEntry
                        {
                            DocId = state.DocIdCurrent
                        });

                    Index(@event,
                        new DeleteIndexEntry
                        {
                            DocId = state.DocIdNew ?? NotFound
                        });

                    state.IsDeleted = true;

                    updates[state.UniqueContentId] = state;
                }
            }

            private void Index(ContentEvent @event, IndexCommand command)
            {
                command.AppId = @event.AppId;
                command.SchemaId = @event.SchemaId;

                if (command is UpdateIndexEntry update &&
                    commands.TryGetValue(command.DocId, out var existing) &&
                    existing is UpsertIndexEntry upsert)
                {
                    upsert.ServeAll = update.ServeAll;
                    upsert.ServePublished = update.ServePublished;
                }
                else
                {
                    commands[command.DocId] = command;
                }
            }
        }

        public TextIndexingProcess(
            IJsonSerializer jsonSerializer,
            ITextIndex textIndex,
            ITextIndexerState textIndexerState)
        {
            this.jsonSerializer = jsonSerializer;
            this.textIndex = textIndex;
            this.textIndexerState = textIndexerState;
        }

        public async Task ClearAsync()
        {
            await textIndex.ClearAsync();
            await textIndexerState.ClearAsync();
        }

        public async Task On(IEnumerable<Envelope<IEvent>> events)
        {
            var states = await QueryStatesAsync(events);

            var updates = new Updates(states, jsonSerializer);

            foreach (var @event in events)
            {
                updates.On(@event);
            }

            await updates.WriteAsync(textIndex, textIndexerState);
        }

        private Task<Dictionary<DomainId, TextContentState>> QueryStatesAsync(IEnumerable<Envelope<IEvent>> events)
        {
            var ids =
                events
                    .Select(x => x.Payload).OfType<ContentEvent>()
                    .Select(x => DomainId.Combine(x.AppId.Id, x.ContentId))
                    .ToHashSet();

            return textIndexerState.GetAsync(ids);
        }
    }
}
