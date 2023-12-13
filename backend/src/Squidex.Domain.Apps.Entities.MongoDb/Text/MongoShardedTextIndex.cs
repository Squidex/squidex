// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Text;

public sealed class MongoShardedTextIndex<T> : ShardedService<MongoTextIndexBase<T>>, ITextIndex where T : class
{
    public MongoShardedTextIndex(IShardingStrategy sharding, Func<string, MongoTextIndexBase<T>> factory)
        : base(sharding, factory)
    {
    }

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
        // Some commands might share a shared, therefore we don't group by app id.
        foreach (var byShard in commands.GroupBy(c => GetShardKey(c.AppId.Id)))
        {
            var shard = Shard(byShard.Key);

            await shard.ExecuteAsync(byShard.ToArray(), ct);
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
}
