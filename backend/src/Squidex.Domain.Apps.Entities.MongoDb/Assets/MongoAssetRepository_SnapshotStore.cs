// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : ISnapshotStore<AssetDomainObject.State>, IDeleter
    {
        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
        }

        IAsyncEnumerable<(AssetDomainObject.State State, long Version)> ISnapshotStore<AssetDomainObject.State>.ReadAllAsync(
            CancellationToken ct)
        {
            return Collection.Find(new BsonDocument(), Batching.Options).ToAsyncEnumerable(ct).Select(x => (Map(x), x.Version));
        }

        async Task<(AssetDomainObject.State Value, bool Valid, long Version)> ISnapshotStore<AssetDomainObject.State>.ReadAsync(DomainId key,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetRepository/ReadAsync"))
            {
                var existing =
                    await Collection.Find(x => x.DocumentId == key)
                        .FirstOrDefaultAsync(ct);

                if (existing != null)
                {
                    return (Map(existing), true, existing.Version);
                }

                return (null!, true, EtagVersion.Empty);
            }
        }

        async Task ISnapshotStore<AssetDomainObject.State>.WriteAsync(DomainId key, AssetDomainObject.State value, long oldVersion, long newVersion,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetRepository/WriteAsync"))
            {
                var entity = Map(value);

                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, entity, ct);
            }
        }

        async Task ISnapshotStore<AssetDomainObject.State>.WriteManyAsync(IEnumerable<(DomainId Key, AssetDomainObject.State Value, long Version)> snapshots,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetRepository/WriteManyAsync"))
            {
                var updates = snapshots.Select(Map).Select(x =>
                    new ReplaceOneModel<MongoAssetEntity>(
                        Filter.Eq(y => y.DocumentId, x.DocumentId),
                        x)
                    {
                        IsUpsert = true
                    }).ToList();

                if (updates.Count == 0)
                {
                    return;
                }

                await Collection.BulkWriteAsync(updates, BulkUnordered, ct);
            }
        }

        async Task ISnapshotStore<AssetDomainObject.State>.RemoveAsync(DomainId key,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetRepository/RemoveAsync"))
            {
                await Collection.DeleteOneAsync(x => x.DocumentId == key, ct);
            }
        }

        private static MongoAssetEntity Map(AssetDomainObject.State value)
        {
            var entity = SimpleMapper.Map(value, new MongoAssetEntity());

            entity.IndexedAppId = value.AppId.Id;

            return entity;
        }

        private static MongoAssetEntity Map((DomainId Key, AssetDomainObject.State Value, long Version) snapshot)
        {
            var entity = Map(snapshot.Value);

            entity.DocumentId = snapshot.Key;

            return entity;
        }

        private static AssetDomainObject.State Map(MongoAssetEntity existing)
        {
            return SimpleMapper.Map(existing, new AssetDomainObject.State());
        }
    }
}
