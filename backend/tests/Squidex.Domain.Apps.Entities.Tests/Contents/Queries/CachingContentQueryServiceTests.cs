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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Json.System;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Queries.Json;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class CachingContentQueryServiceTests : GivenContext
{
    private const string QueryPrefix = "contents/";

    private static readonly IJsonSerializer Serializer = TestUtils.CreateSerializer(options =>
    {
        options.Converters.Add(new SurrogateJsonConverter<FilterNode<JsonValue>, JsonFilterSurrogate>());
    });

    private readonly IContentQueryService inner = A.Fake<IContentQueryService>();
    private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
    private readonly IDistributedCache distributedCache = A.Fake<IDistributedCache>(x => x.Wrapping(new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()))));
    private readonly IOptions<ContentQueryCacheOptions> options = Options.Create(new ContentQueryCacheOptions { CacheDuration = TimeSpan.FromMinutes(1) });
    private readonly ICacheGenerations generations = A.Fake<ICacheGenerations>();
    private readonly CachingContentQueryService sut;
    private readonly EnrichedContent content;
    private readonly Dictionary<string, string> generationValues = [];

    public CachingContentQueryServiceTests()
    {
        content = CreateContent();

        A.CallTo(() => inner.GetSchemaOrThrowAsync(A<Context>._, SchemaId.Name, A<CancellationToken>._))
            .ReturnsLazily(() => Schema);

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .ReturnsLazily(() => ResultList.CreateFrom(10, content));

        A.CallTo(() => inner.FindAsync(A<Context>._, SchemaId.Name, content.Id, A<long>._, A<CancellationToken>._))
            .ReturnsLazily(() => content);

        A.CallTo(() => generations.GetAsync(A<string>._, A<CancellationToken>._))
            .ReturnsLazily((string key, CancellationToken _) => generationValues.GetValueOrDefault(key, "1"));

        A.CallTo(() => generations.Reset(A<string>._))
            .Invokes(x => generationValues[x.GetArgument<string>(0)!] = Guid.NewGuid().ToString());

        sut = CreateSut(distributedCache);
    }

    [Fact]
    public async Task Should_query_inner_service_once_if_query_is_cached()
    {
        var actual = await QueryTwiceAsync(CreateApiContext(), Q.Empty);

        Assert.Equal(10, actual.Total);
        actual.Single().Should().BeEquivalentTo(content);

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_enrich_result_for_request_if_read_from_cache()
    {
        var requestContext = CreateApiContext();

        await QueryTwiceAsync(requestContext, Q.Empty);

        A.CallTo(() => contentEnricher.EnrichCachedAsync(A<IReadOnlyList<EnrichedContent>>.That.Matches(x => x.Single().Id == content.Id), requestContext, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_enrich_result_again_if_queried()
    {
        await sut.QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);

        A.CallTo(() => contentEnricher.EnrichCachedAsync(A<IReadOnlyList<EnrichedContent>>._, A<Context>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_read_query_from_distributed_cache_of_other_node()
    {
        await sut.QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        var actual = await CreateSut(distributedCache).QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);

        actual.Single().Should().BeEquivalentTo(content);

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_query_again_after_change()
    {
        await sut.QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        await HandleAsync(new UpdateContent { AppId = AppId });

        await sut.QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_query_again_if_schema_has_changed()
    {
        await sut.QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        Schema = Schema with { Version = Schema.Version + 1 };

        await sut.QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_query_inner_service_if_generation_cannot_be_read()
    {
        var failingGenerations = A.Fake<ICacheGenerations>();

        A.CallTo(() => failingGenerations.GetAsync(A<string>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        var actual = await CreateSut(distributedCache, failingGenerations).QueryAsync(CreateApiContext(), SchemaId.Name, Q.Empty, CancellationToken);

        actual.Single().Should().BeEquivalentTo(content);
        AssertCacheNotUsed();
    }

    [Fact]
    public async Task Should_not_cache_query_if_disabled_by_header()
    {
        await QueryWithoutCacheAsync(CreateApiContext().Clone(b => b.WithNoQueryCache()), Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_find()
    {
        await sut.FindAsync(CreateApiContext(), SchemaId.Name, content.Id, ct: CancellationToken);
        await sut.FindAsync(CreateApiContext(), SchemaId.Name, content.Id, ct: CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.FindAsync(A<Context>._, SchemaId.Name, content.Id, EtagVersion.Any, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_not_cache_query_for_frontend()
    {
        await QueryWithoutCacheAsync(CreateApiContext(isFrontend: true), Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_query_for_unpublished_content()
    {
        await QueryWithoutCacheAsync(CreateApiContext().Clone(b => b.WithUnpublished()), Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_query_if_workflow_is_resolved()
    {
        await QueryWithoutCacheAsync(CreateApiContext().Clone(b => b.WithResolveFlow()), Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_query_if_user_can_only_read_own_content()
    {
        await QueryWithoutCacheAsync(CreateApiContext(permissionId: PermissionIds.AppContentsReadOwn), Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_query_if_schema_has_query_pre_script()
    {
        Schema = Schema with { Scripts = new SchemaScripts { QueryPre = "<query-script>" } };

        await QueryWithoutCacheAsync(CreateApiContext(), Q.Empty);
    }

    [Fact]
    public async Task Should_not_cache_scheduled_query()
    {
        await QueryWithoutCacheAsync(CreateApiContext(), Q.Empty.WithSchedule(default, default));
    }

    [Fact]
    public async Task Should_not_cache_query_if_schema_has_query_script()
    {
        Schema = Schema with { Scripts = new SchemaScripts { Query = "<query-script>" } };

        await QueryWithoutCacheAsync(CreateApiContext(), Q.Empty);
    }

    [Fact]
    public async Task Should_cache_query_if_schema_has_query_script_but_scripting_is_disabled()
    {
        Schema = Schema with { Scripts = new SchemaScripts { Query = "<query-script>" } };

        await QueryTwiceAsync(CreateApiContext().Clone(b => b.WithNoScripting()), Q.Empty);

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_cache_already_parsed_query()
    {
        await QueryWithoutCacheAsync(CreateApiContext(), Q.Empty.WithQuery(new ClrQuery { Take = 5 }));
    }

    [Fact]
    public async Task Should_not_cache_random_query()
    {
        await QueryWithoutCacheAsync(CreateApiContext(), Q.Empty.WithJsonQuery(new Query<JsonValue> { Random = 5 }));
    }

    [Fact]
    public async Task Should_not_cache_random_json_query()
    {
        await QueryWithoutCacheAsync(CreateApiContext(), Q.Empty.WithJsonQuery("{ \"random\": 5 }"));
    }

    [Fact]
    public async Task Should_cache_different_queries_separately()
    {
        await QuerySeparatelyAsync(
            (CreateApiContext(), Q.Empty.WithODataQuery("$top=1")),
            (CreateApiContext(), Q.Empty.WithODataQuery("$top=2")));
    }

    [Fact]
    public async Task Should_cache_json_queries_with_different_filters_separately()
    {
        await QuerySeparatelyAsync(
            (CreateApiContext(), Q.Empty.WithJsonQuery(CreateJsonQuery("a", 1))),
            (CreateApiContext(), Q.Empty.WithJsonQuery(CreateJsonQuery("a", 2))),
            (CreateApiContext(), Q.Empty.WithJsonQuery(CreateJsonQuery("b", 1))));
    }

    [Fact]
    public async Task Should_cache_json_queries_with_different_sorting_separately()
    {
        var query2 = CreateJsonQuery("a", 1);

        query2.Sort = [new SortNode("data.field", SortOrder.Descending)];

        await QuerySeparatelyAsync(
            (CreateApiContext(), Q.Empty.WithJsonQuery(CreateJsonQuery("a", 1))),
            (CreateApiContext(), Q.Empty.WithJsonQuery(query2)));
    }

    [Fact]
    public async Task Should_cache_queries_with_different_headers_separately()
    {
        await QuerySeparatelyAsync(
            (CreateApiContext().Clone(b => b.WithLanguages(["en"])), Q.Empty),
            (CreateApiContext().Clone(b => b.WithLanguages(["de"])), Q.Empty));
    }

    [Fact]
    public async Task Should_cache_same_json_query_once()
    {
        await QueryTwiceAsync(CreateApiContext(), Q.Empty.WithJsonQuery(CreateJsonQuery("a", 1)));

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_ignore_header_order_for_cache_key()
    {
        await sut.QueryAsync(CreateApiContext().Clone(b => b.WithFlatten().WithNoTotal()), SchemaId.Name, Q.Empty, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        await sut.QueryAsync(CreateApiContext().Clone(b => b.WithNoTotal().WithFlatten()), SchemaId.Name, Q.Empty, CancellationToken);

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_cache_query_over_all_schemas()
    {
        await sut.QueryAsync(CreateApiContext(), Q.Empty, CancellationToken);
        await sut.QueryAsync(CreateApiContext(), Q.Empty, CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.QueryAsync(A<Context>._, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    [Fact]
    public async Task Should_reset_generation_if_content_changed()
    {
        await HandleAsync(new UpdateContent { AppId = AppId });

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
    public async Task Should_not_reset_generation_if_content_is_only_validated()
    {
        await HandleAsync(new ValidateContent { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_content_defaults_are_only_enriched()
    {
        await HandleAsync(new EnrichContentDefaults { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_asset_changed()
    {
        await HandleAsync(new AnnotateAsset { AppId = AppId });

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_reset_generation_if_command_failed()
    {
        var commandContext = new CommandContext(new UpdateContent { AppId = AppId }, A.Fake<ICommandBus>());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.HandleAsync(commandContext, (_, _) => throw new InvalidOperationException(), CancellationToken));

        A.CallTo(() => generations.Reset(A<string>._))
            .MustNotHaveHappened();
    }

    private Task HandleAsync(ICommand command)
    {
        return sut.HandleAsync(new CommandContext(command, A.Fake<ICommandBus>()), (_, _) => Task.CompletedTask, CancellationToken);
    }

    private async Task<IResultList<EnrichedContent>> QueryTwiceAsync(Context requestContext, Q q)
    {
        await sut.QueryAsync(requestContext, SchemaId.Name, q, CancellationToken);
        await WaitForEntriesAsync(QueryPrefix, 1);

        return await sut.QueryAsync(requestContext, SchemaId.Name, q, CancellationToken);
    }

    private async Task QueryWithoutCacheAsync(Context requestContext, Q q)
    {
        await sut.QueryAsync(requestContext, SchemaId.Name, q, CancellationToken);
        await sut.QueryAsync(requestContext, SchemaId.Name, q, CancellationToken);

        AssertCacheNotUsed();

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappenedTwiceExactly();
    }

    private async Task QuerySeparatelyAsync(params (Context Context, Q Query)[] queries)
    {
        // Wait for each entry, so that the next query would get a hit, if the keys were equal.
        for (var i = 0; i < queries.Length; i++)
        {
            await sut.QueryAsync(queries[i].Context, SchemaId.Name, queries[i].Query, CancellationToken);
            await WaitForEntriesAsync(QueryPrefix, i + 1);
        }

        A.CallTo(() => inner.QueryAsync(A<Context>._, SchemaId.Name, A<Q>._, A<CancellationToken>._))
            .MustHaveHappened(queries.Length, Times.Exactly);
    }

    private CachingContentQueryService CreateSut(IDistributedCache cache, ICacheGenerations? customGenerations = null)
    {
        return new CachingContentQueryService(inner,
            CreateHybridCache(cache, Serializer),
            customGenerations ?? generations,
            contentEnricher,
            Serializer,
            options,
            A.Fake<ILogger<CachingContentQueryService>>());
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

    private Context CreateApiContext(bool isFrontend = false, string permissionId = PermissionIds.AppContentsRead)
    {
        var permission = PermissionIds.ForApp(permissionId, AppId.Name, SchemaId.Name).Id;

        return CreateContext(isFrontend, permission);
    }

    private static Query<JsonValue> CreateJsonQuery(string field, int value)
    {
        return new Query<JsonValue>
        {
            Filter = new CompareFilter<JsonValue>($"data.{field}", CompareOperator.Equals, value),
        };
    }
}
