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
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable SA1114 // Parameter list should follow declaration
#pragma warning disable SA1115 // Parameter should follow comma
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public abstract class TextIndexerGrainTestsBase
    {
        private readonly InMemoryTextIndexerState state = new InMemoryTextIndexerState();
        private readonly List<Guid> ids1 = new List<Guid> { Guid.NewGuid() };
        private readonly List<Guid> ids2 = new List<Guid> { Guid.NewGuid() };
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly SearchContext context;

        public abstract IIndexStorage Storage { get; }

        protected TextIndexerGrainTestsBase()
        {
            context = new SearchContext
            {
                Languages = new HashSet<string> { "de", "en" }
            };
        }

        /*
        [Fact]
        public async Task Should_throw_exception_for_invalid_query()
        {
            await ExecuteAsync(Guid.NewGuid(), async sut =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.SearchAsync("~hello", context));
            });
        }*/

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "iv", "Hello"),
                g => CreateContent(g, ids2[0], "iv", "World"),

                g => TestSearchAsync(g, expected: ids1, text: "Hello"),
                g => TestSearchAsync(g, expected: ids2, text: "World"),

                g => TestSearchAsync(g, expected: null, text: "Hello", Scope.Published),
                g => TestSearchAsync(g, expected: null, text: "World", Scope.Published)
            );
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve_with_fuzzy()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "iv", "Hello"),
                g => CreateContent(g, ids2[0], "iv", "World"),

                g => TestSearchAsync(g, expected: ids1, text: "helo~"),
                g => TestSearchAsync(g, expected: ids2, text: "wold~", Scope.Draft)
            );
        }

        [Fact]
        public async Task Should_update_draft_only()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                g => TestSearchAsync(g, expected: null, text: "Morning", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Morning", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Evening", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Evening", target: Scope.Published)
            );
        }

        [Fact]
        public async Task Should_also_serve_published_after_publish()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                g => PublishAsync(g, ids1[0]),

                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Published)
            );
        }

        [Fact]
        public async Task Should_also_update_published_content()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                g => PublishAsync(g, ids1[0]),

                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                g => TestSearchAsync(g, expected: null, text: "Morning", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Morning", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Evening", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Evening", target: Scope.Published)
            );
        }

        [Fact]
        public async Task Should_simulate_new_version()
        {
            await SearchWithGrains(3,
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                // Publish the content.
                g => PublishAsync(g, ids1[0]),

                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Published),

                // Create a new version, the value is still the same as old version.
                g => CreateVersion(g, ids1[0]),

                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Published),

                // Make an update, this updates the new version only.
                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                g => TestSearchAsync(g, expected: null, text: "Morning", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Evening", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Evening", target: Scope.Published)
            );
        }

        [Fact]
        public async Task Should_simulate_content_reversion()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                // Publish the content.
                g => PublishAsync(g, ids1[0]),

                // Create a new version, the value is still the same as old version.
                g => CreateVersion(g, ids1[0]),

                // Make an update, this updates the new version only.
                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                // Make an update, this updates the new version only.
                g => DeleteVersionAsync(g, ids1[0]),

                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Morning", target: Scope.Published),

                g => TestSearchAsync(g, expected: null, text: "Evening", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Evening", target: Scope.Published),

                // Make an update, this updates the current version only.
                g => UpdateContent(g, ids1[0], "iv", "Night"),

                g => TestSearchAsync(g, expected: ids1, text: "Night", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Night", target: Scope.Published)
            );
        }

        /*
        [Fact]
        public async Task Should_also_retrieve_published_content_after_copy()
        {
            await SearchWithGrains(
                g => AddInvariantContent(g, text1: "Hello", text2: "World", onlyDraft: false),

                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Published),

                g => CopyAsync(g, fromDraft: true),

                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Published));
        }
                */

        [Fact]
        public async Task Should_delete_documents_from_index()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "iv", "Hello"),
                g => CreateContent(g, ids2[0], "iv", "World"),

                g => TestSearchAsync(g, expected: ids1, text: "Hello"),
                g => TestSearchAsync(g, expected: ids2, text: "World"),

                g => DeleteAsync(g, id: ids1[0]),

                g => TestSearchAsync(g, expected: null, text: "Hello"),
                g => TestSearchAsync(g, expected: ids2, text: "World")
            );
        }

        [Fact]
        public async Task Should_search_by_field()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "en", "City"),
                g => CreateContent(g, ids2[0], "de", "Stadt"),

                g => TestSearchAsync(g, expected: ids1, text: "en:city"),
                g => TestSearchAsync(g, expected: ids2, text: "de:Stadt")
            );
        }

        [Fact]
        public async Task Should_index_localized_content_and_retrieve()
        {
            await SearchWithGrains(
                g => CreateContent(g, ids1[0], "de", "Stadt und Land and Fluss"),

                g => CreateContent(g, ids2[0], "en", "City and Country und River"),

                g => TestSearchAsync(g, expected: ids1, text: "Stadt"),
                g => TestSearchAsync(g, expected: ids1, text: "and"),
                g => TestSearchAsync(g, expected: ids2, text: "und"),

                g => TestSearchAsync(g, expected: ids2, text: "City"),
                g => TestSearchAsync(g, expected: ids2, text: "und"),
                g => TestSearchAsync(g, expected: ids1, text: "and")
            );
        }

        private async Task CreateContent(GrainTextIndexer grain, Guid id, string language, string text)
        {
            var data =
                new NamedContentData()
                    .AddField("text",
                        new ContentFieldData()
                            .AddValue(language, text));

            var @event = new ContentCreated { ContentId = id, Data = data, SchemaId = schemaId };

            await grain.On(Envelope.Create(@event));
        }

        private async Task UpdateContent(GrainTextIndexer grain, Guid id, string language, string text)
        {
            var data =
                new NamedContentData()
                    .AddField("text",
                        new ContentFieldData()
                            .AddValue(language, text));

            var @event = new ContentUpdated { ContentId = id, Data = data, SchemaId = schemaId };

            await grain.On(Envelope.Create(@event));
        }

        private async Task CreateVersion(GrainTextIndexer grain, Guid id)
        {
            var @event = new ContentVersionCreated { ContentId = id, SchemaId = schemaId };

            await grain.On(Envelope.Create(@event));
        }

        private async Task PublishAsync(GrainTextIndexer grain, Guid id)
        {
            var @event = new ContentStatusChanged { ContentId = id, SchemaId = schemaId, Status = Status.Published };

            await grain.On(Envelope.Create(@event));
        }

        private async Task DeleteVersionAsync(GrainTextIndexer grain, Guid id)
        {
            var @event = new ContentVersionDeleted { ContentId = id, SchemaId = schemaId };

            await grain.On(Envelope.Create(@event));
        }

        private async Task DeleteAsync(GrainTextIndexer grain, Guid id)
        {
            var @event = new ContentDeleted { ContentId = id, SchemaId = schemaId };

            await grain.On(Envelope.Create(@event));
        }

        /*

        private async Task AddInvariantContent(TextIndexerGrain grain, string text1, string text2, bool onlyDraft = false)
        {
            var content1 = new Dictionary<string, string>
            {
                ["iv"] = text1
            };

            var content2 = new Dictionary<string, string>
            {
                ["iv"] = text2
            };

            await grain.IndexAsync(new Update { Id = ids1[0], Text = content1, OnlyDraft = onlyDraft });
            await grain.IndexAsync(new Update { Id = ids2[0], Text = content2, OnlyDraft = onlyDraft });
        }
        private async Task CopyAsync(TextIndexerGrain grain, bool fromDraft)
        {
            await grain.CopyAsync(ids1[0], fromDraft);
            await grain.CopyAsync(ids2[0], fromDraft);
        }
        */

        private async Task TestSearchAsync(GrainTextIndexer indexer, List<Guid>? expected, string text, Scope target = Scope.Draft)
        {
            var app =
                Mocks.App(NamedId.Of(Guid.NewGuid(), "my-app"),
                    Language.DE,
                    Language.EN);

            var result = await indexer.SearchAsync(text, app, schemaId.Id, target);

            if (expected != null)
            {
                Assert.Equal(expected, result);
            }
            else
            {
                Assert.Empty(result);
            }
        }

        private async Task SearchWithGrains(params Func<GrainTextIndexer, Task>[] actions)
        {
            for (var i = 0; i < actions.Length; i++)
            {
                await SearchWithGrains(i, actions);
            }
        }

        private async Task SearchWithGrains(int i, params Func<GrainTextIndexer, Task>[] actions)
        {
            await ExecuteAsync(async sut =>
            {
                foreach (var action in actions.Take(i))
                {
                    await action(sut);
                }
            });

            await ExecuteAsync(async sut =>
            {
                foreach (var action in actions.Skip(i))
                {
                    await action(sut);
                }
            });
        }

        private async Task ExecuteAsync(Func<GrainTextIndexer, Task> action)
        {
            var grain = new TextIndexerGrain(new IndexManager(Storage, A.Fake<ISemanticLog>()));
            try
            {
                await grain.ActivateAsync(schemaId.Id);

                var grainFactory = A.Fake<IGrainFactory>();

                A.CallTo(() => grainFactory.GetGrain<ITextIndexerGrain>(schemaId.Id, null))
                    .Returns(grain);

                var sut = new GrainTextIndexer(grainFactory, state);

                await action(sut);
            }
            finally
            {
                await grain.OnDeactivateAsync();
            }
        }
    }
}
