// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets.DomainObject;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetFolderRepository : ISnapshotStore<AssetFolderDomainObject.State>
    {
        async Task<(AssetFolderDomainObject.State Value, bool Valid, long Version)> ISnapshotStore<AssetFolderDomainObject.State>.ReadAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var existing =
                    await Collection.Find(x => x.DocumentId == key)
                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return (Map(existing), true, existing.Version);
                }

                return (null!, true, EtagVersion.Empty);
            }
        }

        async Task ISnapshotStore<AssetFolderDomainObject.State>.WriteAsync(DomainId key, AssetFolderDomainObject.State value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var entity = Map(value);

                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, entity);
            }
        }

        async Task ISnapshotStore<AssetFolderDomainObject.State>.WriteManyAsync(IEnumerable<(DomainId Key, AssetFolderDomainObject.State Value, long Version)> snapshots)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var entities = snapshots.Select(Map).ToList();

                if (entities.Count == 0)
                {
                    return;
                }

                await Collection.InsertManyAsync(entities, InsertUnordered);
            }
        }

        async Task ISnapshotStore<AssetFolderDomainObject.State>.ReadAllAsync(Func<AssetFolderDomainObject.State, long, Task> callback,
            CancellationToken ct)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.Find(new BsonDocument(), Batching.Options).ForEachPipedAsync(x => callback(Map(x), x.Version), ct);
            }
        }

        async Task ISnapshotStore<AssetFolderDomainObject.State>.RemoveAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.DeleteOneAsync(x => x.DocumentId == key);
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
