// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
    public sealed partial class MongoAssetRepository : ISnapshotStore<AssetDomainObject.State, DomainId>
    {
        async Task<(AssetDomainObject.State Value, long Version)> ISnapshotStore<AssetDomainObject.State, DomainId>.ReadAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var existing =
                    await Collection.Find(x => x.DocumentId == key)
                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return (Map(existing), existing.Version);
                }

                return (null!, EtagVersion.NotFound);
            }
        }

        async Task ISnapshotStore<AssetDomainObject.State, DomainId>.WriteAsync(DomainId key, AssetDomainObject.State value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var entity = SimpleMapper.Map(value, new MongoAssetEntity());

                entity.IndexedAppId = value.AppId.Id;

                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, entity);
            }
        }

        async Task ISnapshotStore<AssetDomainObject.State, DomainId>.ReadAllAsync(Func<AssetDomainObject.State, long, Task> callback, CancellationToken ct)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                await Collection.Find(new BsonDocument(), options: Batching.Options).ForEachPipedAsync(x => callback(Map(x), x.Version), ct);
            }
        }

        async Task ISnapshotStore<AssetDomainObject.State, DomainId>.RemoveAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                await Collection.DeleteOneAsync(x => x.DocumentId == key);
            }
        }

        private static AssetDomainObject.State Map(MongoAssetEntity existing)
        {
            return SimpleMapper.Map(existing, new AssetDomainObject.State());
        }
    }
}
