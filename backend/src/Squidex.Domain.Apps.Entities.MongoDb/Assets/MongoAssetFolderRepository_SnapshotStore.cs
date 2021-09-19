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
    public sealed partial class MongoAssetFolderRepository : ISnapshotStore<AssetFolderDomainObject.State>, IDeleter
    {
        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
        }

        IAsyncEnumerable<(AssetFolderDomainObject.State State, long Version)> ISnapshotStore<AssetFolderDomainObject.State>.ReadAllAsync(
            CancellationToken ct)
        {
            return Collection.Find(new BsonDocument(), Batching.Options).ToAsyncEnumerable(ct).Select(x => (Map(x), x.Version));
        }

        async Task<(AssetFolderDomainObject.State Value, bool Valid, long Version)> ISnapshotStore<AssetFolderDomainObject.State>.ReadAsync(DomainId key,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/ReadAsync"))
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

        async Task ISnapshotStore<AssetFolderDomainObject.State>.WriteAsync(DomainId key, AssetFolderDomainObject.State value, long oldVersion, long newVersion,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/WriteAsync"))
            {
                var entity = Map(value);

                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, entity, ct);
            }
        }

        async Task ISnapshotStore<AssetFolderDomainObject.State>.WriteManyAsync(IEnumerable<(DomainId Key, AssetFolderDomainObject.State Value, long Version)> snapshots,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/WriteManyAsync"))
            {
                var updates = snapshots.Select(Map).Select(x =>
                    new ReplaceOneModel<MongoAssetFolderEntity>(
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

        async Task ISnapshotStore<AssetFolderDomainObject.State>.RemoveAsync(DomainId key,
            CancellationToken ct)
        {
            using (Telemetry.Activities.StartActivity("MongoAssetFolderRepository/RemoveAsync"))
            {
                await Collection.DeleteOneAsync(x => x.DocumentId == key, ct);
            }
        }

        private static MongoAssetFolderEntity Map(AssetFolderDomainObject.State value)
        {
            var entity = SimpleMapper.Map(value, new MongoAssetFolderEntity());

            entity.IndexedAppId = value.AppId.Id;

            return entity;
        }

        private static MongoAssetFolderEntity Map((DomainId Key, AssetFolderDomainObject.State Value, long Version) snapshot)
        {
            var entity = Map(snapshot.Value);

            entity.DocumentId = snapshot.Key;

            return entity;
        }

        private static AssetFolderDomainObject.State Map(MongoAssetFolderEntity existing)
        {
            return SimpleMapper.Map(existing, new AssetFolderDomainObject.State());
        }
    }
}
