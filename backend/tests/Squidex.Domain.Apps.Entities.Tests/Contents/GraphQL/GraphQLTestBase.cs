// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Squidex.Caching;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Newtonsoft;
using Squidex.Shared;
using Squidex.Shared.Users;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLTestBase : IClassFixture<TranslationsFixture>
    {
        protected readonly GraphQLSerializer serializer = new GraphQLSerializer(options =>
        {
            options.Formatting = Formatting.Indented;
            options.Converters.Add(new JsonValueConverter());
            options.Converters.Add(new WriteonlyGeoJsonConverter());
        });

        protected readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        protected readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        protected readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        protected readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        protected readonly Context requestContext;
        private CachingGraphQLResolver sut;

        public GraphQLTestBase()
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

        protected Task<ExecutionResult> ExecuteAsync(ExecutionOptions options, string? permissionId = null)
        {
            var context = requestContext;

            if (permissionId != null)
            {
                var permission = Permissions.ForApp(permissionId, TestApp.Default.Name, TestSchemas.DefaultId.Name).Id;

                context = new Context(Mocks.FrontendUser(permission: permission), TestApp.Default);
            }

            return ExcecuteAsync(options, context);
        }

        private async Task<ExecutionResult> ExcecuteAsync(ExecutionOptions options, Context context)
        {
            sut ??= CreateSut(TestSchemas.Default, TestSchemas.Ref1, TestSchemas.Ref2);

            options.UserContext = ActivatorUtilities.CreateInstance<GraphQLExecutionContext>(sut.Services, context)!;

            foreach (var listener in sut.Services.GetRequiredService<IEnumerable<IDocumentExecutionListener>>())
            {
                options.Listeners.Add(listener);
            }

            await sut.ExecuteAsync(options, x => Task.FromResult<ExecutionResult>(null!));

            return await new DocumentExecuter().ExecuteAsync(options);
        }

        protected CachingGraphQLResolver CreateSut(params ISchemaEntity[] schemas)
        {
            var cache = new BackgroundCache(new MemoryCache(Options.Create(new MemoryCacheOptions())));

            var appProvider = A.Fake<IAppProvider>();

            A.CallTo(() => appProvider.GetSchemasAsync(TestApp.Default.Id, default))
                .Returns(schemas.ToList());

            var serviceProvider =
                new ServiceCollection()
                    .AddMemoryCache()
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
                    .AddSingleton(A.Fake<ILoggerFactory>())
                    .AddSingleton(appProvider)
                    .AddSingleton(assetQuery)
                    .AddSingleton(commandBus)
                    .AddSingleton(contentQuery)
                    .AddSingleton(userResolver)
                    .AddSingleton<InstantGraphType>()
                    .AddSingleton<JsonGraphType>()
                    .AddSingleton<JsonNoopGraphType>()
                    .AddSingleton<StringReferenceExtractor>()
                    .BuildServiceProvider();

            var schemasHash = A.Fake<ISchemasHash>();

            return new CachingGraphQLResolver(cache, schemasHash, serviceProvider, Options.Create(new GraphQLOptions()));
        }
    }
}
