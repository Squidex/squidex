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
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable SA1115 // Parameter should follow comma
#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public abstract class TextIndexerGrainTestsBase
    {
        private readonly List<Guid> ids1 = new List<Guid> { Guid.NewGuid() };
        private readonly List<Guid> ids2 = new List<Guid> { Guid.NewGuid() };
        private readonly SearchContext context;

        public abstract IIndexStorage Storage { get; }

        protected TextIndexerGrainTestsBase()
        {
            context = new SearchContext
            {
                Languages = new HashSet<string> { "de", "en" }
            };
        }

        [Fact]
        public async Task Should_throw_exception_for_invalid_query()
        {
            await ExecuteAsync(Guid.NewGuid(), async sut =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.SearchAsync("~hello", context));
            });
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve()
        {
            await SearchWithGrains(
                g => AddInvariantContent(g, "Hello", "World", false),

                g => TestSearchAsync(g, expected: ids1, text: "Hello"),
                g => TestSearchAsync(g, expected: ids2, text: "World"));
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve_with_fuzzy()
        {
            await SearchWithGrains(
                g => AddInvariantContent(g, "Hello", "World", false),

                g => TestSearchAsync(g, expected: ids1, text: "helo~"),
                g => TestSearchAsync(g, expected: ids2, text: "wold~"));
        }

        [Fact]
        public async Task Should_update_draft_only()
        {
            await SearchWithGrains(
                g => AddInvariantContent(g, text1: "Hello", text2: "World", onlyDraft: false),
                g => AddInvariantContent(g, text1: "Hallo", text2: "Welt", onlyDraft: false),

                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Hallo", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Hallo", target: Scope.Published));
        }

        [Fact]
        public async Task Should_also_update_published_after_copy()
        {
            await SearchWithGrains(
                g => AddInvariantContent(g, text1: "Hello", text2: "World", onlyDraft: false),

                g => CopyAsync(g, fromDraft: true),

                g => AddInvariantContent(g, text1: "Hallo", text2: "Welt", onlyDraft: false),

                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Hallo", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Hallo", target: Scope.Published));
        }

        [Fact]
        public async Task Should_also_update_published_after_copy_for_specific_step()
        {
            await SearchWithGrains(3,
                g => AddInvariantContent(g, text1: "Hello", text2: "World", onlyDraft: false),

                g => CopyAsync(g, fromDraft: true),

                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Draft),

                g => AddInvariantContent(g, text1: "Hallo", text2: "Welt", onlyDraft: false),

                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Hallo", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Hallo", target: Scope.Published));
        }

        [Fact]
        public async Task Should_simulate_content_reversion()
        {
            await SearchWithGrains(
                g => AddInvariantContent(g, text1: "Hello", text2: "World", onlyDraft: false),

                g => CopyAsync(g, fromDraft: true),

                g => AddInvariantContent(g, text1: "Hallo", text2: "Welt", onlyDraft: true),

                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Hallo", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Hallo", target: Scope.Published),

                g => CopyAsync(g, fromDraft: false),

                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Published),

                g => TestSearchAsync(g, expected: null, text: "Hallo", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Hallo", target: Scope.Published),

                g => AddInvariantContent(g, text1: "Guten Morgen", text2: "Welt", onlyDraft: true),

                g => TestSearchAsync(g, expected: null, text: "Hello", target: Scope.Draft),
                g => TestSearchAsync(g, expected: ids1, text: "Hello", target: Scope.Published),

                g => TestSearchAsync(g, expected: ids1, text: "Guten Morgen", target: Scope.Draft),
                g => TestSearchAsync(g, expected: null, text: "Guten Morgen", target: Scope.Published));
        }

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

        [Fact]
        public async Task Should_delete_documents_from_index()
        {
            await SearchWithGrains(
                g => AddInvariantContent(g, text1: "Hello", text2: "World", onlyDraft: false),

                g => TestSearchAsync(g, expected: ids1, text: "Hello"),
                g => TestSearchAsync(g, expected: ids2, text: "World"),

                g => DeleteAsync(g, id: ids1[0]),

                g => TestSearchAsync(g, expected: null, text: "Hello"),
                g => TestSearchAsync(g, expected: ids2, text: "World"));
        }

        [Fact]
        public async Task Should_search_by_field()
        {
            await SearchWithGrains(
                g => AddLocalizedContent(g),

                g => TestSearchAsync(g, expected: null, text: "de:city"),
                g => TestSearchAsync(g, expected: null, text: "en:Stadt"));
        }

        [Fact]
        public async Task Should_index_localized_content_and_retrieve()
        {
            await SearchWithGrains(
                g => AddLocalizedContent(g),

                g => TestSearchAsync(g, expected: ids1, text: "Stadt"),
                g => TestSearchAsync(g, expected: ids1, text: "and"),
                g => TestSearchAsync(g, expected: ids2, text: "und"),

                g => TestSearchAsync(g, expected: ids2, text: "City"),
                g => TestSearchAsync(g, expected: ids2, text: "und"),
                g => TestSearchAsync(g, expected: ids1, text: "and"));
        }

        private async Task AddLocalizedContent(TextIndexerGrain grain)
        {
            var germanText = new Dictionary<string, string>
            {
                ["de"] = "Stadt und Umgebung and whatever"
            };

            var englishText = new Dictionary<string, string>
            {
                ["en"] = "City and Surroundings und sonstiges"
            };

            await grain.IndexAsync(new Update { Id = ids1[0], Text = germanText, OnlyDraft = true });
            await grain.IndexAsync(new Update { Id = ids2[0], Text = englishText, OnlyDraft = true });
        }

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

        private async Task DeleteAsync(TextIndexerGrain grain, Guid id)
        {
            await grain.DeleteAsync(id);
        }

        private async Task CopyAsync(TextIndexerGrain grain, bool fromDraft)
        {
            await grain.CopyAsync(ids1[0], fromDraft);
            await grain.CopyAsync(ids2[0], fromDraft);
        }

        private async Task TestSearchAsync(TextIndexerGrain grain, List<Guid>? expected, string text, Scope target = Scope.Draft)
        {
            context.Scope = target;

            var result = await grain.SearchAsync(text, context);

            if (expected != null)
            {
                Assert.Equal(expected, result);
            }
            else
            {
                Assert.Empty(result);
            }
        }

        private async Task SearchWithGrains(params Func<TextIndexerGrain, Task>[] actions)
        {
            for (var i = 0; i < actions.Length; i++)
            {
                await SearchWithGrains(i, actions);
            }
        }

        private async Task SearchWithGrains(int i, params Func<TextIndexerGrain, Task>[] actions)
        {
            var schemaId = Guid.NewGuid();

            await ExecuteAsync(schemaId, async sut =>
            {
                foreach (var action in actions.Take(i))
                {
                    await action(sut);
                }
            });

            await ExecuteAsync(schemaId, async sut =>
            {
                foreach (var action in actions.Skip(i))
                {
                    await action(sut);
                }
            });
        }

        private async Task ExecuteAsync(Guid id, Func<TextIndexerGrain, Task> action)
        {
            var sut = new TextIndexerGrain(new IndexManager(Storage, A.Fake<ISemanticLog>()));
            try
            {
                await sut.ActivateAsync(id);

                await action(sut);
            }
            finally
            {
                await sut.OnDeactivateAsync();
            }
        }
    }
}
