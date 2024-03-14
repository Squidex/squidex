// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public sealed class MongoShardedContentRepository : ShardedSnapshotStore<MongoContentRepository, WriteContent>, IContentRepository, IDeleter
{
    public MongoShardedContentRepository(IShardingStrategy sharding, Func<string, MongoContentRepository> factory)
        : base(sharding, factory, x => x.AppId.Id)
    {
    }

    public Task<Content?> FindContentAsync(App app, Schema schema, DomainId id, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).FindContentAsync(app, schema, id, scope, ct);
    }

    public Task<bool> HasReferrersAsync(App app, DomainId reference, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).HasReferrersAsync(app, reference, scope, ct);
    }

    public Task<IResultList<Content>> QueryAsync(App app, List<Schema> schemas, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).QueryAsync(app, schemas, q, scope, ct);
    }

    public Task<IResultList<Content>> QueryAsync(App app, Schema schema, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).QueryAsync(app, schema, q, scope, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, Schema schema, FilterNode<ClrValue> filterNode, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).QueryIdsAsync(app, schema, filterNode, scope, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(App app, HashSet<DomainId> ids, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).QueryIdsAsync(app, ids, scope, ct);
    }

    public Task ResetScheduledAsync(DomainId appId, DomainId id, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).ResetScheduledAsync(appId, id, scope, ct);
    }

    public IAsyncEnumerable<Content> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).StreamAll(appId, schemaIds, scope, ct);
    }

    public IAsyncEnumerable<Content> StreamReferencing(DomainId appId, DomainId references, int take, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).StreamReferencing(appId, references, take, scope, ct);
    }

    public async IAsyncEnumerable<Content> StreamScheduledWithoutDataAsync(Instant now, SearchScope scope,
       [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var shard in Shards)
        {
            await foreach (var content in shard.StreamScheduledWithoutDataAsync(now, scope, ct))
            {
                yield return content;
            }
        }
    }
}
