﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public sealed class MongoShardedTextIndex<T>(IShardingStrategy sharding, Func<string, MongoTextIndexBase<T>> factory) : ShardedService<DomainId, MongoTextIndexBase<T>>(sharding, factory), ITextIndex, IDeleter where T : class
{
    public async Task ClearAsync(
        CancellationToken ct = default)
    {
        foreach (var shard in Shards)
        {
            await shard.ClearAsync(ct);
        }
    }

    public async Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default)
    {
        // Reduce the number of writes by grouping by shard.
        foreach (var byShard in commands.GroupBy(c => Shard(c.UniqueContentId.AppId)))
        {
            await byShard.Key.ExecuteAsync(byShard.ToArray(), ct);
        }
    }

    public Task<List<DomainId>?> SearchAsync(App app, TextQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).SearchAsync(app, query, scope, ct);
    }

    public Task<List<DomainId>?> SearchAsync(App app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).SearchAsync(app, query, scope, ct);
    }

    async Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        if (Shard(app.Id) is IDeleter shard)
        {
            await shard.DeleteAppAsync(app, ct);
        }
    }

    async Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        if (Shard(app.Id) is IDeleter shard)
        {
            await shard.DeleteSchemaAsync(app, schema, ct);
        }
    }
}
