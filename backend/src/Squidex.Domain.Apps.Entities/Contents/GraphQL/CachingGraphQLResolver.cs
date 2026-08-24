// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public sealed class CachingGraphQLResolver(
    IBackgroundCache cache,
    ISchemasHash schemasHash,
    IServiceProvider serviceProvider,
    IOptions<GraphQLOptions> options)
    : IConfigureExecution
{
    private readonly GraphQLOptions options = options.Value;

    private sealed record CacheEntry(GraphQLSchema Model, SchemasHashKey HashKey);

    public float SortOrder => 0;

    public IServiceProvider Services
    {
        get => serviceProvider;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionOptions options, ExecutionDelegate next)
    {
        var context = ((GraphQLExecutionContext)options.UserContext!).Context;

        options.Schema = await GetSchemaAsync(context.App);
        options.HandleError(serviceProvider);

        return await next(options);
    }

    public async Task<GraphQLSchema> GetSchemaAsync(App app)
    {
        var entry = await GetModelEntryAsync(app);

        return entry.Model;
    }

    private Task<CacheEntry> GetModelEntryAsync(App app)
    {
        if (options.CacheDuration <= TimeSpan.Zero)
        {
            return CreateModelAsync(app);
        }

        // The version is not part of the key. Building the schema is expensive and the version
        // changes for every app event, most of which do not affect the schema at all. The validator
        // below detects the changes that do, because SchemasHashKey contains the app version as
        // well as the version of every schema.
        var cacheKey = (typeof(CachingGraphQLResolver), app.Id);

        return cache.GetOrCreateAsync(cacheKey, options.CacheDuration, async entry =>
        {
            return await CreateModelAsync(app);
        },
        async entry =>
        {
            var hashKey = await schemasHash.GetCurrentHashAsync(app);

            return hashKey.Equals(entry.HashKey);
        });
    }

    private async Task<CacheEntry> CreateModelAsync(App app)
    {
        var schemasList = await serviceProvider.GetRequiredService<IAppProvider>().GetSchemasAsync(app.Id);
        var schemasKey = SchemasHashKey.Create(app, schemasList);

        return new CacheEntry(new Builder(app, options).BuildSchema(schemasList), schemasKey);
    }
}
