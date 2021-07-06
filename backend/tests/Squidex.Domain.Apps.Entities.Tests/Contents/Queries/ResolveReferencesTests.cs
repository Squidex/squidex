// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ResolveReferencesTests : IClassFixture<TranslationsFixture>
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly NamedId<DomainId> refSchemaId1 = NamedId.Of(DomainId.NewGuid(), "my-ref1");
        private readonly NamedId<DomainId> refSchemaId2 = NamedId.Of(DomainId.NewGuid(), "my-ref2");
        private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        private readonly ProvideSchema schemaProvider;
        private readonly Context requestContext;
        private readonly ResolveReferences sut;

        public ResolveReferencesTests()
        {
            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId, Language.DE));

            var refSchemaDef =
                new Schema("my-ref")
                    .AddString(1, "name", Partitioning.Invariant,
                        new StringFieldProperties())
                    .AddNumber(2, "number", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .SetFieldsInReferences("name", "number");

            var schemaDef =
                new Schema(schemaId.Name)
                    .AddReferences(1, "ref1", Partitioning.Invariant, new ReferencesFieldProperties
                    {
                        ResolveReference = true,
                        MinItems = 1,
                        MaxItems = 1,
                        SchemaId = refSchemaId1.Id
                    })
                    .AddReferences(2, "ref2", Partitioning.Invariant, new ReferencesFieldProperties
                    {
                        ResolveReference = true,
                        MinItems = 1,
                        MaxItems = 1,
                        SchemaId = refSchemaId2.Id
                    })
                    .SetFieldsInLists("ref1", "ref2");

            schemaProvider = x =>
            {
                if (x == schemaId.Id)
                {
                    return Task.FromResult((Mocks.Schema(appId, schemaId, schemaDef), ResolvedComponents.Empty));
                }
                else if (x == refSchemaId1.Id)
                {
                    return Task.FromResult((Mocks.Schema(appId, refSchemaId1, refSchemaDef), ResolvedComponents.Empty));
                }
                else if (x == refSchemaId2.Id)
                {
                    return Task.FromResult((Mocks.Schema(appId, refSchemaId2, refSchemaDef), ResolvedComponents.Empty));
                }
                else
                {
                    throw new DomainObjectNotFoundException(x.ToString());
                }
            };

            sut = new ResolveReferences(new Lazy<IContentQueryService>(() => contentQuery), requestCache);
        }

        [Fact]
        public async Task Should_add_referenced_id_and_as_dependency()
        {
            var ref1_1 = CreateRefContent(DomainId.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(DomainId.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(DomainId.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(DomainId.NewGuid(), 4, "ref2_2", 29, refSchemaId2);

            var contents = new[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.ShouldSkipContentEnrichment() && x.ShouldSkipTotal()), A<Q>.That.HasIds(ref1_1.Id, ref1_2.Id, ref2_1.Id, ref2_2.Id), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

            A.CallTo(() => requestCache.AddDependency(DomainId.Combine(appId, refSchemaId1.Id), 0))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(DomainId.Combine(appId, refSchemaId2.Id), 0))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref1_1.UniqueId, ref1_1.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref2_1.UniqueId, ref2_1.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref1_2.UniqueId, ref1_2.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref2_2.UniqueId, ref2_2.Version))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_with_reference_data()
        {
            var ref1_1 = CreateRefContent(DomainId.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(DomainId.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(DomainId.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(DomainId.NewGuid(), 3, "ref2_2", 29, refSchemaId2);

            var contents = new[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.ShouldSkipContentEnrichment() && x.ShouldSkipTotal()), A<Q>.That.HasIds(ref1_1.Id, ref1_2.Id, ref2_1.Id, ref2_2.Id), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

            Assert.Equal(
                new ContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "ref1_1, 13")
                                    .Add("de", "ref1_1, 13")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "ref2_1, 23")
                                    .Add("de", "ref2_1, 23"))),
                contents[0].ReferenceData);

            Assert.Equal(
                new ContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "ref1_2, 17")
                                    .Add("de", "ref1_2, 17")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "ref2_2, 29")
                                    .Add("de", "ref2_2, 29"))),
                contents[1].ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_if_content_has_more_items()
        {
            var ref1_1 = CreateRefContent(DomainId.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(DomainId.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(DomainId.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(DomainId.NewGuid(), 4, "ref2_2", 29, refSchemaId2);

            var contents = new[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id, ref2_2.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_1.Id, ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.ShouldSkipContentEnrichment() && x.ShouldSkipTotal()), A<Q>.That.HasIds(ref1_1.Id, ref1_2.Id, ref2_1.Id, ref2_2.Id), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

            Assert.Equal(
                new ContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "ref1_1, 13")
                                    .Add("de", "ref1_1, 13")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "2 Reference(s)")
                                    .Add("de", "2 Reference(s)"))),
                contents[0].ReferenceData);

            Assert.Equal(
                new ContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "ref1_2, 17")
                                    .Add("de", "ref1_2, 17")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddInvariant(
                                JsonValue.Object()
                                    .Add("en", "2 Reference(s)")
                                    .Add("de", "2 Reference(s)"))),
                contents[1].ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_references_if_not_api_user()
        {
            var contents = new[]
            {
                CreateContent(new[] { DomainId.NewGuid() }, Array.Empty<DomainId>())
            };

            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            await sut.EnrichAsync(ctx, contents, schemaProvider, default);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_references_if_disabled()
        {
            var contents = new[]
            {
                CreateContent(new[] { DomainId.NewGuid() }, Array.Empty<DomainId>())
            };

            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId)).Clone(b => b.WithoutContentEnrichment(true));

            await sut.EnrichAsync(ctx, contents, schemaProvider, default);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_no_references_found()
        {
            var contents = new[]
            {
                CreateContent(Array.Empty<DomainId>(), Array.Empty<DomainId>())
            };

            await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

            Assert.NotNull(contents[0].ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        private ContentEntity CreateContent(DomainId[] ref1, DomainId[] ref2)
        {
            return new ContentEntity
            {
                Id = DomainId.NewGuid(),
                Data =
                    new ContentData()
                        .AddField("ref1",
                            new ContentFieldData()
                                .AddInvariant(JsonValue.Array(ref1.Select(x => x.ToString()))))
                        .AddField("ref2",
                            new ContentFieldData()
                                .AddInvariant(JsonValue.Array(ref2.Select(x => x.ToString())))),
                SchemaId = schemaId, AppId = appId,
                Version = 0
            };
        }

        private IEnrichedContentEntity CreateRefContent(DomainId id, int version, string name, int number, NamedId<DomainId> refSchemaId)
        {
            return new ContentEntity
            {
                Id = id,
                Data =
                    new ContentData()
                        .AddField("name",
                            new ContentFieldData()
                                .AddInvariant(name))
                        .AddField("number",
                            new ContentFieldData()
                                .AddInvariant(number)),
                SchemaId = refSchemaId, AppId = appId,
                Version = version
            };
        }
    }
}
