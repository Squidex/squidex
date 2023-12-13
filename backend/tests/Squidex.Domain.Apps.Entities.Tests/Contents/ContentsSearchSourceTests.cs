// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ContentsSearchSourceTests : GivenContext
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly ITextIndex contentIndex = A.Fake<ITextIndex>();
    private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    private readonly NamedId<DomainId> schemaId1 = NamedId.Of(DomainId.NewGuid(), "my-schema1");
    private readonly NamedId<DomainId> schemaId2 = NamedId.Of(DomainId.NewGuid(), "my-schema2");
    private readonly NamedId<DomainId> schemaId3 = NamedId.Of(DomainId.NewGuid(), "my-schema3");
    private readonly ContentsSearchSource sut;

    public ContentsSearchSourceTests()
    {
        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns(
            [
                Schema.WithId(schemaId1),
                Schema.WithId(schemaId2),
                Schema.WithId(schemaId3),
            ]);

        sut = new ContentsSearchSource(AppProvider, contentQuery, contentIndex, urlGenerator);
    }

    [Fact]
    public async Task Should_return_content_with_default_name()
    {
        var content = CreateContent() with
        {
            SchemaId = schemaId1
        };

        await TestContentAsync(content, "Content");
    }

    [Fact]
    public async Task Should_return_content_with_multiple_invariant_reference_fields()
    {
        var content = CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddInvariant("hello"))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddInvariant("world")),
            ReferenceFields = new[]
            {
                Fields.String(1, "field1", Partitioning.Invariant),
                Fields.String(2, "field2", Partitioning.Invariant)
            },
            SchemaId = schemaId1
        };

        await TestContentAsync(content, "hello, world");
    }

    [Fact]
    public async Task Should_return_content_with_invariant_reference_field()
    {
        var content = CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddInvariant("hello")),
            ReferenceFields = new[]
            {
                Fields.String(1, "field", Partitioning.Invariant)
            },
            SchemaId = schemaId1
        };

        await TestContentAsync(content, "hello");
    }

    [Fact]
    public async Task Should_return_content_with_localized_reference_field()
    {
        var content = CreateContent() with
        {
            Id = DomainId.NewGuid(),
            Data =
                new ContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddLocalized("en", "hello")),
            ReferenceFields = new[]
            {
                Fields.String(1, "field", Partitioning.Language)
            },
            SchemaId = schemaId1
        };

        await TestContentAsync(content, "hello");
    }

    [Fact]
    public async Task Should_return_content_with_invariant_field_and_reference_data()
    {
        var content = CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddInvariant("raw")),
            ReferenceData =
                new ContentData()
                    .AddField("field",
                        new ContentFieldData()
                            .AddLocalized("en", "resolved")),
            ReferenceFields = new[]
            {
                Fields.String(1, "field", Partitioning.Language)
            },
            SchemaId = schemaId1
        };

        await TestContentAsync(content, "resolved");
    }

    [Fact]
    public async Task Should_not_invoke_content_index_if_user_has_no_permission()
    {
        var requestContext = ContextWithPermissions();

        var actual = await sut.SearchAsync("query", requestContext, CancellationToken);

        Assert.Empty(actual);

        A.CallTo(() => contentIndex.SearchAsync(App, A<TextQuery>._, A<SearchScope>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_context_query_if_no_id_found()
    {
        var requestContext = ContextWithPermissions(schemaId1, schemaId2);

        A.CallTo(() => contentIndex.SearchAsync(App, A<TextQuery>.That.Matches(x => x.Text == "query~"), ApiContext.Scope(), CancellationToken))
            .Returns([]);

        var actual = await sut.SearchAsync("query", requestContext, CancellationToken);

        Assert.Empty(actual);

        A.CallTo(() => contentQuery.QueryAsync(requestContext, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private async Task TestContentAsync(EnrichedContent content, string expectedName)
    {
        var requestContext = ContextWithPermissions(schemaId1, schemaId2);

        var ids = new List<DomainId> { content.Id };

        A.CallTo(() => contentIndex.SearchAsync(App, A<TextQuery>.That.Matches(x => x.Text == "query~"), ApiContext.Scope(), CancellationToken))
            .Returns(ids);

        A.CallTo(() => contentQuery.QueryAsync(requestContext, A<Q>.That.HasIds(ids), CancellationToken))
            .Returns(ResultList.CreateFrom<EnrichedContent>(1, content));

        A.CallTo(() => urlGenerator.ContentUI(AppId, schemaId1, content.Id))
            .Returns("content-url");

        var actual = await sut.SearchAsync("query", requestContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add(expectedName, SearchResultType.Content, "content-url"));
    }

    private Context ContextWithPermissions(params NamedId<DomainId>[] allowedSchemas)
    {
        var permissions = new List<string>();

        foreach (var schemaId in allowedSchemas)
        {
            permissions.Add(PermissionIds.ForApp(PermissionIds.AppContentsReadOwn, AppId.Name, schemaId.Name).Id);
        }

        return CreateContext(false, permissions.ToArray());
    }
}
