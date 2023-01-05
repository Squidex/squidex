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
using Squidex.Infrastructure;
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
    public async Task Should_not_add_actual_to_contents_if_user_has_no_permission()
    {
        var schema1 = CreateSchema("schemaA1");

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns(new List<ISchemaEntity> { schema1 });

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
        .Returns("schemaA1-url");

        var actual = await sut.SearchAsync("schema", ApiContext, CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("schemaA1 Schema", SearchResultType.Schema, "schemaA1-url"));
    }

    [Fact]
    public async Task Should_not_add_actual_to_contents_if_schema_is_component()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppContentsReadOwn, AppId.Name, "schemaA1");

        var schema1 = CreateSchema("schemaA1", SchemaType.Component);

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns(new List<ISchemaEntity> { schema1 });

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
            .Returns("schemaA1-url");

        var actual = await sut.SearchAsync("schema", CreateContext(false, permission.Id), CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("schemaA1 Schema", SearchResultType.Schema, "schemaA1-url"));
    }

    [Fact]
    public async Task Should_return_actual_to_schema_and_contents_if_matching_and_permission_given()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppContentsReadOwn, AppId.Name, "schemaA2");

        var schema1 = CreateSchema("schemaA1");
        var schema2 = CreateSchema("schemaA2");
        var schema3 = CreateSchema("schemaB2");

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns(new List<ISchemaEntity> { schema1, schema2, schema3 });

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
            .Returns("schemaA1-url");

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema2.NamedId()))
            .Returns("schemaA2-url");

        A.CallTo(() => urlGenerator.ContentsUI(AppId, schema2.NamedId()))
            .Returns("schemaA2-contents-url");

        var actual = await sut.SearchAsync("schemaA", CreateContext(false, permission.Id), CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("schemaA1 Schema", SearchResultType.Schema, "schemaA1-url")
                .Add("schemaA2 Schema", SearchResultType.Schema, "schemaA2-url")
                .Add("schemaA2 Contents", SearchResultType.Content, "schemaA2-contents-url", "schemaA2"));
    }

    [Fact]
    public async Task Should_return_actual_to_schema_and_contents_if_schema_is_singleton()
    {
        var permission = PermissionIds.ForApp(PermissionIds.AppContentsReadOwn, AppId.Name, "schemaA1");

        var schema1 = CreateSchema("schemaA1", SchemaType.Singleton);

        A.CallTo(() => AppProvider.GetSchemasAsync(AppId.Id, CancellationToken))
            .Returns(new List<ISchemaEntity> { schema1 });

        A.CallTo(() => urlGenerator.SchemaUI(AppId, schema1.NamedId()))
            .Returns("schemaA1-url");

        A.CallTo(() => urlGenerator.ContentUI(AppId, schema1.NamedId(), schema1.Id))
            .Returns("schemaA1-content-url");

        var actual = await sut.SearchAsync("schemaA", CreateContext(false, permission.Id), CancellationToken);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("schemaA1 Schema", SearchResultType.Schema, "schemaA1-url")
                .Add("schemaA1 Content", SearchResultType.Content, "schemaA1-content-url", "schemaA1"));
    }

    private ISchemaEntity CreateSchema(string name, SchemaType type = SchemaType.Default)
    {
        return Mocks.Schema(AppId, NamedId.Of(DomainId.NewGuid(), name), new Schema(name, type: type));
    }
}
