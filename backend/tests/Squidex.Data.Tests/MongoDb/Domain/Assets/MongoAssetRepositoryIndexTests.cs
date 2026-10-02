// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using NodaTime;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;
using Squidex.MongoDb.TestHelpers;

namespace Squidex.MongoDb.Domain.Assets;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoAssetRepositoryIndexTests : GivenContext, IAsyncLifetime
{
    private const string CollectionName = "States_Assets2";
    private const int PageSize = 20;
    private const int NumNoise = 10_000;
    private const int NumRoot = 200;
    private const int NumFolder = 100;
    private const int NumDeleted = 20;
    private const int NumTarget = NumRoot + NumFolder + NumDeleted;
    private const int MaxExaminedForPage = (PageSize * 2) + 10;
    private const int MaxExaminedForApp = (NumTarget * 2) + 10;
    private static readonly Instant NoiseTime = Instant.FromUtc(2025, 1, 1, 0, 0);
    private static readonly Instant TargetTime = Instant.FromUtc(2024, 1, 1, 0, 0);
    private static readonly NamedId<DomainId> NoiseAppId = NamedId.Of(DomainId.Create("2e4f5a3c-0a51-4c38-8a85-6a4b7c0f0001"), "noise");
    private static readonly NamedId<DomainId> TargetAppId = NamedId.Of(DomainId.Create("2e4f5a3c-0a51-4c38-8a85-6a4b7c0f0002"), "target");
    private static readonly DomainId NoiseFolderId = DomainId.Create("2e4f5a3c-0a51-4c38-8a85-6a4b7c0f0003");
    private static readonly DomainId TargetFolderId = DomainId.Create("2e4f5a3c-0a51-4c38-8a85-6a4b7c0f0004");
    private readonly MongoQueryProfiler profiler;
    private readonly MongoAssetRepository sut;

    public MongoAssetRepositoryIndexTests(MongoFixture fixture)
    {
        // Use a dedicated database to control the data distribution.
        var database = fixture.Client.GetDatabase("Test_AssetIndexes");

        profiler = new MongoQueryProfiler(database, CollectionName);

        sut = new MongoAssetRepository(database, A.Fake<ILogger<MongoAssetRepository>>(), string.Empty);
    }

    public async ValueTask InitializeAsync()
    {
        await sut.InitializeAsync(default);
        await SeedAsync();
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    [Fact]
    public async Task Should_use_index_to_query_assets_of_app()
    {
        await AssertPlansAsync(MaxExaminedForPage, () =>
            sut.QueryAsync(TargetAppId.Id, null, PageQuery(new ClrQuery())));
    }

    [Fact]
    public async Task Should_use_index_to_query_assets_in_root_folder()
    {
        await AssertPlansAsync(MaxExaminedForPage, () =>
            sut.QueryAsync(TargetAppId.Id, DomainId.Empty, PageQuery(new ClrQuery())));
    }

    [Fact]
    public async Task Should_use_index_to_query_assets_in_folder()
    {
        await AssertPlansAsync(MaxExaminedForPage, () =>
            sut.QueryAsync(TargetAppId.Id, TargetFolderId, PageQuery(new ClrQuery())));
    }

    [Fact]
    public async Task Should_use_index_to_query_assets_by_tag()
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Eq("tags", "tag-even"),
        };

        await AssertPlansAsync(MaxExaminedForApp, () =>
            sut.QueryAsync(TargetAppId.Id, null, PageQuery(query)));
    }

    [Fact]
    public async Task Should_use_index_to_count_assets_by_tag()
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Eq("tags", "tag-even"),
        };

        var profiles = await AssertPlansAsync(MaxExaminedForApp, () =>
            sut.QueryAsync(TargetAppId.Id, null, PageQuery(query, withTotal: true)));

        // The query for the page and the query for the total.
        Assert.Equal(2, profiles.Count);
    }

    [Fact]
    public async Task Should_use_index_to_count_assets_in_folder()
    {
        var query = new ClrQuery
        {
            Filter = ClrFilter.Eq("tags", "tag-even"),
        };

        var profiles = await AssertPlansAsync(MaxExaminedForApp, () =>
            sut.QueryAsync(TargetAppId.Id, TargetFolderId, PageQuery(query, withTotal: true)));

        // The query for the page and the query for the total.
        Assert.Equal(2, profiles.Count);
    }

    [Fact]
    public async Task Should_use_index_to_query_assets_randomly()
    {
        var query = new ClrQuery
        {
            Random = 5,
        };

        await AssertPlansAsync(MaxExaminedForPage, () =>
            sut.QueryAsync(TargetAppId.Id, null, PageQuery(query)));
    }

    [Fact]
    public async Task Should_use_index_to_query_assets_by_ids()
    {
        var ids = await GetTargetIdsAsync();

        await AssertPlansAsync(MaxExaminedForPage, () =>
            sut.QueryAsync(TargetAppId.Id, null, Q.Empty.WithIds(ids).WithQuery(SortedQuery(new ClrQuery()))), allowSort: true);
    }

    [Fact]
    public async Task Should_use_index_to_query_ids()
    {
        var ids = await GetTargetIdsAsync();

        await AssertPlansAsync(MaxExaminedForPage, () =>
            sut.QueryIdsAsync(TargetAppId.Id, ids));
    }

    [Fact]
    public async Task Should_use_index_to_query_child_ids()
    {
        await AssertPlansAsync(NumFolder + 10, () =>
            sut.QueryChildIdsAsync(TargetAppId.Id, TargetFolderId));
    }

    [Fact]
    public async Task Should_use_index_to_stream_assets_of_app()
    {
        await AssertPlansAsync(MaxExaminedForApp, () =>
            sut.StreamAll(TargetAppId.Id).ToListAsync().AsTask());
    }

    [Fact]
    public async Task Should_use_index_to_find_asset_by_id()
    {
        var id = (await GetTargetIdsAsync()).First();

        await AssertPlansAsync(1, () =>
            sut.FindAssetAsync(TargetAppId.Id, id, false));
    }

    [Fact]
    public async Task Should_use_index_to_find_asset_by_id_only()
    {
        var id = (await GetTargetIdsAsync()).First();

        await AssertPlansAsync(1, () =>
            sut.FindAssetAsync(id));
    }

    [Fact]
    public async Task Should_use_index_to_find_asset_by_slug()
    {
        await AssertPlansAsync(1, () =>
            sut.FindAssetBySlugAsync(TargetAppId.Id, "target-42", false));
    }

    [Fact]
    public async Task Should_use_index_to_find_asset_by_hash()
    {
        await AssertPlansAsync(1, () =>
            sut.FindAssetByHashAsync(TargetAppId.Id, "hash-42", "file-42.png", 1024));
    }

    private async Task<List<QueryProfile>> AssertPlansAsync(int maxExamined, Func<Task> action, bool allowSort = false)
    {
        var profiles = await profiler.ProfileAsync(action);

        Assert.NotEmpty(profiles);

        foreach (var profile in profiles)
        {
            MongoQueryProfiler.AssertIndexed(profile, allowSort);

            var examined = Math.Max(profile.KeysExamined, profile.DocsExamined);

            Assert.True(examined <= maxExamined, $"Examined more than {maxExamined}: {profile}");
        }

        return profiles;
    }

    private async Task<HashSet<DomainId>> GetTargetIdsAsync()
    {
        return await sut.StreamAll(TargetAppId.Id).Take(PageSize).Select(x => x.Id).ToHashSetAsync();
    }

    private static Q PageQuery(ClrQuery query, bool withTotal = false)
    {
        var q = Q.Empty.WithQuery(SortedQuery(query));

        return withTotal ? q : q.WithoutTotal();
    }

    private static ClrQuery SortedQuery(ClrQuery query)
    {
        // Same default sorting as the asset query parser.
        query.Take = PageSize;
        query.Sort =
        [
            new SortNode("lastModified", SortOrder.Descending),
            new SortNode("id", SortOrder.Ascending),
        ];

        return query;
    }

    private async Task SeedAsync()
    {
        if (await sut.StreamAll(TargetAppId.Id).AnyAsync())
        {
            return;
        }

        var assets = new List<Asset>();

        // The noise app has newer assets, so that an index that is not bounded by the app has to skip all of them.
        for (var i = 0; i < NumNoise; i++)
        {
            assets.Add(CreateAsset(NoiseAppId, i, NoiseTime) with
            {
                ParentId = i % 2 == 0 ? NoiseFolderId : default,
            });
        }

        for (var i = 0; i < NumTarget; i++)
        {
            var asset = CreateAsset(TargetAppId, i, TargetTime);

            if (i >= NumRoot + NumFolder)
            {
                asset = asset with { IsDeleted = true };
            }
            else if (i >= NumRoot)
            {
                asset = asset with { ParentId = TargetFolderId };
            }

            assets.Add(asset);
        }

        var store = (ISnapshotStore<Asset>)sut;

        foreach (var batch in assets.Chunk(1000))
        {
            await store.WriteManyAsync(batch.Select(x => new SnapshotWriteJob<Asset>(x.UniqueId, x, 0)));
        }
    }

    private Asset CreateAsset(NamedId<DomainId> appId, int index, Instant time)
    {
        return CreateAsset() with
        {
            AppId = appId,
            FileHash = $"hash-{index}",
            FileName = $"file-{index}.png",
            LastModified = time.Plus(Duration.FromSeconds(index)),
            Slug = $"{appId.Name}-{index}",
            Tags = [index % 2 == 0 ? "tag-even" : "tag-odd"],
        };
    }
}
