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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ResolveReferencesTests
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> refSchemaId1 = NamedId.Of(Guid.NewGuid(), "my-ref1");
        private readonly NamedId<Guid> refSchemaId2 = NamedId.Of(Guid.NewGuid(), "my-ref2");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
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
                    return Task.FromResult(Mocks.Schema(appId, schemaId, schemaDef));
                }
                else if (x == refSchemaId1.Id)
                {
                    return Task.FromResult(Mocks.Schema(appId, refSchemaId1, refSchemaDef));
                }
                else if (x == refSchemaId2.Id)
                {
                    return Task.FromResult(Mocks.Schema(appId, refSchemaId2, refSchemaDef));
                }
                else
                {
                    throw new DomainObjectNotFoundException(x.ToString(), typeof(ISchemaEntity));
                }
            };

            sut = new ResolveReferences(new Lazy<IContentQueryService>(() => contentQuery), requestCache);
        }

        [Fact]
        public async Task Should_add_referenced_id_and__as_dependency()
        {
            var ref1_1 = CreateRefContent(Guid.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(Guid.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(Guid.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(Guid.NewGuid(), 4, "ref2_2", 29, refSchemaId2);

            var contents = new[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<IReadOnlyList<Guid>>.That.Matches(x => x.Count == 4)))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            var enriched1 = contents[0];

            A.CallTo(() => requestCache.AddDependency(refSchemaId1.Id, 0))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(refSchemaId2.Id, 0))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref1_1.Id, ref1_1.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref2_1.Id, ref2_1.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref1_2.Id, ref1_2.Version))
                .MustHaveHappened();

            A.CallTo(() => requestCache.AddDependency(ref2_2.Id, ref2_2.Version))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_with_reference_data()
        {
            var ref1_1 = CreateRefContent(Guid.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(Guid.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(Guid.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(Guid.NewGuid(), 3, "ref2_2", 29, refSchemaId2);

            var contents = new[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.That.Matches(x => !x.ShouldEnrichContent()), A<IReadOnlyList<Guid>>.That.Matches(x => x.Count == 4)))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

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
                contents[0].ReferenceData);

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
                contents[1].ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_when_content_has_more_items()
        {
            var ref1_1 = CreateRefContent(Guid.NewGuid(), 1, "ref1_1", 13, refSchemaId1);
            var ref1_2 = CreateRefContent(Guid.NewGuid(), 2, "ref1_2", 17, refSchemaId1);
            var ref2_1 = CreateRefContent(Guid.NewGuid(), 3, "ref2_1", 23, refSchemaId2);
            var ref2_2 = CreateRefContent(Guid.NewGuid(), 4, "ref2_2", 29, refSchemaId2);

            var contents = new[]
            {
                CreateContent(new[] { ref1_1.Id }, new[] { ref2_1.Id, ref2_2.Id }),
                CreateContent(new[] { ref1_2.Id }, new[] { ref2_1.Id, ref2_2.Id })
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.That.Matches(x => !x.ShouldEnrichContent()), A<IReadOnlyList<Guid>>.That.Matches(x => x.Count == 4)))
                .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

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
                contents[0].ReferenceData);

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
                contents[1].ReferenceData);
        }

        [Fact]
        public async Task Should_not_enrich_references_if_not_api_user()
        {
            var contents = new[]
            {
                CreateContent(new[] { Guid.NewGuid() }, new Guid[0])
            };

            var ctx = new Context(Mocks.ApiUser(), Mocks.App(appId));

            await sut.EnrichAsync(ctx, contents, schemaProvider);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<List<Guid>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_enrich_references_if_disabled()
        {
            var contents = new[]
            {
                CreateContent(new[] { Guid.NewGuid() }, new Guid[0])
            };

            var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId)).WithoutContentEnrichment(true);

            await sut.EnrichAsync(ctx, contents, schemaProvider);

            Assert.Null(contents[0].ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<List<Guid>>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_query_service_if_no_references_found()
        {
            var contents = new[]
            {
                CreateContent(new Guid[0], new Guid[0])
            };

            await sut.EnrichAsync(requestContext, contents, schemaProvider);

            Assert.NotNull(contents[0].ReferenceData);

            A.CallTo(() => contentQuery.QueryAsync(A<Context>.Ignored, A<List<Guid>>.Ignored))
                .MustNotHaveHappened();
        }

        private ContentEntity CreateContent(Guid[] ref1, Guid[] ref2)
        {
            return new ContentEntity
            {
                Data =
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
                Data =
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
