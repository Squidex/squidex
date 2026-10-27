// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Execution;
using GraphQL.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps.Indexes;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Indexes;
using Squidex.Domain.Apps.Entities.Teams.Indexes;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public class CachingGraphQLResolverTests : GraphQLTestBase
{
    [Fact]
    public async Task Should_not_use_schemas_from_request_cache_to_build_model()
    {
        using var cts = new CancellationTokenSource();

        await cts.CancelAsync();

        var schemasIndex = A.Fake<ISchemasIndex>();

        A.CallTo(() => schemasIndex.GetSchemasAsync(TestApp.Default.Id, cts.Token))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        A.CallTo(() => schemasIndex.GetSchemasAsync(TestApp.Default.Id, default))
            .Returns([TestSchemas.Default]);

        var localCache = new AsyncLocalCache();
        var appProvider = new AppProvider(
            A.Fake<IAppsIndex>(),
            A.Fake<IRulesIndex>(),
            schemasIndex,
            A.Fake<ITeamsIndex>(),
            localCache);

        var sut = CreateSut(appProvider);

        using (localCache.StartContext())
        {
            // The request has loaded the schemas with its own token, but has been canceled.
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => appProvider.GetSchemasAsync(TestApp.Default.Id, cts.Token));

            var model = await sut.GetSchemaAsync(TestApp.Default);

            Assert.NotNull(model);
        }
    }

    [Fact]
    public async Task Should_not_rebuild_model_if_hash_has_not_changed()
    {
        var appProvider = A.Fake<IAppProvider>();

        A.CallTo(() => appProvider.GetSchemasAsync(TestApp.Default.Id, default))
            .Returns([TestSchemas.Default]);

        // The hash store does not know the schema, for example after a restore.
        var hashKey = SchemasHashKey.Create(TestApp.Default, new Dictionary<DomainId, long>());

        var schemasHash = A.Fake<ISchemasHash>();

        A.CallTo(() => schemasHash.GetCurrentHashAsync(TestApp.Default, default))
            .Returns(hashKey);

        var sut = CreateSut(appProvider, schemasHash);

        await sut.GetSchemaAsync(TestApp.Default);
        await sut.GetSchemaAsync(TestApp.Default);

        // Wait for the validation in the background.
        await Task.Delay(500);

        await sut.GetSchemaAsync(TestApp.Default);

        A.CallTo(() => appProvider.GetSchemasAsync(TestApp.Default.Id, default))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_log_error_if_model_cannot_be_created()
    {
        var appProvider = A.Fake<IAppProvider>();

        A.CallTo(() => appProvider.GetSchemasAsync(TestApp.Default.Id, default))
            .ThrowsAsync(new InvalidOperationException());

        var sut = CreateSut(appProvider);

        var options = new TestQuery { Query = "{ __typename }" }.ToOptions(sut.Services);

        var executer = new DocumentExecuter(
            new GraphQLDocumentBuilder(),
            new DocumentValidator(),
            new DefaultExecutionStrategySelector(),
            [sut]);

        var result = await executer.ExecuteAsync(options);

        Assert.NotEmpty(result.Errors!);

        var loggerFactory = sut.Services.GetRequiredService<ILoggerFactory>();

        A.CallTo(() => loggerFactory.CreateLogger("GraphQL"))
            .MustHaveHappened();
    }
}
