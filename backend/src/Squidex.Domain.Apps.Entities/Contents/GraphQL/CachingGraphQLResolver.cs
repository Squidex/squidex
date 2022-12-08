// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using GraphQL;
using GraphQL.DI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Caching;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
#pragma warning disable RECS0082 // Parameter has the same name as a member and hides it

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public sealed class CachingGraphQLResolver : IConfigureExecution
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private readonly IBackgroundCache cache;
    private readonly ISchemasHash schemasHash;
    private readonly IServiceProvider serviceProvider;
    private readonly GraphQLOptions options;

    private sealed record CacheEntry(GraphQLSchema Model, string Hash, Instant Created);

    public float SortOrder => 0;

    public IServiceProvider Services
    {
        get => serviceProvider;
    }

    public CachingGraphQLResolver(IBackgroundCache cache, ISchemasHash schemasHash, IServiceProvider serviceProvider,
        IOptions<GraphQLOptions> options)
    {
        this.cache = cache;
        this.schemasHash = schemasHash;
        this.serviceProvider = serviceProvider;
        this.options = options.Value;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionOptions options, ExecutionDelegate next)
    {
        var context = ((GraphQLExecutionContext)options.UserContext!).Context;

        options.Schema = await GetSchemaAsync(context.App);

        return await next(options);
    }

    public async Task<GraphQLSchema> GetSchemaAsync(IAppEntity app)
    {
        var entry = await GetModelEntryAsync(app);

        return entry.Model;
    }

    private Task<CacheEntry> GetModelEntryAsync(IAppEntity app)
    {
        if (options.CacheDuration <= 0)
        {
            return CreateModelAsync(app);
        }

        var cacheKey = CreateCacheKey(app.Id, app.Version.ToString(CultureInfo.InvariantCulture));

        return cache.GetOrCreateAsync(cacheKey, CacheDuration, async entry =>
        {
            return await CreateModelAsync(app);
        },
        async entry =>
        {
            var (created, hash) = await schemasHash.GetCurrentHashAsync(app);

            return created < entry.Created || string.Equals(hash, entry.Hash, StringComparison.OrdinalIgnoreCase);
        });
    }

    private async Task<CacheEntry> CreateModelAsync(IAppEntity app)
    {
        var schemasList = await serviceProvider.GetRequiredService<IAppProvider>().GetSchemasAsync(app.Id);
        var schemasKey = await schemasHash.ComputeHashAsync(app, schemasList);

        var now = SystemClock.Instance.GetCurrentInstant();

        return new CacheEntry(new Builder(app, options).BuildSchema(schemasList), schemasKey, now);
    }

    private static object CreateCacheKey(DomainId appId, string etag)
    {
        return $"GraphQLModel_{appId}_{etag}";
    }
}
