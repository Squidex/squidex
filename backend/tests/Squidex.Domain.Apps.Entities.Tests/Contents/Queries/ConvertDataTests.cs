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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using TestUtils = Squidex.Domain.Apps.Core.TestHelpers.TestUtils;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ConvertDataTests
{
    private readonly ISchemaEntity schema;
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly ProvideSchema schemaProvider;
    private readonly ConvertData sut;

    public ConvertDataTests()
    {
        var schemaDef =
            new Schema("my-schema")
                .AddReferences(1, "references", Partitioning.Invariant)
                .AddAssets(2, "assets", Partitioning.Invariant)
                .AddArray(3, "array", Partitioning.Invariant, a => a
                    .AddAssets(31, "nested"));

        schema = Mocks.Schema(appId, schemaId, schemaDef);
        schemaProvider = x => Task.FromResult((schema, ResolvedComponents.Empty));

        sut = new ConvertData(urlGenerator, TestUtils.DefaultSerializer, assetRepository, contentRepository);
    }

    [Fact]
    public async Task Should_convert_data_and_data_draft_if_frontend_user()
    {
        var content = CreateContent(new ContentData());

        var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

        await sut.EnrichAsync(ctx, Enumerable.Repeat(content, 1), schemaProvider, default);

        Assert.NotNull(content.Data);
    }

    [Fact]
    public async Task Should_cleanup_references()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var source = BuildTestData(id1, id2);

        var content = CreateContent(source);

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
                                new JsonObject()
                                    .Add("nested", JsonValue.Array(id2)))));

        A.CallTo(() => assetRepository.QueryIdsAsync(appId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), A<CancellationToken>._))
            .Returns(new List<DomainId> { id2 });

        A.CallTo(() => contentRepository.QueryIdsAsync(appId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), SearchScope.All, A<CancellationToken>._))
            .Returns(new List<ContentIdStatus> { new ContentIdStatus(id2, id2, Status.Published) });

        var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

        await sut.EnrichAsync(ctx, Enumerable.Repeat(content, 1), schemaProvider, default);

        Assert.Equal(expected, content.Data);
    }

    [Fact]
    public async Task Should_cleanup_references_if_everything_deleted()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var source = BuildTestData(id1, id2);

        var content = CreateContent(source);

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
                                new JsonObject()
                                    .Add("nested", JsonValue.Array()))));

        A.CallTo(() => assetRepository.QueryIdsAsync(appId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), A<CancellationToken>._))
            .Returns(new List<DomainId>());

        A.CallTo(() => contentRepository.QueryIdsAsync(appId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), SearchScope.All, A<CancellationToken>._))
            .Returns(new List<ContentIdStatus>());

        var ctx = new Context(Mocks.FrontendUser(), Mocks.App(appId));

        await sut.EnrichAsync(ctx, Enumerable.Repeat(content, 1), schemaProvider, default);

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
                            new JsonObject()
                                .Add("nested", JsonValue.Array(id1, id2)))));
    }

    private ContentEntity CreateContent(ContentData data)
    {
        return new ContentEntity
        {
            Data = data,
            SchemaId = schemaId,
            Status = Status.Published
        };
    }
}
