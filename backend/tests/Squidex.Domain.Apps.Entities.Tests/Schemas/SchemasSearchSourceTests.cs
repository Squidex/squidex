// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Schemas;

public class SchemasSearchSourceTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly SchemasSearchSource sut;

    public SchemasSearchSourceTests()
    {
        sut = new SchemasSearchSource(AppProvider, urlGenerator);
    }

    [Fact]
    public async Task Should_not_add_result_to_contents_if_user_has_no_permission()
    {
        var schema1 = Schema with { Name = "SchemaA1" };

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns([schema1]);

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
        .Returns("schemaA1-url");

        var actual = await sut.SearchAsync("schema", ApiContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("SchemaA1 Schema", SearchResultType.Schema, "schemaA1-url"));
    }

    [Fact]
    public async Task Should_not_add_result_to_contents_if_schema_is_component()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppContentsReadOwn, AppId.Name, "schemaA1");

        var schema1 = Schema with { Name = "SchemaA1", Type = SchemaType.Component };

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns([schema1]);

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
            .Returns("schemaA1-url");

        var actual = await sut.SearchAsync("schema", CreateContext(false, permission.Id), CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("SchemaA1 Schema", SearchResultType.Schema, "schemaA1-url"));
    }

    [Fact]
    public async Task Should_return_result_to_schema_and_contents_if_matching_and_permission_given()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppContentsReadOwn, AppId.Name, "schemaA2");

        var schema1 = Schema with { Name = "SchemaA1" };
        var schema2 = Schema with { Name = "SchemaA2" };
        var schema3 = Schema with { Name = "SchemaB2" };

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns([schema1, schema2, schema3]);

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
            .Returns("schemaA1-url");

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema2.NamedId()))
            .Returns("schemaA2-url");

        A.CallTo(() => urlGenerator.ContentsUI(AppId, schema2.NamedId()))
            .Returns("schemaA2-contents-url");

        var actual = await sut.SearchAsync("schemaA", CreateContext(false, permission.Id), CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("SchemaA1 Schema", SearchResultType.Schema, "schemaA1-url")
                .Add("SchemaA2 Schema", SearchResultType.Schema, "schemaA2-url")
                .Add("SchemaA2 Contents", SearchResultType.Content, "schemaA2-contents-url", "SchemaA2"));
    }

    [Fact]
    public async Task Should_return_result_to_schema_and_contents_if_schema_is_singleton()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppContentsReadOwn, AppId.Name, "schemaA1");

        var schema1 = Schema with { Name = "SchemaA1", Type = SchemaType.Singleton };

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns([schema1]);

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
            .Returns("schemaA1-url");

        A.CallTo(() => urlGenerator.ContentUI(AppId, schema1.NamedId(), schema1.Id))
            .Returns("schemaA1-content-url");

        var actual = await sut.SearchAsync("schemaA", CreateContext(false, permission.Id), CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("SchemaA1 Schema", SearchResultType.Schema, "schemaA1-url")
                .Add("SchemaA1 Content", SearchResultType.Content, "schemaA1-content-url", "SchemaA1"));
    }
}
