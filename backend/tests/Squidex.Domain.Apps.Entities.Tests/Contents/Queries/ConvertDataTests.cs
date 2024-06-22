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
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json.Objects;
using TestUtils = Squidex.Domain.Apps.Core.TestHelpers.TestUtils;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ConvertDataTests : GivenContext
{
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly ConvertData sut;

    public ConvertDataTests()
    {
        Schema = Schema
            .AddReferences(1, "references", Partitioning.Invariant,
                new ReferencesFieldProperties { DefaultValue = ReadonlyList.Create("default1") })
            .AddAssets(2, "assets", Partitioning.Invariant,
                new AssetsFieldProperties { DefaultValue = ReadonlyList.Create("default2") })
            .AddArray(3, "array", Partitioning.Invariant, a => a
                .AddAssets(31, "nested",
                    new AssetsFieldProperties { DefaultValue = ReadonlyList.Create("default3") }));

        sut = new ConvertData(urlGenerator, TestUtils.DefaultSerializer, assetRepository, contentRepository);
    }

    [Fact]
    public async Task Should_convert_data_and_data_draft_if_frontend_user()
    {
        var content = CreateContent();

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.NotNull(content.Data);
    }

    [Fact]
    public async Task Should_enrich_with_default_value()
    {
        var source =
            new ContentData()
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object())));

        var content = CreateContent() with
        {
            Data = source
        };

        var expected =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array("default1")))
                .AddField("assets",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array("default2")))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested", JsonValue.Array("default3")))));

        await sut.EnrichAsync(ApiContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(expected, content.Data);
    }

    [Fact]
    public async Task Should_cleanup_references()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var source = BuildTestData(id1, id2);

        var content = CreateContent() with
        {
            Data = source
        };

        var expected =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(id2)))
                .AddField("assets",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array()))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested", JsonValue.Array(id2)))));

        A.CallTo(() => assetRepository.QueryIdsAsync(AppId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), CancellationToken))
            .Returns([id2]);

        A.CallTo(() => contentRepository.QueryIdsAsync(App, A<HashSet<DomainId>>.That.Is(id1, id2), SearchScope.All, CancellationToken))
            .Returns([new ContentIdStatus(id2, id2, Status.Published)]);

        await sut.EnrichAsync(ApiContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(expected, content.Data);
    }

    [Fact]
    public async Task Should_cleanup_references_if_everything_deleted()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var source = BuildTestData(id1, id2);

        var content = CreateContent() with
        {
            Data = source
        };

        var expected =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array()))
                .AddField("assets",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array()))
                .AddField("array",
                    new ContentFieldData()
                        .AddInvariant(
                            JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested", JsonValue.Array()))));

        A.CallTo(() => assetRepository.QueryIdsAsync(AppId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), CancellationToken))
            .Returns([]);

        A.CallTo(() => contentRepository.QueryIdsAsync(App, A<HashSet<DomainId>>.That.Is(id1, id2), SearchScope.All, CancellationToken))
            .Returns([]);

        await sut.EnrichAsync(ApiContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(expected, content.Data);
    }

    private static ContentData BuildTestData(DomainId id1, DomainId id2)
    {
        return new ContentData()
            .AddField("references",
                new ContentFieldData()
                    .AddInvariant(JsonValue.Array(id1, id2)))
            .AddField("assets",
                new ContentFieldData()
                    .AddInvariant(JsonValue.Array(id1)))
            .AddField("array",
                new ContentFieldData()
                    .AddInvariant(
                        JsonValue.Array(
                            JsonValue.Object()
                                .Add("nested", JsonValue.Array(id1, id2)))));
    }

    private ProvideSchema SchemaProvider()
    {
        return x => Task.FromResult((Schema, ResolvedComponents.Empty));
    }
}
