// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Runtime.CompilerServices;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.DomainObject;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents;

public sealed class MongoShardedContentRepository : ShardedSnapshotStore<MongoContentRepository, ContentDomainObject.State>, IContentRepository
{
    public MongoShardedContentRepository(IShardingStrategy sharding, Func<string, MongoContentRepository> factory)
        : base(sharding, factory)
    {
    }

    public Task<IContentEntity?> FindContentAsync(IAppEntity app, ISchemaEntity schema, DomainId id, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).FindContentAsync(app, schema, id, scope, ct);
    }

    public Task<bool> HasReferrersAsync(DomainId appId, DomainId reference, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).HasReferrersAsync(appId, reference, scope, ct);
    }

    public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, List<ISchemaEntity> schemas, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).QueryAsync(app, schemas, q, scope, ct);
    }

    public Task<IResultList<IContentEntity>> QueryAsync(IAppEntity app, ISchemaEntity schema, Q q, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(app.Id).QueryAsync(app, schema, q, scope, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(DomainId appId, DomainId schemaId, FilterNode<ClrValue> filterNode, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).QueryIdsAsync(appId, schemaId, filterNode, scope, ct);
    }

    public Task<IReadOnlyList<ContentIdStatus>> QueryIdsAsync(DomainId appId, HashSet<DomainId> ids, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).QueryIdsAsync(appId, ids, scope, ct);
    }

    public Task ResetScheduledAsync(DomainId appId, DomainId id, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).ResetScheduledAsync(appId, id, scope, ct);
    }

    public IAsyncEnumerable<IContentEntity> StreamAll(DomainId appId, HashSet<DomainId>? schemaIds, SearchScope scope,
        CancellationToken ct = default)
    {

        return Shard(appId).StreamAll(appId, schemaIds, scope, ct);
    }

    public IAsyncEnumerable<IContentEntity> StreamReferencing(DomainId appId, DomainId references, int take, SearchScope scope,
        CancellationToken ct = default)
    {
        return Shard(appId).StreamReferencing(appId, references, take, scope, ct);
    }

    public async IAsyncEnumerable<IContentEntity> StreamScheduledWithoutDataAsync(Instant now, SearchScope scope,
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

    protected override string GetShardKey(ContentDomainObject.State state)
    {
        return GetShardKey(state.AppId.Id);
    }
}
