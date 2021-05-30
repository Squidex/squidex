﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.Execution;
using GraphQL.NewtonsoftJson;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Squidex.Caching;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Log;
using Squidex.Shared;
using Squidex.Shared.Users;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLTestBase : IClassFixture<TranslationsFixture>
    {
        protected readonly IJsonSerializer serializer =
            TestUtils.CreateSerializer(TypeNameHandling.None,
                new ExecutionResultJsonConverter(new ErrorInfoProvider()));
        protected readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        protected readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        protected readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        protected readonly IUserResolver userResolver = A.Fake<IUserResolver>();
        protected readonly Context requestContext;

        public GraphQLTestBase()
        {
            A.CallTo(() => userResolver.QueryManyAsync(A<string[]>._))
                .ReturnsLazily(x =>
                {
                    var ids = x.GetArgument<string[]>(0)!;

                    var users = ids.Select(id => UserMocks.User(id, $"{id}@email.com", $"name_{id}"));

                    return Task.FromResult(users.ToDictionary(x => x.Id));
                });

            var schemaDef =
                new Schema(schemaId.Name)
                    .Publish()
                    .AddJson(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties())
                    .AddString(2, "my-string", Partitioning.Language,
                        new StringFieldProperties())
                    .AddString(3, "my-string2", Partitioning.Invariant,
                        new StringFieldProperties())
                    .AddString(4, "my-localized", Partitioning.Language,
                        new StringFieldProperties())
                    .AddNumber(5, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddNumber(6, "my_number", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddAssets(7, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties())
                    .AddBoolean(8, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties())
                    .AddDateTime(9, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties())
                    .AddReferences(10, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = schemaRefId1.Id })
                    .AddReferences(11, "my-union", Partitioning.Invariant,
                        new ReferencesFieldProperties())
                    .AddReferences(12, "my-invalid", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = DomainId.NewGuid() })
                    .AddGeolocation(13, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties())
                    .AddComponent(14, "my-component", Partitioning.Invariant,
                        new ComponentFieldProperties())
                    .AddComponents(15, "my-components", Partitioning.Invariant,
                        new ComponentsFieldProperties())
                    .AddTags(16, "my-tags", Partitioning.Invariant,
                        new TagsFieldProperties())
                    .AddArray(17, "my-empty-array", Partitioning.Invariant, null,
                        new ArrayFieldProperties())
                    .AddNumber(50, "2_numbers", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddNumber(51, "2-numbers", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddNumber(52, "content", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddArray(100, "my-array", Partitioning.Invariant, f => f
                        .AddBoolean(121, "nested-boolean")
                        .AddNumber(122, "nested-number")
                        .AddNumber(123, "nested_number"))
                    .SetScripts(new SchemaScripts { Query = "<query-script>" });

            schema = Mocks.Schema(appId, schemaId, schemaDef);

            var schemaRef1Def =
                new Schema(schemaRefId1.Name)
                    .Publish()
                    .AddString(1, "ref1-field", Partitioning.Invariant);

            schemaRef1 = Mocks.Schema(appId, schemaRefId1, schemaRef1Def);

            var schemaRef2Def =
                new Schema(schemaRefId2.Name)
                    .Publish()
                    .AddString(1, "ref2-field", Partitioning.Invariant);

            schemaRef2 = Mocks.Schema(appId, schemaRefId2, schemaRef2Def);

            var schemaInvalidNameDef =
                new Schema(schemaInvalidNameId.Name)
                    .Publish()
                    .AddString(1, "my-field", Partitioning.Invariant);

            schemaInvalidName = Mocks.Schema(appId, schemaInvalidNameId, schemaInvalidNameDef);

            requestContext = new Context(Mocks.FrontendUser(), app);

            sut = CreateSut();
        }

        protected void AssertResult(object expected, ExecutionResult result)
        {
            var resultJson = serializer.Serialize(result, true);
            var expectJson = serializer.Serialize(expected, true);

            Assert.Equal(expectJson, resultJson);
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

        private Task<ExecutionResult> ExcecuteAsync(ExecutionOptions options, Context context)
        {
            var sut = CreateSut(TestSchemas.Default, TestSchemas.Ref1, TestSchemas.Ref2);

            options.UserContext = ActivatorUtilities.CreateInstance<GraphQLExecutionContext>(sut.Services, context);

            var listener = sut.Services.GetService<DataLoaderDocumentListener>();

            if (listener != null)
            {
                options.Listeners.Add(listener);
            }

            return sut.ExecuteAsync(options);
        }

        protected CachingGraphQLService CreateSut(params ISchemaEntity[] schemas)
        {
            var cache = new BackgroundCache(new MemoryCache(Options.Create(new MemoryCacheOptions())));

            var appProvider = A.Fake<IAppProvider>();

            A.CallTo(() => appProvider.GetSchemasAsync(TestApp.Default.Id))
                .Returns(schemas.ToList());

            var dataLoaderContext = (IDataLoaderContextAccessor)new DataLoaderContextAccessor();
            var dataLoaderListener = new DataLoaderDocumentListener(dataLoaderContext);

            var services =
                new ServiceCollection()
                    .AddMemoryCache()
                    .AddTransient<GraphQLExecutionContext>()
                    .AddSingleton(A.Fake<ISemanticLog>())
                    .AddSingleton(appProvider)
                    .AddSingleton(assetQuery)
                    .AddSingleton(commandBus)
                    .AddSingleton(contentQuery)
                    .AddSingleton(dataLoaderContext)
                    .AddSingleton(dataLoaderListener)
                    .AddSingleton(userResolver)
                    .AddSingleton<InstantGraphType>()
                    .AddSingleton<JsonGraphType>()
                    .AddSingleton<JsonNoopGraphType>()
                    .AddSingleton<SharedTypes>()
                    .AddSingleton<IUrlGenerator,
                        FakeUrlGenerator>()
                    .BuildServiceProvider();

            var schemasHash = A.Fake<ISchemasHash>();

            return new CachingGraphQLService(cache, schemasHash, services, Options.Create(new GraphQLOptions()));
        }
    }
}
