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
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentEnricherReferencesTests
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetUrlGenerator assetUrlGenerator = A.Fake<IAssetUrlGenerator>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> refSchemaId1 = NamedId.Of(Guid.NewGuid(), "my-ref1");
        private readonly NamedId<Guid> refSchemaId2 = NamedId.Of(Guid.NewGuid(), "my-ref2");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly Context requestContext;
        private readonly ContentEnricher sut;

        public ContentEnricherReferencesTests()
        {
            requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId, Language.DE));

            var refSchemaDef =
                new Schema("my-ref")
                    .AddString(1, "name", Partitioning.Invariant,
                        new StringFieldProperties())
                    .AddNumber(2, "number", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .ConfigureFieldsInReferences("name", "number");

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
                    .ConfigureFieldsInLists("ref1", "ref2");

            void SetupSchema(NamedId<Guid> id, Schema def)
            {
                var schemaEntity = Mocks.Schema(appId, id, def);

                A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(requestContext, id.Id.ToString()))
                    .Returns(schemaEntity);
            }

            SetupSchema(schemaId, schemaDef);
            SetupSchema(refSchemaId1, refSchemaDef);
            SetupSchema(refSchemaId2, refSchemaDef);

            sut = new ContentEnricher(assetQuery, assetUrlGenerator, new Lazy<IContentQueryService>(() => contentQuery), contentWorkflow);
        }

        [Fact]
        public async Task Should_add_referenced_id_and__as_dependency()
        {
            var ref1_1 = CreateRefContent(Guid.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(Guid.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(Guid.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(Guid.NewGuid(), 4, "ref2_2", 29, refSchemaId2);

            var source = new IContentEntity[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<IReadOnlyList<Guid>>.That.Matches(x => x.Count == 4)))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            var enriched = await sut.EnrichAsync(source, requestContext);

            var enriched1 = enriched.ElementAt(0);

            Assert.Contains(refSchemaId1.Id, enriched1.CacheDependencies);
            Assert.Contains(refSchemaId2.Id, enriched1.CacheDependencies);

            Assert.Contains(ref1_1.Id, enriched1.CacheDependencies);
            Assert.Contains(ref1_1.Version, enriched1.CacheDependencies);

            Assert.Contains(ref2_1.Id, enriched1.CacheDependencies);
            Assert.Contains(ref2_1.Version, enriched1.CacheDependencies);

            var enriched2 = enriched.ElementAt(1);

            Assert.Contains(refSchemaId1.Id, enriched2.CacheDependencies);
            Assert.Contains(refSchemaId2.Id, enriched2.CacheDependencies);

            Assert.Contains(ref1_2.Id, enriched2.CacheDependencies);
            Assert.Contains(ref1_2.Version, enriched2.CacheDependencies);

            Assert.Contains(ref2_2.Id, enriched2.CacheDependencies);
            Assert.Contains(ref2_2.Version, enriched2.CacheDependencies);
        }

        [Fact]
        public async Task Should_enrich_with_reference_data()
        {
            var ref1_1 = CreateRefContent(Guid.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(Guid.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(Guid.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(Guid.NewGuid(), 3, "ref2_2", 29, refSchemaId2);

            var source = new IContentEntity[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.That.Matches(x => x.IsNoEnrichment()), A<IReadOnlyList<Guid>>.That.Matches(x => x.Count == 4)))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            var enriched = await sut.EnrichAsync(source, requestContext);

            Assert.Equal(
                new NamedContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "ref1_1, 13")
                                    .Add("de", "ref1_1, 13")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "ref2_1, 23")
                                    .Add("de", "ref2_1, 23"))),
                enriched.ElementAt(0).ReferenceData);

            Assert.Equal(
                new NamedContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "ref1_2, 17")
                                    .Add("de", "ref1_2, 17")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "ref2_2, 29")
                                    .Add("de", "ref2_2, 29"))),
                enriched.ElementAt(1).ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_when_content_has_more_items()
        {
            var ref1_1 = CreateRefContent(Guid.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(Guid.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(Guid.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(Guid.NewGuid(), 4, "ref2_2", 29, refSchemaId2);

            var source = new IContentEntity[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id, ref2_2.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_1.Id, ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.That.Matches(x => x.IsNoEnrichment()), A<IReadOnlyList<Guid>>.That.Matches(x => x.Count == 4)))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            var enriched = await sut.EnrichAsync(source, requestContext);

            Assert.Equal(
                new NamedContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "ref1_1, 13")
                                    .Add("de", "ref1_1, 13")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "2 Reference(s)")
                                    .Add("de", "2 Reference(s)"))),
                enriched.ElementAt(0).ReferenceData);

            Assert.Equal(
                new NamedContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "ref1_2, 17")
                                    .Add("de", "ref1_2, 17")))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddJsonValue("iv",
                                JsonValue.Object()
                                    .Add("en", "2 Reference(s)")
                                    .Add("de", "2 Reference(s)"))),
                enriched.ElementAt(1).ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_references_if_not_api_user()
        {
            var source = new IContentEntity[]
            {
                CreateContent(new Guid[] { Guid.NewGuid() }, new Guid[0])
            };

            var enriched = await sut.EnrichAsync(source, new Context(Mocks.ApiUser(), Mocks.App(appId)));

            Assert.Null(enriched.ElementAt(0).ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<List<Guid>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_references_if_disabled()
        {
            var source = new IContentEntity[]
            {
                CreateContent(new Guid[] { Guid.NewGuid() }, new Guid[0])
            };

            var enriched = await sut.EnrichAsync(source, new Context(Mocks.ApiUser(), Mocks.App(appId)));

            Assert.Null(enriched.ElementAt(0).ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<List<Guid>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_no_references_found()
        {
            var source = new IContentEntity[]
            {
                CreateContent(new Guid[0], new Guid[0])
            };

            var enriched = await sut.EnrichAsync(source, requestContext);

            Assert.NotNull(enriched.ElementAt(0).ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<List<Guid>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_nothing_to_enrich()
        {
            var source = new IContentEntity[0];

            await sut.EnrichAsync(source, requestContext);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<List<Guid>>.Ignored))
                .MustNotHaveHappened();
        }

        private IEnrichedContentEntity CreateContent(Guid[] ref1, Guid[] ref2)
        {
            return new ContentEntity
            {
                DataDraft =
                    new NamedContentData()
                        .AddField("ref1",
                            new ContentFieldData()
                                .AddJsonValue("iv", JsonValue.Array(ref1.Select(x => x.ToString()).ToArray())))
                        .AddField("ref2",
                            new ContentFieldData()
                                .AddJsonValue("iv", JsonValue.Array(ref2.Select(x => x.ToString()).ToArray()))),
                SchemaId = schemaId
            };
        }

        private static IEnrichedContentEntity CreateRefContent(Guid id, int version, string name, int number, NamedId<Guid> schemaId)
        {
            return new ContentEntity
            {
                Id = id,
                DataDraft =
                    new NamedContentData()
                        .AddField("name",
                            new ContentFieldData()
                                .AddValue("iv", name))
                        .AddField("number",
                            new ContentFieldData()
                                .AddValue("iv", number)),
                SchemaId = schemaId,
                Version = version
            };
        }
    }
}
