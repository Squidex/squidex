// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable RECS0021 // Warns about calls to virtual member functions occuring in the constructor

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public abstract class TextIndexerGrainTestsBase : IDisposable
    {
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly List<Guid> ids1 = new List<Guid> { Guid.NewGuid() };
        private readonly List<Guid> ids2 = new List<Guid> { Guid.NewGuid() };
        private readonly SearchContext context;
        private readonly TextIndexerGrain sut;

        public abstract IIndexStorage Storage { get; }

        protected TextIndexerGrainTestsBase()
        {
            context = new SearchContext
            {
                Languages = new HashSet<string> { "de", "en" }
            };

            var factory = new IndexManager(Storage, A.Fake<ISemanticLog>());

            sut = new TextIndexerGrain(factory);
            sut.ActivateAsync(schemaId).Wait();
        }

        public void Dispose()
        {
            sut.OnDeactivateAsync().Wait();
        }

        [Fact]
        public async Task Should_throw_exception_for_invalid_query()
        {
            await Assert.ThrowsAsync<ValidationException>(() => sut.SearchAsync("~hello", context));
        }

        [Fact]
        public async Task Should_read_index_and_retrieve()
        {
            await AddInvariantContent("Hello", "World", false);

            await sut.OnDeactivateAsync();

            var other = new TextIndexerGrain(new IndexManager(Storage, A.Fake<ISemanticLog>()));
            try
            {
                await other.ActivateAsync(schemaId);

                await TestSearchAsync(ids1, "Hello", grain: other);
                await TestSearchAsync(ids2, "World", grain: other);
            }
            finally
            {
                await other.OnDeactivateAsync();
            }
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve()
        {
            await AddInvariantContent("Hello", "World", false);

            await TestSearchAsync(ids1, "Hello");
            await TestSearchAsync(ids2, "World");
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve_with_fuzzy()
        {
            await AddInvariantContent("Hello", "World", false);

            await TestSearchAsync(ids1, "helo~");
            await TestSearchAsync(ids2, "wold~");
        }

        [Fact]
        public async Task Should_update_draft_only()
        {
            await AddInvariantContent("Hello", "World", false);
            await AddInvariantContent("Hallo", "Welt", false);

            await TestSearchAsync(null, "Hello", Scope.Draft);
            await TestSearchAsync(null, "Hello", Scope.Published);

            await TestSearchAsync(ids1, "Hallo", Scope.Draft);
            await TestSearchAsync(null, "Hallo", Scope.Published);
        }

        [Fact]
        public async Task Should_also_update_published_after_copy()
        {
            await AddInvariantContent("Hello", "World", false);

            await CopyAsync(true);

            await AddInvariantContent("Hallo", "Welt", false);

            await TestSearchAsync(null, "Hello", Scope.Draft);
            await TestSearchAsync(null, "Hello", Scope.Published);

            await TestSearchAsync(ids1, "Hallo", Scope.Draft);
            await TestSearchAsync(ids1, "Hallo", Scope.Published);
        }

        [Fact]
        public async Task Should_simulate_content_reversion()
        {
            await AddInvariantContent("Hello", "World", false);

            await CopyAsync(true);

            await AddInvariantContent("Hallo", "Welt", true);

            await TestSearchAsync(null, "Hello", Scope.Draft);
            await TestSearchAsync(ids1, "Hello", Scope.Published);

            await TestSearchAsync(ids1, "Hallo", Scope.Draft);
            await TestSearchAsync(null, "Hallo", Scope.Published);

            await CopyAsync(false);

            await TestSearchAsync(ids1, "Hello", Scope.Draft);
            await TestSearchAsync(ids1, "Hello", Scope.Published);

            await TestSearchAsync(null, "Hallo", Scope.Draft);
            await TestSearchAsync(null, "Hallo", Scope.Published);

            await AddInvariantContent("Guten Morgen", "Welt", true);

            await TestSearchAsync(null, "Hello", Scope.Draft);
            await TestSearchAsync(ids1, "Hello", Scope.Published);

            await TestSearchAsync(ids1, "Guten Morgen", Scope.Draft);
            await TestSearchAsync(null, "Guten Morgen", Scope.Published);
        }

        [Fact]
        public async Task Should_also_retrieve_published_content_after_copy()
        {
            await AddInvariantContent("Hello", "World", false);

            await TestSearchAsync(ids1, "Hello", Scope.Draft);
            await TestSearchAsync(null, "Hello", Scope.Published);

            await CopyAsync(true);

            await TestSearchAsync(ids1, "Hello", Scope.Draft);
            await TestSearchAsync(ids1, "Hello", Scope.Published);
        }

        [Fact]
        public async Task Should_delete_documents_from_index()
        {
            await AddInvariantContent("Hello", "World", false);

            await TestSearchAsync(ids1, "Hello");
            await TestSearchAsync(ids2, "World");

            await DeleteAsync(ids1[0]);

            await TestSearchAsync(null, "Hello");
            await TestSearchAsync(ids2, "World");
        }

        [Fact]
        public async Task Should_search_by_field()
        {
            await AddLocalizedContent();

            await TestSearchAsync(null, "de:city");
            await TestSearchAsync(null, "en:Stadt");
        }

        [Fact]
        public async Task Should_index_localized_content_and_retrieve()
        {
            await AddLocalizedContent();

            await TestSearchAsync(ids1, "Stadt");
            await TestSearchAsync(ids1, "and");
            await TestSearchAsync(ids2, "und");

            await TestSearchAsync(ids2, "City");
            await TestSearchAsync(ids2, "und");
            await TestSearchAsync(ids1, "and");
        }

        private async Task AddLocalizedContent()
        {
            var germanText = new Dictionary<string, string>
            {
                ["de"] = "Stadt und Umgebung and whatever"
            };

            var englishText = new Dictionary<string, string>
            {
                ["en"] = "City and Surroundings und sonstiges"
            };

            await sut.IndexAsync(new Update { Id = ids1[0], Text = germanText, OnlyDraft = true });
            await sut.IndexAsync(new Update { Id = ids2[0], Text = englishText, OnlyDraft = true });
        }

        private async Task AddInvariantContent(string text1, string text2, bool onlyDraft = false)
        {
            var content1 = new Dictionary<string, string>
            {
                ["iv"] = text1
            };

            var content2 = new Dictionary<string, string>
            {
                ["iv"] = text2
            };

            await sut.IndexAsync(new Update { Id = ids1[0], Text = content1, OnlyDraft = onlyDraft });
            await sut.IndexAsync(new Update { Id = ids2[0], Text = content2, OnlyDraft = onlyDraft });
        }

        private async Task DeleteAsync(Guid id)
        {
            await sut.DeleteAsync(id);
        }

        private async Task CopyAsync(bool fromDraft)
        {
            await sut.CopyAsync(ids1[0], fromDraft);
            await sut.CopyAsync(ids2[0], fromDraft);
        }

        private async Task TestSearchAsync(List<Guid>? expected, string text, Scope target = Scope.Draft, TextIndexerGrain? grain = null)
        {
            context.Scope = target;

            var result = await (grain ?? sut).SearchAsync(text, context);

            if (expected != null)
            {
                Assert.Equal(expected, result);
            }
            else
            {
                Assert.Empty(result);
            }
        }
    }
}
