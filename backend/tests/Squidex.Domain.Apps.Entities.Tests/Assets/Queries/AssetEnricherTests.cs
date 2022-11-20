// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class AssetEnricherTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly IUrlGenerator urlGenerator = A.Fake<IUrlGenerator>();
    private readonly IAssetMetadataSource assetMetadataSource1 = A.Fake<IAssetMetadataSource>();
    private readonly IAssetMetadataSource assetMetadataSource2 = A.Fake<IAssetMetadataSource>();
    private readonly IJsonSerializer serializer = A.Fake<IJsonSerializer>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly Context requestContext;
    private readonly AssetEnricher sut;

    public AssetEnricherTests()
    {
        ct = cts.Token;

        var assetMetadataSources = new[]
        {
            assetMetadataSource1,
            assetMetadataSource2
        };

        requestContext = Context.Anonymous(Mocks.App(appId));

        sut = new AssetEnricher(tagService, assetMetadataSources, requestCache, urlGenerator, serializer);
    }

    [Fact]
    public async Task Should_not_enrich_if_asset_contains_null_tags()
    {
        var source = new AssetEntity { AppId = appId };

        var actual = await sut.EnrichAsync(source, requestContext, ct);

        Assert.Empty(actual.TagNames);
    }

    [Fact]
    public async Task Should_enrich_with_cache_dependencies()
    {
        var source = new AssetEntity { AppId = appId, Id = DomainId.NewGuid(), Version = 13 };

        var actual = await sut.EnrichAsync(source, requestContext, ct);

        A.CallTo(() => requestCache.AddDependency(actual.UniqueId, actual.Version))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_asset_with_tag_names()
    {
        var source = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "id1",
                "id2"
            },
            AppId = appId
        };

        A.CallTo(() => tagService.GetTagNamesAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2"), ct))
            .Returns(new Dictionary<string, string>
            {
                ["id1"] = "name1",
                ["id2"] = "name2"
            });

        var actual = await sut.EnrichAsync(source, requestContext, ct);

        Assert.Equal(new HashSet<string> { "name1", "name2" }, actual.TagNames);
    }

    [Fact]
    public async Task Should_not_enrich_asset_with_tag_names_if_disabled()
    {
        var source = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "id1",
                "id2"
            },
            AppId = appId
        };

        var actual = await sut.EnrichAsync(source, requestContext.Clone(b => b.WithoutAssetEnrichment()), ct);

        Assert.Null(actual.TagNames);
    }

    [Fact]
    public async Task Should_enrich_asset_with_metadata()
    {
        var source = new AssetEntity
        {
            FileSize = 2 * 1024,
            Tags = new HashSet<string>
            {
                "id1",
                "id2"
            },
            AppId = appId
        };

        A.CallTo(() => assetMetadataSource1.Format(A<IAssetEntity>._))
            .Returns(new[] { "metadata1" });

        A.CallTo(() => assetMetadataSource2.Format(A<IAssetEntity>._))
            .Returns(new[] { "metadata2", "metadata3" });

        var actual = await sut.EnrichAsync(source, requestContext, ct);

        Assert.Equal("metadata1, metadata2, metadata3, 2 kB", actual.MetadataText);
    }

    [Fact]
    public async Task Should_enrich_multiple_assets_with_tag_names()
    {
        var source1 = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "id1",
                "id2"
            },
            AppId = appId
        };

        var source2 = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "id2",
                "id3"
            },
            AppId = appId
        };

        A.CallTo(() => tagService.GetTagNamesAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2", "id3"), ct))
            .Returns(new Dictionary<string, string>
            {
                ["id1"] = "name1",
                ["id2"] = "name2",
                ["id3"] = "name3"
            });

        var actual = await sut.EnrichAsync(new[] { source1, source2 }, requestContext, ct);

        Assert.Equal(new HashSet<string> { "name1", "name2" }, actual[0].TagNames);
        Assert.Equal(new HashSet<string> { "name2", "name3" }, actual[1].TagNames);
    }

    [Fact]
    public async Task Should_also_compute_ui_tokens_for_frontend()
    {
        var source = new AssetEntity
        {
            AppId = appId
        };

        var actual = await sut.EnrichAsync(new[] { source }, new Context(Mocks.FrontendUser(), Mocks.App(appId)), ct);

        Assert.NotNull(actual[0].EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_compute_ui_tokens()
    {
        var source = new AssetEntity
        {
            AppId = appId
        };

        var actual = await sut.EnrichAsync(new[] { source }, requestContext, ct);

        Assert.NotNull(actual[0].EditToken);

        A.CallTo(() => urlGenerator.Root())
            .MustHaveHappened();
    }
}
