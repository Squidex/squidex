// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using F = Squidex.Infrastructure.Queries.ClrFilter;

#pragma warning disable SA1300 // Element should begin with upper-case letter

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb;

[Trait("Category", "Dependencies")]
public class AssetsQueryIntegrationTests : IClassFixture<AssetsQueryFixture>
{
    public AssetsQueryFixture _ { get; }

    public AssetsQueryIntegrationTests(AssetsQueryFixture fixture)
    {
        _ = fixture;
    }

    [Fact]
    public async Task Should_find_asset_by_slug()
    {
        var random = _.RandomValue();

        var asset = await _.AssetRepository.FindAssetBySlugAsync(_.RandomAppId(), random);

        // The Slug is random here, as it does not really matter.
        Assert.NotNull(asset);
    }

    [Fact]
    public async Task Should_query_asset_by_hash()
    {
        var random = _.RandomValue();

        var assets = await _.AssetRepository.FindAssetByHashAsync(_.RandomAppId(), random, random, 1024);

        // The Hash is random here, as it does not really matter.
        Assert.NotNull(assets);
    }

    [Fact]
    public async Task Should_verify_ids()
    {
        var ids = Enumerable.Repeat(0, 50).Select(_ => DomainId.NewGuid()).ToHashSet();

        var assets = await _.AssetRepository.QueryIdsAsync(_.RandomAppId(), ids);

        // The IDs are random here, as it does not really matter.
        Assert.NotNull(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets(DomainId? parentId)
    {
        var query = new ClrQuery();

        var assets = await QueryAsync(parentId, query);

        // Default page size is 1000.
        Assert.Equal(1000, assets.Count);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_with_random_count(DomainId? parentId)
    {
        var query = new ClrQuery
        {
            Random = 40
        };

        var assets = await QueryAsync(parentId, query);

        // Default page size is 1000, so we expect less elements.
        Assert.Equal(40, assets.Count);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_tags(DomainId? parentId)
    {
        var random = _.RandomValue();

        var query = new ClrQuery
        {
            Filter = F.Eq("Tags", random)
        };

        var assets = await QueryAsync(parentId, query);

        // The tag is random here, as it does not really matter.
        Assert.NotNull(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_tags_and_fileName(DomainId? parentId)
    {
        var random = _.RandomValue();

        var query = new ClrQuery
        {
            Filter = F.And(F.Eq("Tags", random), F.Contains("FileName", random))
        };

        var assets = await QueryAsync(parentId, query);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_fileName(DomainId? parentId)
    {
        var random = _.RandomValue();

        var query = new ClrQuery
        {
            Filter = F.Contains("FileName", random)
        };

        var assets = await QueryAsync(parentId, query);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_fileName_and_tags(DomainId? parentId)
    {
        var random = _.RandomValue();

        var query = new ClrQuery
        {
            Filter = F.And(F.Contains("FileName", random), F.Eq("Tags", random))
        };

        var assets = await QueryAsync(parentId, query);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    public static IEnumerable<object?[]> ParentIds()
    {
        yield return new object?[] { null };
        yield return new object?[] { DomainId.Empty };
    }

    private async Task<IResultList<IAssetEntity>> QueryAsync(DomainId? parentId, ClrQuery clrQuery)
    {
        clrQuery.Top = 1000;
        clrQuery.Skip = 100;

        if (clrQuery.Sort == null || clrQuery.Sort.Count == 0)
        {
            clrQuery.Sort = new List<SortNode>
            {
                new SortNode("LastModified", SortOrder.Descending),
                new SortNode("Id", SortOrder.Descending)
            };
        }

        var q =
            Q.Empty
                .WithoutTotal()
                .WithQuery(clrQuery);

        var assets = await _.AssetRepository.QueryAsync(_.RandomAppId(), parentId, q);

        return assets;
    }
}
