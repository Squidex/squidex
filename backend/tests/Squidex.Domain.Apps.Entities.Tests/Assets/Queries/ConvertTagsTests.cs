// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Tags;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class ConvertTagsTests : GivenContext
{
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly ConvertTags sut;

    public ConvertTagsTests()
    {
        sut = new ConvertTags(tagService);
    }

    [Fact]
    public async Task Should_not_enrich_if_asset_has_null_tags()
    {
        var asset = CreateAsset() with { Tags = null! };

        await sut.EnrichAsync(ApiContext, Enumerable.Repeat(asset, 1), CancellationToken);

        Assert.Empty(asset.TagNames);
    }

    [Fact]
    public async Task Should_not_enrich_asset_with_tag_names_if_disabled()
    {
        var asset = CreateAsset() with { TagNames = null! };

        await sut.EnrichAsync(ApiContext.Clone(b => b.WithNoAssetEnrichment()), Enumerable.Repeat(asset, 1), CancellationToken);

        Assert.Null(asset.TagNames);
    }

    [Fact]
    public async Task Should_enrich_asset_with_tag_names()
    {
        var asset = CreateAsset() with
        {
            Tags = ["id1", "id2"]
        };

        A.CallTo(() => tagService.GetTagNamesAsync(AppId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2"), CancellationToken))
            .Returns(new Dictionary<string, string>
            {
                ["id1"] = "name1",
                ["id2"] = "name2"
            });

        await sut.EnrichAsync(ApiContext, Enumerable.Repeat(asset, 1), CancellationToken);

        Assert.Equal(["name1", "name2"], asset.TagNames);
    }

    [Fact]
    public async Task Should_enrich_multiple_assets_with_tag_names()
    {
        var asset1 = CreateAsset() with
        {
            Tags = ["id1", "id2"]
        };

        var asset2 = CreateAsset() with
        {
            Tags = ["id2", "id3"]
        };

        A.CallTo(() => tagService.GetTagNamesAsync(AppId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2", "id3"), CancellationToken))
            .Returns(new Dictionary<string, string>
            {
                ["id1"] = "name1",
                ["id2"] = "name2",
                ["id3"] = "name3"
            });

        await sut.EnrichAsync(ApiContext, new[] { asset1, asset2 }, CancellationToken);

        Assert.Equal(["name1", "name2"], asset1.TagNames);
        Assert.Equal(["name2", "name3"], asset2.TagNames);
    }
}
