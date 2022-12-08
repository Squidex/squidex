// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.RegularExpressions;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Tasks;
using Squidex.Messaging.Subscriptions;
using Squidex.Shared;
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

    protected GraphQLTestBase()
    {
        A.CallTo(() => userResolver.QueryManyAsync(A<string[]>._, default))
            .ReturnsLazily(x =>
            {
                var ids = x.GetArgument<string[]>(0)!;

                var users = ids.Select(id => UserMocks.User(id, $"{id}@email.com", $"name_{id}"));

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

    protected Task<ExecutionResult> ExecuteAsync(ExecutionOptions options)
    {
        return ExecuteCoreAsync(options, requestContext);
    }

    protected Task<ExecutionResult> ExecuteAsync(ExecutionOptions options, string permissionId)
    {
        return ExecuteCoreAsync(options, BuildContext(permissionId));
    }

    protected async Task<ExecutionResult> ExecuteCoreAsync(ExecutionOptions options, Context context)
    {
        // Use a shared instance to test caching.
        sut ??= CreateSut(TestSchemas.Default, TestSchemas.Ref1, TestSchemas.Ref2);

        // Provide the context to the test if services need to be resolved.
        var graphQLContext = ActivatorUtilities.CreateInstance<GraphQLExecutionContext>(sut.Services, context)!;

        options.UserContext = graphQLContext;

        // Register data loader and other listeners.
        foreach (var listener in sut.Services.GetRequiredService<IEnumerable<IDocumentExecutionListener>>())
        {
            options.Listeners.Add(listener);
        }

        // Enrich the context with the schema.
        await sut.ExecuteAsync(options, x => Task.FromResult<ExecutionResult>(null!));

        var actual = await new DocumentExecuter().ExecuteAsync(options);

        if (actual.Streams?.Count > 0 && actual.Errors?.Any() != true)
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

    private static Context BuildContext(string permissionId)
    {
        var permission = PermissionIds.ForApp(permissionId, TestApp.Default.Name, TestSchemas.DefaultId.Name).Id;

        return new Context(Mocks.FrontendUser(permission: permission), TestApp.Default);
    }

    protected CachingGraphQLResolver CreateSut(params ISchemaEntity[] schemas)
    {
        var appProvider = A.Fake<IAppProvider>();

        A.CallTo(() => appProvider.GetSchemasAsync(TestApp.Default.Id, default))
            .Returns(schemas.ToList());

        var serviceProvider =
            new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .AddBackgroundCache()
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
                .AddSingleton(appProvider)
                .AddSingleton(assetQuery)
                .AddSingleton(commandBus)
                .AddSingleton(contentQuery)
                .AddSingleton(subscriptionService)
                .AddSingleton(userResolver)
                .BuildServiceProvider();

        return ActivatorUtilities.CreateInstance<CachingGraphQLResolver>(serviceProvider);
    }

    protected static string CreateQuery(string query, DomainId id = default, IEnrichedContentEntity? content = null)
    {
        query = query
            .Replace("'", "\"", StringComparison.Ordinal)
            .Replace("`", "\"", StringComparison.Ordinal)
            .Replace("<FIELDS_ASSET>", TestAsset.AllFields, StringComparison.Ordinal)
            .Replace("<FIELDS_CONTENT>", TestContent.AllFields, StringComparison.Ordinal)
            .Replace("<FIELDS_CONTENT_FLAT>", TestContent.AllFlatFields, StringComparison.Ordinal);

        if (id != default)
        {
            query = query.Replace("<ID>", id.ToString(), StringComparison.Ordinal);
        }

        if (query.Contains("<DATA>", StringComparison.Ordinal) && content != null)
        {
            var data = TestContent.Input(content, TestSchemas.Ref1.Id, TestSchemas.Ref2.Id);

            // Json is not the same as the input format of graphql, therefore we need to convert it.
            var dataJson = TestUtils.DefaultSerializer.Serialize(data, true);

            // Use properties without quotes.
            dataJson = Regex.Replace(dataJson, "\"([^\"]+)\":", x => $"{x.Groups[1].Value}:");

            // Use enum values whithout quotes.
            dataJson = Regex.Replace(dataJson, "\"Enum([A-Za-z]+)\"", x => $"Enum{x.Groups[1].Value}");

            query = query.Replace("<DATA>", dataJson, StringComparison.Ordinal);
        }

        return query;
    }

    protected Context MatchsAssetContext()
    {
        return A<Context>.That.Matches(x =>
            x.App == TestApp.Default &&
            x.ShouldSkipCleanup() &&
            x.ShouldSkipContentEnrichment() &&
            x.UserPrincipal == requestContext.UserPrincipal);
    }

    protected Context MatchsContentContext()
    {
        return A<Context>.That.Matches(x =>
            x.App == TestApp.Default &&
            x.ShouldSkipCleanup() &&
            x.ShouldSkipContentEnrichment() &&
            x.UserPrincipal == requestContext.UserPrincipal);
    }
}
