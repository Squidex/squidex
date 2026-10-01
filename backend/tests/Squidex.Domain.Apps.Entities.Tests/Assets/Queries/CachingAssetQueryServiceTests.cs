// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities.Assets.Queries;

public class CachingAssetQueryServiceTests : GivenContext
{
    private const string QueryPrefix = "assets/";

    private readonly IAssetQueryService inner = A.Fake<IAssetQueryService>();
    private readonly IAssetEnricher assetEnricher = A.Fake<IAssetEnricher>();
    private readonly IDistributedCache distributedCache = A.Fake<IDistributedCache>(x => x.Wrapping(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))));
    private readonly IOptions<AssetQueryCacheOptions> options = Options.Create(new AssetQueryCacheOptions { CacheDuration = TimeSpan.FromMinutes(1) });
    private readonly ICacheGenerations generations = A.Fake<ICacheGenerations>();
    private readonly CachingAssetQueryService sut;
    private readonly EnrichedAsset asset;
    private readonly Dictionary<string, string> generationValues = [];

    public CachingAssetQueryServiceTests()
    {
        asset = CreateAsset();

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .ReturnsLazily(() => ResultList.CreateFrom(10, asset));

        A.CallTo(() => inner.FindAsync(A<Context>._, asset.Id, A<bool>._, A<long>._, A<CancellationToken>._))
            .ReturnsLazily(() => asset);

        A.CallTo(() => inner.FindBySlugAsync(A<Context>._, "my-slug", A<bool>._, A<CancellationToken>._))
            .ReturnsLazily(() => asset);

        A.CallTo(() => generations.GetAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily((string key, CancellationToken _) => generationValues.GetValueOrDefault(key, "1"));

        A.CallTo(() => generations.Reset(A<string>._))
            .Invokes(x => generationValues[x.GetArgument<string>(0)!] = Guid.NewGuid().ToString());

        sut = CreateSut(distributedCache);
    }

    [Fact]
    public async Task Should_query_inner_service_once_if_query_is_cached()
    {
        var actual = await QueryTwiceAsync(ApiContext, null, Q.Empty);

        Assert.Equal(10, actual.Total);
        actual.Single().Should().BeEquivalentTo(asset);

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_enrich_result_for_request_if_read_from_cache()
    {
        await QueryTwiceAsync(ApiContext, null, Q.Empty);

        A.CallTo(() => assetEnricher.EnrichCachedAsync(A<IReadOnlyList<EnrichedAsset>>.That.Matches(x => x.Single().Id == asset.Id), ApiContext, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_enrich_result_again_if_queried()
    {
        await sut.QueryAsync(ApiContext, null, Q.Empty, CancellationToken);

        A.CallTo(() => assetEnricher.EnrichCachedAsync(A<IReadOnlyList<EnrichedAsset>>._, A<Context>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_read_query_from_distributed_cache_of_other_node()
    {
        await sut.QueryAsync(ApiContext, null, Q.Empty, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        var actual = await CreateSut(distributedCache).QueryAsync(ApiContext, null, Q.Empty, CancellationToken);

        actual.Single().Should().BeEquivalentTo(asset);

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_query_again_after_change()
    {
        await sut.QueryAsync(ApiContext, null, Q.Empty, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        await HandleAsync(new AnnotateAsset { AppId = AppId });

        await sut.QueryAsync(ApiContext, null, Q.Empty, CancellationToken);

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_cache_queries_with_different_parents_separately()
    {
        await sut.QueryAsync(ApiContext, null, Q.Empty, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        await sut.QueryAsync(ApiContext, DomainId.NewGuid(), Q.Empty, CancellationToken);

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_query_inner_service_if_generation_cannot_be_read()
    {
        var failingGenerations = A.Fake<ICacheGenerations>();

        A.CallTo(() => failingGenerations.GetAsync(A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        var actual = await CreateSut(distributedCache, failingGenerations).QueryAsync(ApiContext, null, Q.Empty, CancellationToken);

        actual.Single().Should().BeEquivalentTo(asset);
        AssertCacheNotUsed();
    }

    [Fact]
    public async Task Should_not_cache_query_if_disabled_by_header()
    {
        await QueryWithoutCacheAsync(ApiContext.Clone(b => b.WithNoQueryCache()), Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_find()
    {
        await sut.FindAsync(ApiContext, asset.Id, ct: CancellationToken);
        await sut.FindAsync(ApiContext, asset.Id, ct: CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.FindAsync(A<Context>._, asset.Id, false, EtagVersion.Any, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_not_cache_find_by_slug()
    {
        await sut.FindBySlugAsync(ApiContext, "my-slug", ct: CancellationToken);
        await sut.FindBySlugAsync(ApiContext, "my-slug", ct: CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.FindBySlugAsync(A<Context>._, "my-slug", false, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_not_cache_query_for_frontend()
    {
        await QueryWithoutCacheAsync(FrontendContext, Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_query_if_app_has_asset_query_script()
    {
        App = App with { AssetScripts = new AssetScripts { Query = "<query-script>" } };

        await QueryWithoutCacheAsync(ApiContext, Q.Empty);
    }

    [Fact]
    public async Task Should_cache_query_if_app_has_asset_query_script_but_scripting_is_disabled()
    {
        App = App with { AssetScripts = new AssetScripts { Query = "<query-script>" } };

        await QueryTwiceAsync(ApiContext.Clone(b => b.WithNoScripting()), null, Q.Empty);

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_cache_different_queries_separately()
    {
        await sut.QueryAsync(ApiContext, null, Q.Empty.WithODataQuery("$top=1"), CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        await sut.QueryAsync(ApiContext, null, Q.Empty.WithODataQuery("$top=2"), CancellationToken);

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_not_cache_already_parsed_query()
    {
        await QueryWithoutCacheAsync(ApiContext, Q.Empty.WithQuery(new ClrQuery { Take = 5 }));
    }

    [Fact]
    public async Task Should_not_cache_random_query()
    {
        await QueryWithoutCacheAsync(ApiContext, Q.Empty.WithJsonQuery(new Query<JsonValue> { Random = 5 }));
    }

    [Fact]
    public async Task Should_not_cache_find_by_hash()
    {
        await sut.FindByHashAsync(ApiContext, "hash", "file", 100, CancellationToken);
        await sut.FindByHashAsync(ApiContext, "hash", "file", 100, CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.FindByHashAsync(A<Context>._, "hash", "file", 100, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_not_cache_find_global()
    {
        await sut.FindGlobalAsync(ApiContext, asset.Id, CancellationToken);
        await sut.FindGlobalAsync(ApiContext, asset.Id, CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.FindGlobalAsync(A<Context>._, asset.Id, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_reset_generation_if_asset_changed()
    {
        await HandleAsync(new AnnotateAsset { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_reset_generation_if_asset_deleted()
    {
        await HandleAsync(new DeleteAsset { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_folder_changed()
    {
        await HandleAsync(new RenameAssetFolder { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_command_failed()
    {
        var commandContext = new CommandContext(new AnnotateAsset { AppId = AppId }, A.Fake<ICommandBus>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.HandleAsync(commandContext, (_, _) => throw new InvalidOperationException(), CancellationToken));

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    private Task HandleAsync(ICommand command)
    {
        return sut.HandleAsync(new CommandContext(command, A.Fake<ICommandBus>()), (_, _) => Task.CompletedTask, CancellationToken);
    }

    private async Task<IResultList<EnrichedAsset>> QueryTwiceAsync(Context requestContext, DomainId? parentId, Q q)
    {
        await sut.QueryAsync(requestContext, parentId, q, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        return await sut.QueryAsync(requestContext, parentId, q, CancellationToken);
    }

    private async Task QueryWithoutCacheAsync(Context requestContext, Q q)
    {
        await sut.QueryAsync(requestContext, null, q, CancellationToken);
        await sut.QueryAsync(requestContext, null, q, CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<DomainId?>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    private CachingAssetQueryService CreateSut(IDistributedCache cache, ICacheGenerations? customGenerations = null)
    {
        return new CachingAssetQueryService(inner,
            CreateHybridCache(cache, TestUtils.DefaultSerializer),
            customGenerations ?? generations,
            assetEnricher,
            TestUtils.DefaultSerializer,
            options,
            A.Fake<ILogger<CachingAssetQueryService>>());
    }

    private static HybridCache CreateHybridCache(IDistributedCache cache, IJsonSerializer serializer)
    {
        var services = new ServiceCollection();

        services.AddSingleton(cache);
        services.AddSingleton(serializer);
        services.AddHybridCache()
            .AddSerializerFactory<JsonHybridCacheSerializerFactory>();

        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    private async Task WaitForEntriesAsync(string prefix, int count)
    {
        // The hybrid cache writes to the distributed cache in the background.
        for (var i = 0; i < 500; i++)
        {
            var numWrites = Fake.GetCalls(distributedCache).Count(x => x.Method.Name.StartsWith("Set", StringComparison.Ordinal) && x.Arguments.Get<string>(0)!.StartsWith(prefix, StringComparison.Ordinal));
            if (numWrites >= count)
            {
                return;
            }

            await Task.Delay(10, CancellationToken);
        }

        throw new TimeoutException($"Expected {count} entries with prefix '{prefix}'.");
    }

    private void AssertCacheNotUsed()
    {
        // The hybrid cache reads its own invalidation state when it is created.
        A.CallTo(() => distributedCache.GetAsync(A<string>.That.Not.StartsWith("__MSFT_HCT__"), A<CancellationToken>._))
            .MustNotHaveHappened();
        A.CallTo(() => distributedCache.SetAsync(A<string>._, A<byte[]>._, A<DistributedCacheEntryOptions>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
}
