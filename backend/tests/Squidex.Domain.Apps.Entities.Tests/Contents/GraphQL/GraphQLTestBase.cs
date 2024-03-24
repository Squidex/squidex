// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Tasks;
using Squidex.Messaging.Subscriptions;
using Squidex.Shared.Users;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public abstract class GraphQLTestBase : IClassFixture<TranslationsFixture>
{
    protected readonly GraphQLSerializer serializer = new GraphQLSerializer(TestUtils.DefaultOptions());
    protected readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    protected readonly ICommandBus commandBus = A.Fake<ICommandBus>();
    protected readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    protected readonly ISubscriptionService subscriptionService = A.Fake<ISubscriptionService>();
    protected readonly IUserResolver userResolver = A.Fake<IUserResolver>();
    protected readonly Context requestContext;
    private CachingGraphQLResolver? sut;

    protected class QueryOptions
    {
        public string Query { get; set; }
    }

    protected GraphQLTestBase()
    {
        A.CallTo(() => userResolver.QueryManyAsync(A<string[]>._, default))
            .ReturnsLazily(x =>
            {
                var ids = x.GetArgument<string[]>(0)!;

                var users = ids.Select(id =>
                    UserMocks.User(
                        id,
                        $"{id}@email.com",
                        $"{id}name"));

                return Task.FromResult(users.ToDictionary(x => x.Id));
            });

        requestContext = new Context(Mocks.FrontendUser(), TestApp.Default);
    }

    protected void AssertResult(object expected, ExecutionResult actual)
    {
        var jsonOutputResult = serializer.Serialize(actual);
        var isonOutputExpected = serializer.Serialize(expected);

        Assert.Equal(isonOutputExpected, jsonOutputResult);
    }

    protected Task<ExecutionResult> ExecuteAsync(TestQuery query)
    {
        // Use a shared instance to test caching.
        sut ??= CreateSut(
            TestSchemas.Default,
            TestSchemas.Reference1,
            TestSchemas.Reference2,
            TestSchemas.Singleton,
            TestSchemas.Component);

        var options = query.ToOptions(sut.Services);

        return ExecuteAsync(sut, options);
    }

    private static async Task<ExecutionResult> ExecuteAsync(CachingGraphQLResolver resolver, ExecutionOptions options)
    {
        await resolver.ExecuteAsync(options, x => Task.FromResult<ExecutionResult>(null!));

        var actual = await new DocumentExecuter().ExecuteAsync(options);

        if (actual.Streams is { Count: > 0 } && actual.Errors is not { Count: > 0 })
        {
            // Resolve the first stream actual with a timeout.
            var stream = actual.Streams.First();

            using (var cts = new CancellationTokenSource(5000))
            {
                actual = await stream.Value.FirstAsync().ToTask().WithCancellation(cts.Token);
            }
        }

        return actual;
    }

    protected CachingGraphQLResolver CreateSut(params Schema[] schemas)
    {
        var appProvider = A.Fake<IAppProvider>();

        A.CallTo(() => appProvider.GetSchemasAsync(TestApp.Default.Id, default))
            .Returns(schemas.ToList());

        var serviceProvider =
            new ServiceCollection()
                .AddLogging(options =>
                {
                    options.AddDebug();
                })
                .Configure<AssetOptions>(x =>
                {
                    x.CanCache = true;
                })
                .Configure<ContentOptions>(x =>
                {
                    x.CanCache = true;
                })
                .AddSingleton<StringReferenceExtractor>()
                .AddSingleton<IDocumentExecutionListener,
                    DataLoaderDocumentListener>()
                .AddSingleton<IDataLoaderContextAccessor,
                    DataLoaderContextAccessor>()
                .AddTransient<IAssetCache,
                    AssetCache>()
                .AddTransient<IContentCache,
                    ContentCache>()
                .AddSingleton<IUrlGenerator,
                    FakeUrlGenerator>()
                .AddSingleton(
                    A.Fake<ILoggerFactory>())
                .AddSingleton(
                    A.Fake<ISchemasHash>())
                .AddMemoryCache()
                .AddBackgroundCache()
                .AddSingleton(appProvider)
                .AddSingleton(assetQuery)
                .AddSingleton(commandBus)
                .AddSingleton(contentQuery)
                .AddSingleton(subscriptionService)
                .AddSingleton(userResolver)
                .BuildServiceProvider();

        return ActivatorUtilities.CreateInstance<CachingGraphQLResolver>(serviceProvider);
    }

    protected static Context MatchsAssetContext()
    {
        return A<Context>.That.Matches(x =>
            x.App == TestApp.Default &&
            x.NoCleanup() &&
           !x.NoAssetEnrichment());
    }

    protected static Context MatchsContentContext()
    {
        return A<Context>.That.Matches(x =>
            x.App == TestApp.Default &&
            x.NoCleanup() &&
            x.NoEnrichment());
    }
}
