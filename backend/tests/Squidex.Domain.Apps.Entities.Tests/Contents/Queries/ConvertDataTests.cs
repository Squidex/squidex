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
        var schemaDef =
            new Schema(SchemaId.Name)
                .AddReferences(1, "references", Partitioning.Invariant)
                .AddAssets(2, "assets", Partitioning.Invariant)
                .AddArray(3, "array", Partitioning.Invariant, a => a
                    .AddAssets(31, "nested"));

        A.CallTo(() => Schema.SchemaDef)
            .Returns(schemaDef);

        sut = new ConvertData(urlGenerator, TestUtils.DefaultSerializer, assetRepository, contentRepository);
    }

    [Fact]
    public async Task Should_convert_data_and_data_draft_if_frontend_user()
    {
        var content = CreateContent(new ContentData());

        await sut.EnrichAsync(FrontendContext, new[] { content }, SchemaProvider(), CancellationToken);

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

        A.CallTo(() => assetRepository.QueryIdsAsync(AppId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), CancellationToken))
            .Returns(new List<DomainId> { id2 });

        A.CallTo(() => contentRepository.QueryIdsAsync(AppId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), SearchScope.All, CancellationToken))
            .Returns(new List<ContentIdStatus> { new ContentIdStatus(id2, id2, Status.Published) });

        await sut.EnrichAsync(ApiContext, new[] { content }, SchemaProvider(), CancellationToken);

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

        A.CallTo(() => assetRepository.QueryIdsAsync(AppId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), CancellationToken))
            .Returns(new List<DomainId>());

        A.CallTo(() => contentRepository.QueryIdsAsync(AppId.Id, A<HashSet<DomainId>>.That.Is(id1, id2), SearchScope.All, CancellationToken))
            .Returns(new List<ContentIdStatus>());

        await sut.EnrichAsync(ApiContext, new[] { content }, SchemaProvider(), CancellationToken);

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
            AppId = AppId,
            Created = default,
            CreatedBy = User,
            Data = data,
            SchemaId = SchemaId,
            Status = Status.Published
        };
    }

    private ProvideSchema SchemaProvider()
    {
        return x => Task.FromResult((Schema, ResolvedComponents.Empty));
    }
}
