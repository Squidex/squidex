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
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class MongoShardedContentRepository(IShardingStrategy sharding, Func<string, MongoContentRepository> factory)
    : ShardedSnapshotStore<MongoContentRepository, WriteContent>(sharding, factory, x => x.AppId.Id), IContentRepository, IDeleter
{
    public Task<Content?> FindContentAsync(App app, Schema schema, DomainId id, SearchScope scope,
        IEnumerable<string>? fields,
        CancellationToken ct = default)
    {
        return Shard(app.Id).FindContentAsync(app, schema, id, scope, fields, ct);
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

    public IAsyncEnumerable<DomainId> StreamIds(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
       CancellationToken ct = default)
    {
        return Shard(appId).StreamIds(appId, schemaIds, scope, ct);
    }

    public IAsyncEnumerable<Content> StreamReferencing(DomainId appId, DomainId references, int take, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).StreamReferencing(appId, references, take, scope, ct);
    }

    public Task CreateIndexAsync(DomainId appId, DomainId schemaId, IndexDefinition index,
        CancellationToken ct = default)
    {
        return Shard(appId).CreateIndexAsync(appId, schemaId, index, ct);
    }

    public Task DropIndexAsync(DomainId appId, DomainId schemaId, string name,
        CancellationToken ct = default)
    {
        return Shard(appId).DropIndexAsync(appId, schemaId, name, ct);
    }

    public Task<List<IndexDefinition>> GetIndexesAsync(DomainId appId, DomainId schemaId,
        CancellationToken ct = default)
    {
        return Shard(appId).GetIndexesAsync(appId, schemaId, ct);
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
