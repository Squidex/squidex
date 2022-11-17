// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class ResolveAssetsTests
{
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
    private readonly ProvideSchema schemaProvider;
    private readonly Context requestContext;
    private readonly ResolveAssets sut;

    public ResolveAssetsTests()
    {
        requestContext = new Context(Mocks.FrontendUser(), Mocks.App(appId, Language.DE));

        var schemaDef =
            new Schema(schemaId.Name)
                .AddAssets(1, "asset1", Partitioning.Invariant, new AssetsFieldProperties
                {
                    ResolveFirst = true,
                    MinItems = 2,
                    MaxItems = 3
                })
                .AddAssets(2, "asset2", Partitioning.Language, new AssetsFieldProperties
                {
                    ResolveFirst = true,
                    MinItems = 1,
                    MaxItems = 1
                })
                .SetFieldsInLists("asset1", "asset2");

        A.CallTo(() => urlGenerator.AssetContent(appId, A<string>._))
            .ReturnsLazily(ctx => $"url/to/{ctx.GetArgument<string>(1)}");

        schemaProvider = x =>
        {
            if (x == schemaId.Id)
            {
                return Task.FromResult((Mocks.Schema(appId, schemaId, schemaDef), ResolvedComponents.Empty));
            }
            else
            {
                throw new DomainObjectNotFoundException(x.ToString());
            }
        };

        sut = new ResolveAssets(urlGenerator, assetQuery, requestCache);
    }

    [Fact]
    public async Task Should_add_assets_id_and_versions_as_dependency()
    {
        var doc1 = CreateAsset(DomainId.NewGuid(), 3, AssetType.Unknown, "Document1.docx");
        var doc2 = CreateAsset(DomainId.NewGuid(), 4, AssetType.Unknown, "Document2.docx");

        var contents = new[]
        {
            CreateContent(
                new[] { doc1.Id },
                new[] { doc1.Id }),
            CreateContent(
                new[] { doc2.Id },
                new[] { doc2.Id })
        };

        A.CallTo(() => assetQuery.QueryAsync(
                A<Context>.That.Matches(x => x.ShouldSkipAssetEnrichment() && x.ShouldSkipTotal()), null, A<Q>.That.HasIds(doc1.Id, doc2.Id), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(4, doc1, doc2));

        await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

        A.CallTo(() => requestCache.AddDependency(doc1.UniqueId, doc1.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(doc2.UniqueId, doc2.Version))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_with_asset_urls()
    {
        var img1 = CreateAsset(DomainId.NewGuid(), 1, AssetType.Image, "Image1.png");
        var img2 = CreateAsset(DomainId.NewGuid(), 2, AssetType.Unknown, "Image2.png", "image/svg+xml");

        var doc1 = CreateAsset(DomainId.NewGuid(), 3, AssetType.Unknown, "Document1.png");
        var doc2 = CreateAsset(DomainId.NewGuid(), 4, AssetType.Unknown, "Document2.png", "image/svg+xml", 20_000);

        var contents = new[]
        {
            CreateContent(
                new[] { img1.Id },
                new[] { img2.Id, img1.Id }),
            CreateContent(
                new[] { doc1.Id },
                new[] { doc2.Id, doc1.Id })
        };

        A.CallTo(() => assetQuery.QueryAsync(
                A<Context>.That.Matches(x => x.ShouldSkipAssetEnrichment() && x.ShouldSkipTotal()), null, A<Q>.That.HasIds(doc1.Id, doc2.Id, img1.Id, img2.Id), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(4, img1, img2, doc1, doc2));

        await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

        Assert.Equal(
            new ContentData()
                .AddField("asset1",
                    new ContentFieldData()
                        .AddLocalized("iv", JsonValue.Array($"url/to/{img1.Id}", img1.FileName)))
                .AddField("asset2",
                    new ContentFieldData()
                        .AddLocalized("en", JsonValue.Array($"url/to/{img2.Id}", img2.FileName))),
            contents[0].ReferenceData);

        Assert.Equal(
            new ContentData()
                .AddField("asset1",
                    new ContentFieldData()
                        .AddLocalized("iv", JsonValue.Array(doc1.FileName)))
                .AddField("asset2",
                    new ContentFieldData()
                        .AddLocalized("en", JsonValue.Array(doc2.FileName))),
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

        A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._, A<CancellationToken>._))
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

        A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_query_service_if_no_assets_found()
    {
        var contents = new[]
        {
            CreateContent(Array.Empty<DomainId>(), Array.Empty<DomainId>())
        };

        await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

        Assert.NotNull(contents[0].ReferenceData);

        A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_only_query_first_assets()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        var contents = new[]
        {
            CreateContent(new[] { id1, id2 }, Array.Empty<DomainId>())
        };

        await sut.EnrichAsync(requestContext, contents, schemaProvider, default);

        Assert.NotNull(contents[0].ReferenceData);

        A.CallTo(() => assetQuery.QueryAsync(
                A<Context>.That.Matches(x => x.ShouldSkipAssetEnrichment() && x.ShouldSkipTotal()), null, A<Q>.That.HasIds(id1), A<CancellationToken>._))
            .MustHaveHappened();
    }

    private ContentEntity CreateContent(DomainId[] assets1, DomainId[] assets2)
    {
        return new ContentEntity
        {
            Data =
                new ContentData()
                    .AddField("asset1",
                        new ContentFieldData()
                            .AddLocalized("iv", JsonValue.Array(assets1.Select(x => x.ToString()))))
                    .AddField("asset2",
                        new ContentFieldData()
                            .AddLocalized("en", JsonValue.Array(assets2.Select(x => x.ToString())))),
            SchemaId = schemaId
        };
    }

    private IEnrichedAssetEntity CreateAsset(DomainId id, int version, AssetType type, string fileName, string? fileType = null, int fileSize = 100)
    {
        return new AssetEntity
        {
            AppId = appId,
            Id = id,
            Type = type,
            FileName = fileName,
            FileSize = fileSize,
            MimeType = fileType!,
            Version = version
        };
    }
}
