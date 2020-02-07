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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable SA1114 // Parameter list should follow declaration
#pragma warning disable SA1115 // Parameter should follow comma
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public abstract class TextIndexerTestsBase
    {
        private readonly List<Guid> ids1 = new List<Guid> { Guid.NewGuid() };
        private readonly List<Guid> ids2 = new List<Guid> { Guid.NewGuid() };
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");

        public abstract IIndexerFactory Factory { get; }

        public virtual InMemoryTextIndexerState State { get; } = new InMemoryTextIndexerState();

        [Fact]
        public async Task Should_throw_exception_for_invalid_query()
        {
            await Assert.ThrowsAsync<ValidationException>(async () =>
            {
                await TestCombinations(g => Search(g, expected: null, text: "~hello"));
            });
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "iv", "Hello"),
                g => CreateContent(g, ids2[0], "iv", "World"),

                g => Search(g, expected: ids1, text: "Hello"),
                g => Search(g, expected: ids2, text: "World"),

                g => Search(g, expected: null, text: "Hello", SearchScope.Published),
                g => Search(g, expected: null, text: "World", SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve_with_fuzzy()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "iv", "Hello"),
                g => CreateContent(g, ids2[0], "iv", "World"),

                g => Search(g, expected: ids1, text: "helo~"),
                g => Search(g, expected: ids2, text: "wold~", SearchScope.All)
            );
        }

        [Fact]
        public async Task Should_update_draft_only()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                g => Search(g, expected: null, text: "Morning", target: SearchScope.All),
                g => Search(g, expected: null, text: "Morning", target: SearchScope.Published),

                g => Search(g, expected: ids1, text: "Evening", target: SearchScope.All),
                g => Search(g, expected: null, text: "Evening", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_also_serve_published_after_publish()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                g => PublishAsync(g, ids1[0]),

                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.All),
                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_also_update_published_content()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                g => PublishAsync(g, ids1[0]),

                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                g => Search(g, expected: null, text: "Morning", target: SearchScope.All),
                g => Search(g, expected: null, text: "Morning", target: SearchScope.Published),

                g => Search(g, expected: ids1, text: "Evening", target: SearchScope.All),
                g => Search(g, expected: ids1, text: "Evening", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_simulate_new_version()
        {
            await TestCombinations(3,
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                // Publish the content.
                g => PublishAsync(g, ids1[0]),

                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.All),
                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.Published),

                // Create a new version, the value is still the same as old version.
                g => CreateVersion(g, ids1[0]),

                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.All),
                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.Published),

                // Make an update, this updates the new version only.
                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                g => Search(g, expected: null, text: "Morning", target: SearchScope.All),
                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.Published),

                g => Search(g, expected: ids1, text: "Evening", target: SearchScope.All),
                g => Search(g, expected: null, text: "Evening", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_simulate_content_reversion()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "iv", "Morning"),

                // Publish the content.
                g => PublishAsync(g, ids1[0]),

                // Create a new version, the value is still the same as old version.
                g => CreateVersion(g, ids1[0]),

                // Make an update, this updates the new version only.
                g => UpdateContent(g, ids1[0], "iv", "Evening"),

                // Make an update, this updates the new version only.
                g => DeleteVersionAsync(g, ids1[0]),

                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.All),
                g => Search(g, expected: ids1, text: "Morning", target: SearchScope.Published),

                g => Search(g, expected: null, text: "Evening", target: SearchScope.All),
                g => Search(g, expected: null, text: "Evening", target: SearchScope.Published),

                // Make an update, this updates the current version only.
                g => UpdateContent(g, ids1[0], "iv", "Night"),

                g => Search(g, expected: ids1, text: "Night", target: SearchScope.All),
                g => Search(g, expected: ids1, text: "Night", target: SearchScope.Published)
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
            await TestCombinations(
                g => CreateContent(g, ids1[0], "iv", "Hello"),
                g => CreateContent(g, ids2[0], "iv", "World"),

                g => Search(g, expected: ids1, text: "Hello"),
                g => Search(g, expected: ids2, text: "World"),

                g => DeleteAsync(g, id: ids1[0]),

                g => Search(g, expected: null, text: "Hello"),
                g => Search(g, expected: ids2, text: "World")
            );
        }

        [Fact]
        public async Task Should_search_by_field()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "en", "City"),
                g => CreateContent(g, ids2[0], "de", "Stadt"),

                g => Search(g, expected: ids1, text: "en:city"),
                g => Search(g, expected: ids2, text: "de:Stadt")
            );
        }

        [Fact]
        public async Task Should_index_localized_content_and_retrieve()
        {
            await TestCombinations(
                g => CreateContent(g, ids1[0], "de", "Stadt und Land and Fluss"),

                g => CreateContent(g, ids2[0], "en", "City and Country und River"),

                g => Search(g, expected: ids1, text: "Stadt"),
                g => Search(g, expected: ids1, text: "and"),
                g => Search(g, expected: ids2, text: "und"),

                g => Search(g, expected: ids2, text: "City"),
                g => Search(g, expected: ids2, text: "und"),
                g => Search(g, expected: ids1, text: "and")
            );
        }

        private async Task CreateContent(TextIndexingProcess grain, Guid id, string language, string text)
        {
            var data =
                new NamedContentData()
                    .AddField("text",
                        new ContentFieldData()
                            .AddValue(language, text));

            var @event = new ContentCreated { ContentId = id, Data = data, SchemaId = schemaId };

            await grain.On(Envelope.Create(@event));
        }

        private async Task UpdateContent(TextIndexingProcess grain, Guid id, string language, string text)
        {
            var data =
                new NamedContentData()
                    .AddField("text",
                        new ContentFieldData()
                            .AddValue(language, text));

            var @event = new ContentUpdated { ContentId = id, Data = data, SchemaId = schemaId };

            await grain.On(Envelope.Create(@event));
        }

        private async Task CreateVersion(TextIndexingProcess process, Guid id)
        {
            var @event = new ContentDraftCreated { ContentId = id, SchemaId = schemaId };

            await process.On(Envelope.Create(@event));
        }

        private async Task PublishAsync(TextIndexingProcess process, Guid id)
        {
            var @event = new ContentStatusChanged { ContentId = id, SchemaId = schemaId, Status = Status.Published };

            await process.On(Envelope.Create(@event));
        }

        private async Task DeleteVersionAsync(TextIndexingProcess process, Guid id)
        {
            var @event = new ContentDraftDeleted { ContentId = id, SchemaId = schemaId };

            await process.On(Envelope.Create(@event));
        }

        private async Task DeleteAsync(TextIndexingProcess process, Guid id)
        {
            var @event = new ContentDeleted { ContentId = id, SchemaId = schemaId };

            await process.On(Envelope.Create(@event));
        }

        private async Task Search(TextIndexingProcess process, List<Guid>? expected, string text, SearchScope target = SearchScope.All)
        {
            var app =
                Mocks.App(NamedId.Of(Guid.NewGuid(), "my-app"),
                    Language.DE,
                    Language.EN);

            var result = await process.TextIndexer.SearchAsync(text, app, schemaId.Id, target);

            if (expected != null)
            {
                Assert.Equal(expected, result);
            }
            else
            {
                Assert.Empty(result);
            }
        }

        private async Task TestCombinations(params Func<TextIndexingProcess, Task>[] actions)
        {
            for (var i = 0; i < actions.Length; i++)
            {
                await TestCombinations(i, actions);
            }
        }

        private async Task TestCombinations(int i, params Func<TextIndexingProcess, Task>[] actions)
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

        private async Task ExecuteAsync(Func<TextIndexingProcess, Task> action)
        {
            var indexer = await Factory.CreateAsync(schemaId.Id);
            try
            {
                var sut = new TextIndexingProcess(indexer, State);

                await action(sut);
            }
            finally
            {
                await Factory.CleanupAsync();
            }
        }
    }
}
