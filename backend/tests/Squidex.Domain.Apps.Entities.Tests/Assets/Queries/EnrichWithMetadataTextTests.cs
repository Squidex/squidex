// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Json;
using Squidex.Web;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class EnrichWithMetadataTextTests
{
    private readonly IAssetMetadataSource assetMetadataSource1 = A.Fake<IAssetMetadataSource>();
    private readonly IAssetMetadataSource assetMetadataSource2 = A.Fake<IAssetMetadataSource>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly Context requestContext;
    private readonly EnrichWithMetadataText sut;

    public EnrichWithMetadataTextTests()
    {
        var assetMetadataSources = new[]
        {
            assetMetadataSource1,
            assetMetadataSource2
        };

        requestContext = Context.Anonymous(Mocks.App(appId));

        sut = new EnrichWithMetadataText(assetMetadataSources);
    }

    [Fact]
    public async Task Should_not_enrich_if_disabled()
    {
        var context = ;

        var asset = new AssetEntity();

        await sut.EnrichAsync(requestContext.Clone(b => b.WithoutAssetEnrichment()), Enumerable.Repeat(asset, 1), default);

        A.CallTo(() => assetMetadataSource1.Format(A<IAssetEntity>._))
            .MustNotHaveHappened();

        A.CallTo(() => assetMetadataSource2.Format(A<IAssetEntity>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_asset_with_metadata()
    {
        var asset = new AssetEntity
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

        await sut.EnrichAsync(requestContext, Enumerable.Repeat(asset, 1), default);

        Assert.Equal("metadata1, metadata2, metadata3, 2 kB", asset.MetadataText);
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
}
