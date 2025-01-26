// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

#pragma warning disable xUnit1044 // Avoid using TheoryData type arguments that are not serializable
#pragma warning disable MA0040 // Forward the CancellationToken parameter to methods that take one

namespace Squidex.Shared;

public abstract class AssetRepositoryTests : GivenContext
{
    private const int NumValues = 250;
    private readonly string randomValue = Random.Shared.Next(NumValues).ToString(CultureInfo.InvariantCulture);
    private readonly DomainId appId;
    private readonly DomainId parentId = DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d451");
    private readonly NamedId<DomainId>[] appIds =
    [
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d452"), "my-app1"),
        NamedId.Of(DomainId.Create("3b5ba909-e5a5-4858-9d0d-df4ff922d453"), "my-app1"),
    ];

    protected AssetRepositoryTests()
    {
        appId = appIds[Random.Shared.Next(appIds.Length)].Id;
    }

    protected abstract Task<IAssetRepository> CreateSutAsync();

    protected async Task<IAssetRepository> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        if (await sut.StreamAll(appIds[0].Id).AnyAsync())
        {
            return sut;
        }

        if (sut is not ISnapshotStore<Asset> store)
        {
            return sut;
        }

        var batch = new List<SnapshotWriteJob<Asset>>();

        async Task ExecuteBatchAsync(Asset? entity)
        {
            if (entity != null)
            {
                batch.Add(new SnapshotWriteJob<Asset>(entity.UniqueId, entity, 0));
            }

            if ((entity == null || batch.Count >= 1000) && batch.Count > 0)
            {
                await store.WriteManyAsync(batch);
                batch.Clear();
            }
        }

        foreach (var forAppId in appIds)
        {
            for (var i = 0; i < NumValues; i++)
            {
                var fileName = i.ToString(CultureInfo.InvariantCulture);

                for (var j = 0; j < NumValues; j++)
                {
                    var asset = CreateAsset() with
                    {
                        AppId = forAppId,
                        FileHash = fileName,
                        FileName = fileName,
                        Metadata = new AssetMetadata
                        {
                            ["value"] = JsonValue.Create($"value_{j}"),
                        },
                        Tags =
                        [
                            $"tag_{j}",
                        ],
                        Slug = fileName,
                    };

                    await ExecuteBatchAsync(asset);
                }
            }

            var byParent = CreateAsset() with
            {
                AppId = forAppId,
                ParentId = parentId,
                FileHash = "0",
                FileName = "0",
                Metadata = new AssetMetadata
                {
                    ["value"] = JsonValue.Create("0"),
                },
                Tags =
                [
                    "tag_0",
                ],
                Slug = "0",
            };

            await ExecuteBatchAsync(byParent);
        }

        await ExecuteBatchAsync(null);
        return sut;
    }

    public static readonly TheoryData<DomainId?> ParentIds = new TheoryData<DomainId?>
    {
        { null },
        { DomainId.Empty },
    };

    [Fact]
    public async Task Should_find_asset_by_document_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var asset1 = await sut.StreamAll(appId).FirstAsync();
        var asset2 = await sut.FindAssetAsync(asset1.Id);

        // The Slug is random here, as it does not really matter.
        Assert.NotNull(asset2);
    }

    [Fact]
    public async Task Should_find_asset_by_id()
    {
        var sut = await CreateAndPrepareSutAsync();

        var assetRef = await sut.StreamAll(appId).FirstAsync();
        var asset = await sut.FindAssetAsync(appId, assetRef.Id, false);

        // The Slug is random here, as it does not really matter.
        Assert.NotNull(asset);
    }

    [Fact]
    public async Task Should_find_asset_by_hash()
    {
        var sut = await CreateAndPrepareSutAsync();

        var asset = await sut.FindAssetByHashAsync(appId, randomValue, randomValue, 1024);

        // The Hash is random here, as it does not really matter.
        Assert.NotNull(asset);
    }

    [Fact]
    public async Task Should_find_asset_by_slug()
    {
        var sut = await CreateAndPrepareSutAsync();

        var asset = await sut.FindAssetBySlugAsync(appId, randomValue, false);

        // The Slug is random here, as it does not really matter.
        Assert.NotNull(asset);
    }

    [Fact]
    public async Task Should_query_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var assetIds = await sut.StreamAll(appId).Take(100).Select(x => x.Id).ToHashSetAsync();
        var assets = await sut.QueryIdsAsync(appId, assetIds);

        // The IDs are random here, as it does not really matter.
        Assert.NotEmpty(assets);
    }

    [Fact]
    public async Task Should_query_child_ids()
    {
        var sut = await CreateAndPrepareSutAsync();

        var assets = await sut.QueryChildIdsAsync(appId, parentId);

        // No pagination is going on here.
        Assert.Single(assets);
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
            Random = 40,
        };

        var assets = await QueryAsync(parentId, query);

        // Default page size is 1000, so we expect less elements.
        Assert.Equal(40, assets.Count);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_tags(DomainId? parentId)
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Eq("tags", $"tag_{randomValue}"),
        };

        var assets = await QueryAsync(parentId, query);

        // The tag is random here, as it does not really matter.
        Assert.NotNull(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_tags_in_query(DomainId? parentId)
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.In("tags", new List<string> { randomValue, "other" }),
        };

        var assets = await QueryAsync(parentId, query);

        // The tag is random here, as it does not really matter.
        Assert.NotNull(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_tags_and_fileName(DomainId? parentId)
    {
        var query = new ClrQuery
        {
            Filter =
                ClrFilter.And(
                    ClrFilter.Eq("tags", $"tag_{randomValue}"),
                    ClrFilter.Contains("fileName", randomValue)),
        };

        var assets = await QueryAsync(parentId, query);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_fileName(DomainId? parentId)
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Contains("fileName", randomValue),
        };

        var assets = await QueryAsync(parentId, query);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_metadata(DomainId? parentId)
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Contains("metadata.value", $"value_{randomValue}"),
        };

        var assets = await QueryAsync(parentId, query);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_fileName_and_tags(DomainId? parentId)
    {
        var query = new ClrQuery
        {
            Filter =
                ClrFilter.And(
                    ClrFilter.Contains("fileName", randomValue),
                    ClrFilter.Eq("tags", $"tag_{randomValue}")),
        };

        var assets = await QueryAsync(parentId, query);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    [Theory]
    [MemberData(nameof(ParentIds))]
    public async Task Should_query_assets_by_ids(DomainId? parentId)
    {
        var sut = await CreateAndPrepareSutAsync();

        var q =
            Q.Empty
                .WithIds(await sut.StreamAll(appId).Take(100).Select(x => x.Id).ToHashSetAsync());

        var assets = await sut.QueryAsync(appId, parentId, q);

        // The filter is a random value from the expected result set.
        Assert.NotEmpty(assets);
    }

    private async Task<IResultList<Asset>> QueryAsync(DomainId? parentId,
        ClrQuery clrQuery, int top = 1000, int skip = 0)
    {
        var sut = await CreateAndPrepareSutAsync();

        clrQuery.Top = top;
        clrQuery.Skip = skip;
        clrQuery.Sort ??= [];

        if (clrQuery.Sort.Count == 0)
        {
            clrQuery.Sort.Add(new SortNode("lastModified", SortOrder.Descending));
        }

        if (!clrQuery.Sort.Exists(x => x.Path.Equals("id")))
        {
            clrQuery.Sort.Add(new SortNode("id", SortOrder.Ascending));
        }

        var q =
            Q.Empty
                .WithoutTotal()
                .WithQuery(clrQuery);

        var assets = await sut.QueryAsync(appId, parentId, q);

        return assets;
    }
}
