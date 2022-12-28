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

public class ConvertTagsTests
{
    private readonly ITagService tagService = A.Fake<ITagService>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly Context requestContext;
    private readonly ConvertTags sut;

    public ConvertTagsTests()
    {
        requestContext = Context.Anonymous(Mocks.App(appId));

        sut = new ConvertTags(tagService);
    }

    [Fact]
    public async Task Should_not_enrich_if_asset_has_null_tags()
    {
        var asset = new AssetEntity();

        await sut.EnrichAsync(requestContext, Enumerable.Repeat(asset, 1), default);

        Assert.Empty(asset.TagNames);
    }

    [Fact]
    public async Task Should_not_enrich_asset_with_tag_names_if_disabled()
    {
        var asset = new AssetEntity();

        await sut.EnrichAsync(requestContext.Clone(b => b.WithoutAssetEnrichment()), Enumerable.Repeat(asset, 1), default);

        Assert.Null(asset.TagNames);
    }

    [Fact]
    public async Task Should_enrich_asset_with_tag_names()
    {
        var asset = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "id1",
                "id2"
            },
            AppId = appId
        };

        A.CallTo(() => tagService.GetTagNamesAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2"), default))
            .Returns(new Dictionary<string, string>
            {
                ["id1"] = "name1",
                ["id2"] = "name2"
            });

        await sut.EnrichAsync(requestContext, Enumerable.Repeat(asset, 1), default);

        Assert.Equal(new HashSet<string> { "name1", "name2" }, asset.TagNames);
    }

    [Fact]
    public async Task Should_enrich_multiple_assets_with_tag_names()
    {
        var asset1 = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "id1",
                "id2"
            },
            AppId = appId
        };

        var asset2 = new AssetEntity
        {
            Tags = new HashSet<string>
            {
                "id2",
                "id3"
            },
            AppId = appId
        };

        A.CallTo(() => tagService.GetTagNamesAsync(appId.Id, TagGroups.Assets, A<HashSet<string>>.That.Is("id1", "id2", "id3"), default))
            .Returns(new Dictionary<string, string>
            {
                ["id1"] = "name1",
                ["id2"] = "name2",
                ["id3"] = "name3"
            });

        await sut.EnrichAsync(requestContext, new[] { asset1, asset2 }, default);

        Assert.Equal(new HashSet<string> { "name1", "name2" }, asset1.TagNames);
        Assert.Equal(new HashSet<string> { "name2", "name3" }, asset2.TagNames);
    }
}
