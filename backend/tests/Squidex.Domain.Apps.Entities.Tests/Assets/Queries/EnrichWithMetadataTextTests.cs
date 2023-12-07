// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities.Assets.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class EnrichWithMetadataTextTests : GivenContext
{
    private readonly IAssetMetadataSource assetMetadataSource1 = A.Fake<IAssetMetadataSource>();
    private readonly IAssetMetadataSource assetMetadataSource2 = A.Fake<IAssetMetadataSource>();
    private readonly EnrichWithMetadataText sut;

    public EnrichWithMetadataTextTests()
    {
        var assetMetadataSources = new[]
        {
            assetMetadataSource1,
            assetMetadataSource2
        };

        sut = new EnrichWithMetadataText(assetMetadataSources);
    }

    [Fact]
    public async Task Should_not_enrich_if_disabled()
    {
        var asset = CreateAsset();

        await sut.EnrichAsync(ApiContext.Clone(b => b.WithNoAssetEnrichment()), Enumerable.Repeat(asset, 1), CancellationToken);

        A.CallTo(() => assetMetadataSource1.Format(A<Asset>._))
            .MustNotHaveHappened();

        A.CallTo(() => assetMetadataSource2.Format(A<Asset>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_asset_with_metadata()
    {
        var asset = CreateAsset() with
        {
            FileSize = 2 * 1024,
            Tags =
            [
                "id1",
                "id2"
            ]
        };

        A.CallTo(() => assetMetadataSource1.Format(asset))
            .Returns(new[] { "metadata1" });

        A.CallTo(() => assetMetadataSource2.Format(asset))
            .Returns(new[] { "metadata2", "metadata3" });

        await sut.EnrichAsync(FrontendContext, Enumerable.Repeat(asset, 1), CancellationToken);

        Assert.Equal("metadata1, metadata2, metadata3, 2 kB", asset.MetadataText);
    }
}
