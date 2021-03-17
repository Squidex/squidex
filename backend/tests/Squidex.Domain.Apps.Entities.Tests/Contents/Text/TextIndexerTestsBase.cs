// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Text.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1115 // Parameter should follow comma

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public abstract class TextIndexerTestsBase
    {
        protected readonly List<DomainId> ids1 = new List<DomainId> { DomainId.NewGuid() };
        protected readonly List<DomainId> ids2 = new List<DomainId> { DomainId.NewGuid() };

        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly IAppEntity app;

        protected delegate Task IndexOperation(TextIndexingProcess process);

        public abstract IIndexerFactory Factory { get; }

        public virtual bool SupportsCleanup { get; set; } = false;

        public virtual bool SupportsQuerySyntax { get; set; } = true;

        public virtual bool SupportsGeo { get; set; } = false;

        public virtual InMemoryTextIndexerState State { get; } = new InMemoryTextIndexerState();

        protected TextIndexerTestsBase()
        {
            app =
                Mocks.App(appId,
                    Language.DE,
                    Language.EN);
        }

        [SkippableFact]
        public async Task Should_index_invariant_content_and_retrieve_with_fuzzy()
        {
            Skip.IfNot(SupportsQuerySyntax);

            await TestCombinations(
                CreateText(ids1[0], "iv", "Hello"),
                CreateText(ids2[0], "iv", "World"),

                SearchText(expected: ids1, text: "helo~"),
                SearchText(expected: ids2, text: "wold~", SearchScope.All)
            );
        }

        [SkippableFact]
        public async Task Should_search_by_field()
        {
            Skip.IfNot(SupportsQuerySyntax);

            await TestCombinations(
                CreateText(ids1[0], "en", "City"),
                CreateText(ids2[0], "de", "Stadt"),

                SearchText(expected: ids1, text: "en:city"),
                SearchText(expected: ids2, text: "de:Stadt")
            );
        }

        [SkippableFact]
        public async Task Should_search_by_geo()
        {
            Skip.IfNot(SupportsGeo);

            await TestCombinations(
                CreateGeo(ids1[0], "geo", 51.343391192211506, 12.401476788622826), // Within radius
                CreateGeo(ids2[0], "geo", 51.30765141427311, 12.379631713912486),  // Not in radius

                SearchGeo(expected: ids1, "geo.iv", 51.34641682574934, 12.401965298137707),
                SearchGeo(expected: null, "abc.iv", 51.48596429889613, 12.102629469505713) // Wrong field
            );
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "Hello"),
                CreateText(ids2[0], "iv", "World"),

                SearchText(expected: ids1, text: "Hello"),
                SearchText(expected: ids2, text: "World"),

                SearchText(expected: null, text: "Hello", SearchScope.Published),
                SearchText(expected: null, text: "World", SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_update_draft_only()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "V1"),

                UpdateText(ids1[0], "iv", "V2"),

                SearchText(expected: null, text: "V1", target: SearchScope.All),
                SearchText(expected: null, text: "V1", target: SearchScope.Published),

                SearchText(expected: ids1, text: "V2", target: SearchScope.All),
                SearchText(expected: null, text: "V2", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_update_draft_only_multiple_times()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "V1"),

                UpdateText(ids1[0], "iv", "V2"),
                UpdateText(ids1[0], "iv", "V3"),

                SearchText(expected: null, text: "V2", target: SearchScope.All),
                SearchText(expected: null, text: "V2", target: SearchScope.Published),

                SearchText(expected: ids1, text: "V3", target: SearchScope.All),
                SearchText(expected: null, text: "V3", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_also_serve_published_after_publish()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "V1"),

                Publish(ids1[0]),

                SearchText(expected: ids1, text: "V1", target: SearchScope.All),
                SearchText(expected: ids1, text: "V1", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_also_update_published_content()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "V1"),

                Publish(ids1[0]),

                UpdateText(ids1[0], "iv", "V2"),

                SearchText(expected: null, text: "V1", target: SearchScope.All),
                SearchText(expected: null, text: "V1", target: SearchScope.Published),

                SearchText(expected: ids1, text: "V2", target: SearchScope.All),
                SearchText(expected: ids1, text: "V2", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_also_update_published_content_multiple_times()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "V1"),

                Publish(ids1[0]),

                UpdateText(ids1[0], "iv", "V2"),
                UpdateText(ids1[0], "iv", "V3"),

                SearchText(expected: null, text: "V2", target: SearchScope.All),
                SearchText(expected: null, text: "V2", target: SearchScope.Published),

                SearchText(expected: ids1, text: "V3", target: SearchScope.All),
                SearchText(expected: ids1, text: "V3", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_simulate_new_version()
        {
            await TestCombinations(0,
                CreateText(ids1[0], "iv", "V1"),

                // Publish the content.
                Publish(ids1[0]),

                SearchText(expected: ids1, text: "V1", target: SearchScope.All),
                SearchText(expected: ids1, text: "V1", target: SearchScope.Published),

                // Create a new version, the value is still the same as old version.
                CreateDraft(ids1[0]),

                SearchText(expected: ids1, text: "V1", target: SearchScope.All),
                SearchText(expected: ids1, text: "V1", target: SearchScope.Published),

                // Make an update, this updates the new version only.
                UpdateText(ids1[0], "iv", "V2"),

                SearchText(expected: null, text: "V1", target: SearchScope.All),
                SearchText(expected: ids1, text: "V1", target: SearchScope.Published),

                SearchText(expected: ids1, text: "V2", target: SearchScope.All),
                SearchText(expected: null, text: "V2", target: SearchScope.Published),

                // Publish the new version to get rid of the "V1" version.
                Publish(ids1[0]),

                SearchText(expected: null, text: "V1", target: SearchScope.All),
                SearchText(expected: null, text: "V1", target: SearchScope.Published),

                SearchText(expected: ids1, text: "V2", target: SearchScope.All),
                SearchText(expected: ids1, text: "V2", target: SearchScope.Published),

                // Unpublish the version
                Unpublish(ids1[0]),

                SearchText(expected: ids1, text: "V2", target: SearchScope.All),
                SearchText(expected: null, text: "V2", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_simulate_new_version_with_migration()
        {
            await TestCombinations(0,
                CreateText(ids1[0], "iv", "V1"),

                // Publish the content.
                Publish(ids1[0]),

                SearchText(expected: ids1, text: "V1", target: SearchScope.All),
                SearchText(expected: ids1, text: "V1", target: SearchScope.Published),

                // Create a new version, his updates the new version also.
                CreateDraftWithText(ids1[0], "iv", "V2"),

                SearchText(expected: null, text: "V1", target: SearchScope.All),
                SearchText(expected: ids1, text: "V1", target: SearchScope.Published),

                SearchText(expected: ids1, text: "V2", target: SearchScope.All),
                SearchText(expected: null, text: "V2", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_simulate_content_reversion()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "V1"),

                // Publish the content.
                Publish(ids1[0]),

                // Create a new version, the value is still the same as old version.
                CreateDraft(ids1[0]),

                // Make an update, this updates the new version only.
                UpdateText(ids1[0], "iv", "V2"),

                // Make an update, this updates the new version only.
                DeleteDraft(ids1[0]),

                SearchText(expected: ids1, text: "V1", target: SearchScope.All),
                SearchText(expected: ids1, text: "V1", target: SearchScope.Published),

                SearchText(expected: null, text: "V2", target: SearchScope.All),
                SearchText(expected: null, text: "V2", target: SearchScope.Published),

                // Make an update, this updates the current version only.
                UpdateText(ids1[0], "iv", "V3"),

                SearchText(expected: ids1, text: "V3", target: SearchScope.All),
                SearchText(expected: ids1, text: "V3", target: SearchScope.Published)
            );
        }

        [Fact]
        public async Task Should_delete_documents_from_index()
        {
            await TestCombinations(
                CreateText(ids1[0], "iv", "V1_1"),
                CreateText(ids2[0], "iv", "V2_1"),

                SearchText(expected: ids1, text: "V1_1"),
                SearchText(expected: ids2, text: "V2_1"),

                Delete(ids1[0]),

                SearchText(expected: null, text: "V1_1"),
                SearchText(expected: ids2, text: "V2_1")
            );
        }

        protected IndexOperation CreateText(DomainId id, string language, string text)
        {
            var data = TextData(language, text);

            return Op(id, new ContentCreated { Data = data });
        }

        protected IndexOperation CreateGeo(DomainId id, string field, double latitude, double longitude)
        {
            var data = GeoData(field, latitude, longitude);

            return Op(id, new ContentCreated { Data = data });
        }

        protected IndexOperation UpdateText(DomainId id, string language, string text)
        {
            var data = TextData(language, text);

            return Op(id, new ContentUpdated { Data = data });
        }

        protected IndexOperation CreateDraftWithText(DomainId id, string language, string text)
        {
            var data = TextData(language, text);

            return Op(id, new ContentDraftCreated { MigratedData = data });
        }

        private static ContentData TextData(string language, string text)
        {
            return new ContentData()
                .AddField("text",
                    new ContentFieldData()
                        .AddLocalized(language, text));
        }

        private static ContentData GeoData(string field, double latitude, double longitude)
        {
            return new ContentData()
                .AddField(field,
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Object().Add("latitude", latitude).Add("longitude", longitude)));
        }

        protected IndexOperation CreateDraft(DomainId id)
        {
            return Op(id, new ContentDraftCreated());
        }

        protected IndexOperation Publish(DomainId id)
        {
            return Op(id, new ContentStatusChanged { Status = Status.Published });
        }

        protected IndexOperation Unpublish(DomainId id)
        {
            return Op(id, new ContentStatusChanged { Status = Status.Draft });
        }

        protected IndexOperation DeleteDraft(DomainId id)
        {
            return Op(id, new ContentDraftDeleted());
        }

        protected IndexOperation Delete(DomainId id)
        {
            return Op(id, new ContentDeleted());
        }

        private IndexOperation Op(DomainId id, ContentEvent contentEvent)
        {
            contentEvent.ContentId = id;
            contentEvent.AppId = appId;
            contentEvent.SchemaId = schemaId;

            return p => p.On(Enumerable.Repeat(Envelope.Create<IEvent>(contentEvent), 1));
        }

        protected IndexOperation SearchGeo(List<DomainId>? expected, string field, double latitude, double longitude, SearchScope target = SearchScope.All)
        {
            return async p =>
            {
                var query = new GeoQuery(schemaId.Id, field, latitude, longitude, 1000);

                var result = await p.TextIndex.SearchAsync(app, query, target);

                if (expected != null)
                {
                    result.Should().BeEquivalentTo(expected.ToHashSet());
                }
                else
                {
                    result.Should().BeEmpty();
                }
            };
        }

        protected IndexOperation SearchText(List<DomainId>? expected, string text, SearchScope target = SearchScope.All)
        {
            return async p =>
            {
                var query = new TextQuery(text, TextFilter.ShouldHaveSchemas(schemaId.Id));

                var result = await p.TextIndex.SearchAsync(app, query, target);

                if (expected != null)
                {
                    result.Should().BeEquivalentTo(expected.ToHashSet());
                }
                else
                {
                    result.Should().BeEmpty();
                }
            };
        }

        protected async Task TestCombinations(params IndexOperation[] actions)
        {
            if (SupportsCleanup)
            {
                for (var i = 0; i < actions.Length; i++)
                {
                    await TestCombinations(i, actions);
                }
            }
            else
            {
                await TestCombinations(0, actions);
            }
        }

        protected async Task TestCombinations(int firstSteps, params IndexOperation[] actions)
        {
            await ExecuteAsync(async sut =>
            {
                foreach (var action in actions.Take(firstSteps))
                {
                    await action(sut);
                }
            });

            await ExecuteAsync(async sut =>
            {
                foreach (var action in actions.Skip(firstSteps))
                {
                    await action(sut);
                }
            });
        }

        private async Task ExecuteAsync(IndexOperation action)
        {
            var indexer = await Factory.CreateAsync(schemaId.Id);
            try
            {
                var sut = new TextIndexingProcess(TestUtils.DefaultSerializer, indexer, State);

                await action(sut);
            }
            finally
            {
                await Factory.CleanupAsync();
            }
        }
    }
}
