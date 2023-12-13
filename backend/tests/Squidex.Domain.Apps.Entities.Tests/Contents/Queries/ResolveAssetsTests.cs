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

public class ResolveAssetsTests : GivenContext
{
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly ProvideSchema schemaProvider;
    private readonly ResolveAssets sut;

    public ResolveAssetsTests()
    {
        Schema = Schema
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
            .SetFieldsInLists(FieldNames.Create("asset1", "asset2"));

        A.CallTo(() => urlGenerator.AssetContent(AppId, A<string>._))
            .ReturnsLazily(ctx => $"url/to/{ctx.GetArgument<string>(1)}");

        schemaProvider = x =>
        {
            if (x == SchemaId.Id)
            {
                return Task.FromResult((Schema, ResolvedComponents.Empty));
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
        var doc1 = CreateAsset() with { Version = 3, Type = AssetType.Unknown, FileName = "Document1.docx" };
        var doc2 = CreateAsset() with { Version = 4, Type = AssetType.Unknown, FileName = "Document2.docx" };

        var contents = new[]
        {
            CreateContent(
                [doc1.Id],
                [doc1.Id]),
            CreateContent(
                [doc2.Id],
                [doc2.Id])
        };

        A.CallTo(() => assetQuery.QueryAsync(
                A<Context>.That.Matches(x => x.NoAssetEnrichment() && x.NoTotal()), null, A<Q>.That.HasIds(doc1.Id, doc2.Id), CancellationToken))
            .Returns(ResultList.CreateFrom(4, doc1, doc2));

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, CancellationToken);

        A.CallTo(() => requestCache.AddDependency(doc1.UniqueId, doc1.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(doc2.UniqueId, doc2.Version))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_with_asset_urls()
    {
        var img1 = CreateAsset() with { Version = 1, Type = AssetType.Image, FileName = "Image1.png" };
        var img2 = CreateAsset() with { Version = 2, Type = AssetType.Unknown, FileName = "Image2.png", MimeType = "image/svg+xml" };

        var doc1 = CreateAsset() with { Version = 3, Type = AssetType.Unknown, FileName = "Document1.png" };
        var doc2 = CreateAsset() with { Version = 4, Type = AssetType.Unknown, FileName = "Document2.png", MimeType = "image/svg+xml", FileSize = 20_000 };

        var contents = new[]
        {
            CreateContent(
                [img1.Id],
                [img2.Id, img1.Id]),
            CreateContent(
                [doc1.Id],
                [doc2.Id, doc1.Id])
        };

        A.CallTo(() => assetQuery.QueryAsync(
                A<Context>.That.Matches(x => x.NoAssetEnrichment() && x.NoTotal()), null, A<Q>.That.HasIds(doc1.Id, doc2.Id, img1.Id, img2.Id), CancellationToken))
            .Returns(ResultList.CreateFrom(4, img1, img2, doc1, doc2));

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, CancellationToken);

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
    public async Task Should_not_enrich_references_if_not_frontend_user()
    {
        var contents = new[]
        {
            CreateContent([DomainId.NewGuid()], [])
        };

        await sut.EnrichAsync(ApiContext, contents, schemaProvider, CancellationToken);

        Assert.Null(contents[0].ReferenceData);

        A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._, A<CancellationToken>._))
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

        A.CallTo(() => assetQuery.QueryAsync(A<Context>._, null, A<Q>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_query_service_if_no_assets_found()
    {
        var contents = new[]
        {
            CreateContent([], [])
        };

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, CancellationToken);

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
            CreateContent([id1, id2], [])
        };

        await sut.EnrichAsync(FrontendContext, contents, schemaProvider, CancellationToken);

        Assert.NotNull(contents[0].ReferenceData);

        A.CallTo(() => assetQuery.QueryAsync(
                A<Context>.That.Matches(x => x.NoAssetEnrichment() && x.NoTotal()), null, A<Q>.That.HasIds(id1), A<CancellationToken>._))
            .MustHaveHappened();
    }

    private EnrichedContent CreateContent(DomainId[] assets1, DomainId[] assets2)
    {
        return CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("asset1",
                        new ContentFieldData()
                            .AddLocalized("iv", JsonValue.Array(assets1.Select(x => x.ToString()))))
                    .AddField("asset2",
                        new ContentFieldData()
                            .AddLocalized("en", JsonValue.Array(assets2.Select(x => x.ToString()))))
        };
    }
}
