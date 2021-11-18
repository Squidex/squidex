// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public abstract class TextIndexerTestsBase
    {
        protected readonly List<DomainId> ids1 = new List<DomainId> { DomainId.NewGuid() };
        protected readonly List<DomainId> ids2 = new List<DomainId> { DomainId.NewGuid() };

        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly IAppEntity app;
        private readonly TextIndexingProcess sut;

        public virtual bool SupportsQuerySyntax => true;

        public virtual bool SupportsGeo => false;

        protected TextIndexerTestsBase()
        {
            app =
                Mocks.App(appId,
                    Language.DE,
                    Language.EN);

#pragma warning disable MA0056 // Do not call overridable members in constructor
            sut = new TextIndexingProcess(TestUtils.DefaultSerializer, CreateIndex(), new InMemoryTextIndexerState());
#pragma warning restore MA0056 // Do not call overridable members in constructor
        }

        public abstract ITextIndex CreateIndex();

        [SkippableFact]
        public async Task Should_index_invariant_content_and_retrieve_with_fuzzy()
        {
            Skip.IfNot(SupportsQuerySyntax);

            await CreateTextAsync(ids1[0], "iv", "Hello");

            await SearchText(expected: ids1, text: "helo~");
        }

        [SkippableFact]
        public async Task Should_index_invariant_content_and_retrieve_with_fuzzy_with_full_scope()
        {
            Skip.IfNot(SupportsQuerySyntax);

            await CreateTextAsync(ids2[0], "iv", "World");

            await SearchText(expected: ids2, text: "wold~", SearchScope.All);
        }

        [SkippableFact]
        public async Task Should_search_by_field()
        {
            Skip.IfNot(SupportsQuerySyntax);

            await CreateTextAsync(ids1[0], "en", "City");

            await SearchText(expected: ids1, text: "en:city");
        }

        [SkippableFact]
        public async Task Should_search_by_geo()
        {
            Skip.IfNot(SupportsGeo);

            // Within radius
            await CreateGeoAsync(ids1[0], "geo", 51.343391192211506, 12.401476788622826);

            // Not in radius
            await CreateGeoAsync(ids2[0], "geo", 51.30765141427311, 12.379631713912486);

            await SearchGeo(expected: ids1, "geo.iv", 51.34641682574934, 12.401965298137707);

            // Wrong field
            await SearchGeo(expected: null, "abc.iv", 51.48596429889613, 12.102629469505713);
        }

        [Fact]
        public async Task Should_index_invariant_content_and_retrieve()
        {
            await CreateTextAsync(ids1[0], "iv", "Hello");
            await CreateTextAsync(ids2[0], "iv", "World");

            await SearchText(expected: ids1, text: "Hello");
            await SearchText(expected: ids2, text: "World");

            await SearchText(expected: null, text: "Hello", SearchScope.Published);
            await SearchText(expected: null, text: "World", SearchScope.Published);
        }

        [Fact]
        public async Task Should_update_draft_only()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            await UpdateTextAsync(ids1[0], "iv", "V2");

            await SearchText(expected: null, text: "V1", target: SearchScope.All);
            await SearchText(expected: null, text: "V1", target: SearchScope.Published);

            await SearchText(expected: ids1, text: "V2", target: SearchScope.All);
            await SearchText(expected: null, text: "V2", target: SearchScope.Published);
        }

        [Fact]
        public async Task Should_update_draft_only_multiple_times()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            await UpdateTextAsync(ids1[0], "iv", "V2");
            await UpdateTextAsync(ids1[0], "iv", "V3");

            await SearchText(expected: null, text: "V2", target: SearchScope.All);
            await SearchText(expected: null, text: "V2", target: SearchScope.Published);

            await SearchText(expected: ids1, text: "V3", target: SearchScope.All);
            await SearchText(expected: null, text: "V3", target: SearchScope.Published);
        }

        [Fact]
        public async Task Should_also_serve_published_after_publish()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            await PublishAsync(ids1[0]);

            await SearchText(expected: ids1, text: "V1", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V1", target: SearchScope.Published);
        }

        [Fact]
        public async Task Should_also_update_published_content()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            await PublishAsync(ids1[0]);

            await UpdateTextAsync(ids1[0], "iv", "V2");

            await SearchText(expected: null, text: "V1", target: SearchScope.All);
            await SearchText(expected: null, text: "V1", target: SearchScope.Published);

            await SearchText(expected: ids1, text: "V2", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V2", target: SearchScope.Published);
        }

        [Fact]
        public async Task Should_also_update_published_content_multiple_times()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            await PublishAsync(ids1[0]);

            await UpdateTextAsync(ids1[0], "iv", "V2");
            await UpdateTextAsync(ids1[0], "iv", "V3");

            await SearchText(expected: null, text: "V2", target: SearchScope.All);
            await SearchText(expected: null, text: "V2", target: SearchScope.Published);

            await SearchText(expected: ids1, text: "V3", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V3", target: SearchScope.Published);
            }

        [Fact]
        public async Task Should_simulate_new_version()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            // Publish the content.
            await PublishAsync(ids1[0]);

            await SearchText(expected: ids1, text: "V1", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V1", target: SearchScope.Published);

            // Create a new version, the value is still the same as old version.
            await CreateDraftAsync(ids1[0]);

            await SearchText(expected: ids1, text: "V1", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V1", target: SearchScope.Published);

            // Make an update, this updates the new version only.
            await UpdateTextAsync(ids1[0], "iv", "V2");

            await SearchText(expected: null, text: "V1", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V1", target: SearchScope.Published);

            await SearchText(expected: ids1, text: "V2", target: SearchScope.All);
            await SearchText(expected: null, text: "V2", target: SearchScope.Published);

            // Publish the new version to get rid of the "V1" version.
            await PublishAsync(ids1[0]);

            await SearchText(expected: null, text: "V1", target: SearchScope.All);
            await SearchText(expected: null, text: "V1", target: SearchScope.Published);

            await SearchText(expected: ids1, text: "V2", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V2", target: SearchScope.Published);

            // Unpublish the version
            await UnpublishAsync(ids1[0]);

            await SearchText(expected: ids1, text: "V2", target: SearchScope.All);
            await SearchText(expected: null, text: "V2", target: SearchScope.Published);
        }

        [Fact]
        public async Task Should_simulate_new_version_with_migration()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            // Publish the content.
            await PublishAsync(ids1[0]);

            await SearchText(expected: ids1, text: "V1", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V1", target: SearchScope.Published);

            // Create a new version, his updates the new version also.
            await CreateDraftWithTextAsync(ids1[0], "iv", "V2");

            await SearchText(expected: null, text: "V1", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V1", target: SearchScope.Published);

            await SearchText(expected: ids1, text: "V2", target: SearchScope.All);
            await SearchText(expected: null, text: "V2", target: SearchScope.Published);
        }

        [Fact]
        public async Task Should_simulate_content_reversion()
        {
            await CreateTextAsync(ids1[0], "iv", "V1");

            // Publish the content.
            await PublishAsync(ids1[0]);

            // Create a new version, the value is still the same as old version.
            await CreateDraftAsync(ids1[0]);

            // Make an update, this updates the new version only.
            await UpdateTextAsync(ids1[0], "iv", "V2");

            // Make an update, this updates the new version only.
            await DeleteDraftAsync(ids1[0]);

            await SearchText(expected: ids1, text: "V1", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V1", target: SearchScope.Published);

            await SearchText(expected: null, text: "V2", target: SearchScope.All);
            await SearchText(expected: null, text: "V2", target: SearchScope.Published);

            // Make an update, this updates the current version only.
            await UpdateTextAsync(ids1[0], "iv", "V3");

            await SearchText(expected: ids1, text: "V3", target: SearchScope.All);
            await SearchText(expected: ids1, text: "V3", target: SearchScope.Published);
        }

        [Fact]
        public async Task Should_delete_documents_from_index()
        {
            await CreateTextAsync(ids1[0], "iv", "V1_1");
            await CreateTextAsync(ids2[0], "iv", "V2_1");

            await SearchText(expected: ids1, text: "V1_1");
            await SearchText(expected: ids2, text: "V2_1");

            await DeleteAsync(ids1[0]);

            await SearchText(expected: null, text: "V1_1");
            await SearchText(expected: ids2, text: "V2_1");
        }

        protected Task CreateTextAsync(DomainId id, string language, string text)
        {
            var data = TextData(language, text);

            return UpdateAsync(id, new ContentCreated { Data = data });
        }

        protected Task CreateGeoAsync(DomainId id, string field, double latitude, double longitude)
        {
            var data = GeoData(field, latitude, longitude);

            return UpdateAsync(id, new ContentCreated { Data = data });
        }

        protected Task UpdateTextAsync(DomainId id, string language, string text)
        {
            var data = TextData(language, text);

            return UpdateAsync(id, new ContentUpdated { Data = data });
        }

        protected Task CreateDraftWithTextAsync(DomainId id, string language, string text)
        {
            var data = TextData(language, text);

            return UpdateAsync(id, new ContentDraftCreated { MigratedData = data });
        }

        protected Task CreateDraftAsync(DomainId id)
        {
            return UpdateAsync(id, new ContentDraftCreated());
        }

        protected Task PublishAsync(DomainId id)
        {
            return UpdateAsync(id, new ContentStatusChanged { Status = Status.Published });
        }

        protected Task UnpublishAsync(DomainId id)
        {
            return UpdateAsync(id, new ContentStatusChanged { Status = Status.Draft });
        }

        protected Task DeleteDraftAsync(DomainId id)
        {
            return UpdateAsync(id, new ContentDraftDeleted());
        }

        protected Task DeleteAsync(DomainId id)
        {
            return UpdateAsync(id, new ContentDeleted());
        }

        private Task UpdateAsync(DomainId id, ContentEvent contentEvent)
        {
            contentEvent.ContentId = id;
            contentEvent.AppId = appId;
            contentEvent.SchemaId = schemaId;

            return sut.On(Enumerable.Repeat(Envelope.Create<IEvent>(contentEvent), 1));
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

        protected async Task SearchGeo(List<DomainId>? expected, string field, double latitude, double longitude, SearchScope target = SearchScope.All)
        {
            var query = new GeoQuery(schemaId.Id, field, latitude, longitude, 1000, 1000);

            var result = await sut.TextIndex.SearchAsync(app, query, target);

            if (expected != null)
            {
                result.Should().BeEquivalentTo(expected.ToHashSet());
            }
            else
            {
                result.Should().BeEmpty();
            }
        }

        protected async Task SearchText(List<DomainId>? expected, string text, SearchScope target = SearchScope.All)
        {
            var query = new TextQuery(text, 1000)
            {
                RequiredSchemaIds = new List<DomainId> { schemaId.Id }
            };

            var result = await sut.TextIndex.SearchAsync(app, query, target);

            if (expected != null)
            {
                result.Should().BeEquivalentTo(expected.ToHashSet());
            }
            else
            {
                result.Should().BeEmpty();
            }
        }
    }
}
