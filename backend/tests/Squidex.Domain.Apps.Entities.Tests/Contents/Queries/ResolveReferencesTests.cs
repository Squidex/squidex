// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ResolveReferencesTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly NamedId<DomainId> referenceSchemaId1 = NamedId.Of(DomainId.NewGuid(), "my-ref1");
    private readonly NamedId<DomainId> referenceSchemaId2 = NamedId.Of(DomainId.NewGuid(), "my-ref2");
    private readonly ProvideSchema schemaProvider;
    private readonly ResolveReferences sut;

    public ResolveReferencesTests()
    {
        var refSchema1 =
            Schema.WithId(referenceSchemaId1)
                .AddString(1, "name", Partitioning.Invariant,
                    new StringFieldProperties())
                .AddNumber(2, "number", Partitioning.Invariant,
                    new NumberFieldProperties())
                .SetFieldsInReferences(FieldNames.Create("data.name", "data.number"));

        var refSchema2 =
            Schema.WithId(referenceSchemaId2)
                .AddString(1, "name", Partitioning.Invariant,
                    new StringFieldProperties())
                .AddNumber(2, "number", Partitioning.Invariant,
                    new NumberFieldProperties())
                .SetFieldsInReferences(FieldNames.Create("data.name", "data.number"));

        Schema = Schema
            .AddReferences(1, "ref1", Partitioning.Invariant, new ReferencesFieldProperties
            {
                ResolveReference = true,
                MinItems = 1,
                MaxItems = 1,
                SchemaId = referenceSchemaId1.Id
            })
            .AddReferences(2, "ref2", Partitioning.Invariant, new ReferencesFieldProperties
            {
                ResolveReference = true,
                MinItems = 1,
                MaxItems = 1,
                SchemaId = referenceSchemaId2.Id
            })
            .SetFieldsInLists(FieldNames.Create("ref1", "ref2"));

        schemaProvider = x =>
        {
            if (x == SchemaId.Id)
            {
                return Task.FromResult((Schema, ResolvedComponents.Empty));
            }
            else if (x == referenceSchemaId1.Id)
            {
                return Task.FromResult((refSchema1, ResolvedComponents.Empty));
            }
            else if (x == referenceSchemaId2.Id)
            {
                return Task.FromResult((refSchema2, ResolvedComponents.Empty));
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
        var ref1_1 = CreateRefContent(1, "ref1_1", 13, referenceSchemaId1);
        var ref1_2 = CreateRefContent(2, "ref1_2", 17, referenceSchemaId1);
        var ref2_1 = CreateRefContent(3, "ref2_1", 23, referenceSchemaId2);
        var ref2_2 = CreateRefContent(4, "ref2_2", 29, referenceSchemaId2);

        var contents = new[]
        {
            CreateContent([ref1_1.Id], [ref2_1.Id]),
            CreateContent([ref1_2.Id], [ref2_2.Id])
        };

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x => x.NoEnrichment() && x.NoTotal()), A<Q>.That.HasIds(ref1_1.Id, ref1_2.Id, ref2_1.Id, ref2_2.Id), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, default);

        A.CallTo(() => requestCache.AddDependency(DomainId.Combine(AppId, referenceSchemaId1.Id), 1))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(DomainId.Combine(AppId, referenceSchemaId2.Id), 1))
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
        var ref1_1 = CreateRefContent(1, "ref1_1", 13, referenceSchemaId1);
        var ref1_2 = CreateRefContent(2, "ref1_2", 17, referenceSchemaId1);
        var ref2_1 = CreateRefContent(3, "ref2_1", 23, referenceSchemaId2);
        var ref2_2 = CreateRefContent(3, "ref2_2", 29, referenceSchemaId2);

        var contents = new[]
        {
            CreateContent([ref1_1.Id], [ref2_1.Id]),
            CreateContent([ref1_2.Id], [ref2_2.Id])
        };

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x => x.NoEnrichment() && x.NoTotal()), A<Q>.That.HasIds(ref1_1.Id, ref1_2.Id, ref2_1.Id, ref2_2.Id), CancellationToken))
            .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, CancellationToken);

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
        var ref1_1 = CreateRefContent(1, "ref1_1", 13, referenceSchemaId1);
        var ref1_2 = CreateRefContent(2, "ref1_2", 17, referenceSchemaId1);
        var ref2_1 = CreateRefContent(3, "ref2_1", 23, referenceSchemaId2);
        var ref2_2 = CreateRefContent(4, "ref2_2", 29, referenceSchemaId2);

        var contents = new[]
        {
            CreateContent([ref1_1.Id], [ref2_1.Id, ref2_2.Id]),
            CreateContent([ref1_2.Id], [ref2_1.Id, ref2_2.Id])
        };

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x => x.NoEnrichment() && x.NoTotal()), A<Q>.That.HasIds(ref1_1.Id, ref1_2.Id, ref2_1.Id, ref2_2.Id), CancellationToken))
            .Returns(ResultList.CreateFrom(4, ref1_1, ref1_2, ref2_1, ref2_2));

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, CancellationToken);

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
    public async Task Should_not_enrich_references_if_not_frontend_user()
    {
        var contents = new[]
        {
            CreateContent([DomainId.NewGuid()], [])
        };

        await sut.EnrichAsync(ApiContext, contents, schemaProvider, CancellationToken);

        Assert.Null(contents[0].ReferenceData);

        A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_enrich_references_if_disabled()
    {
        var contents = new[]
        {
            CreateContent([DomainId.NewGuid()], [])
        };

        await sut.EnrichAsync(FrontendContext.Clone(b => b.WithNoEnrichment(true)), contents, schemaProvider, CancellationToken);

        Assert.Null(contents[0].ReferenceData);

        A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_query_service_if_no_references_found()
    {
        var contents = new[]
        {
            CreateContent([], [])
        };

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, CancellationToken);

        Assert.NotNull(contents[0].ReferenceData);

        A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private EnrichedContent CreateContent(DomainId[] ref1, DomainId[] ref2)
    {
        return CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("ref1",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(ref1.Select(x => x.ToString()))))
                    .AddField("ref2",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(ref2.Select(x => x.ToString())))),
        };
    }

    private EnrichedContent CreateRefContent(int version, string name, int number, NamedId<DomainId> refSchemaId)
    {
        return CreateContent() with
        {
            SchemaId = refSchemaId,
            Data =
                new ContentData()
                    .AddField("name",
                        new ContentFieldData()
                            .AddInvariant(name))
                    .AddField("number",
                        new ContentFieldData()
                            .AddInvariant(number)),
            Version = version,
        };
    }
}
