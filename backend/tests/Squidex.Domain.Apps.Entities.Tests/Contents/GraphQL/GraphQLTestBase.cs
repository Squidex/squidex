// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reactive.Linq;
using System.Text.RegularExpressions;
using FakeItEasy;
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
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Tasks;
using Squidex.Shared;
using Squidex.Shared.Users;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public abstract class GraphQLTestBase : IClassFixture<TranslationsFixture>
    {
        protected readonly GraphQLSerializer serializer = new GraphQLSerializer(TestUtils.DefaultOptions());
        protected readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        protected readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        protected readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
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

        protected void AssertResult(object expected, ExecutionResult result)
        {
            var jsonOutputResult = serializer.Serialize(result);
            var isonOutputExpected = serializer.Serialize(expected);

            Assert.Equal(isonOutputExpected, jsonOutputResult);
        }

        protected Task<ExecutionResult> ExecuteAsync(ExecutionOptions options)
        {
            var (result, _) = ExecuteCoreAsync(options);

            return result;
        }

        protected Task<ExecutionResult> ExecuteAsync(ExecutionOptions options, string permissionId)
        {
            var (result, _) = ExecuteCoreAsync(options, BuildContext(permissionId));

            return result;
        }

        protected (Task<ExecutionResult>, GraphQLExecutionContext) ExecuteCoreAsync(ExecutionOptions options)
        {
            return ExecuteCoreAsync(options, requestContext);
        }

        protected (Task<ExecutionResult>, GraphQLExecutionContext) ExecuteCoreAsync(ExecutionOptions options, Context context)
        {
            // Use a shared instance to test caching.
            sut ??= CreateSut(TestSchemas.Default, TestSchemas.Ref1, TestSchemas.Ref2);

            // Provide the context to the test if services need to be resolved.
            var graphQLContext = ActivatorUtilities.CreateInstance<GraphQLExecutionContext>(sut.Services, context)!;

            foreach (var listener in sut.Services.GetRequiredService<IEnumerable<IDocumentExecutionListener>>())
            {
                options.Listeners.Add(listener);
            }

            options.UserContext = graphQLContext;

            return (ExecuteOptionsAsync(options), graphQLContext);
        }

        private async Task<ExecutionResult> ExecuteOptionsAsync(ExecutionOptions options)
        {
            await sut!.ExecuteAsync(options, x => Task.FromResult<ExecutionResult>(null!));

            var result = await new DocumentExecuter().ExecuteAsync(options);

            if (result.Streams?.Count > 0 && result.Errors?.Any() != true)
            {
                var stream = result.Streams.First();

                async Task<ExecutionResult> FirstAsync()
                {
                    var result = await stream.Value.FirstOrDefaultAsync();

                    return result;
                }

                using (var cts = new CancellationTokenSource(5000))
                {
                    result = await FirstAsync().WithCancellation(cts.Token);
                }
            }

            return result;
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
                    .AddMessaging()
                    .AddMessagingSubscriptions()
                    .AddTransient<GraphQLExecutionContext>()
                    .Configure<AssetOptions>(x =>
                    {
                        x.CanCache = true;
                    })
                    .Configure<ContentOptions>(x =>
                    {
                        x.CanCache = true;
                    })
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
                    .AddSingleton(userResolver)
                    .AddSingleton<CachingGraphQLResolver>()
                    .AddSingleton<InstantGraphType>()
                    .AddSingleton<JsonGraphType>()
                    .AddSingleton<JsonNoopGraphType>()
                    .AddSingleton<StringReferenceExtractor>()
                    .BuildServiceProvider();

            return serviceProvider.GetRequiredService<CachingGraphQLResolver>();
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

                var dataJson = TestUtils.DefaultSerializer.Serialize(data, true);

                // Use Properties without quotes.
                dataJson = Regex.Replace(dataJson, "\"([^\"]+)\":", x => x.Groups[1].Value + ":");

                // Use pure integer numbers.
                dataJson = dataJson.Replace(".0", string.Empty, StringComparison.Ordinal);

                // Use enum values whithout quotes.
                dataJson = dataJson.Replace("\"EnumA\"", "EnumA", StringComparison.Ordinal);
                dataJson = dataJson.Replace("\"EnumB\"", "EnumB", StringComparison.Ordinal);

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
                x.User == requestContext.User);
        }

        protected Context MatchsContentContext()
        {
            return A<Context>.That.Matches(x =>
                x.App == TestApp.Default &&
                x.ShouldSkipCleanup() &&
                x.ShouldSkipContentEnrichment() &&
                x.User == requestContext.User);
        }
    }
}
