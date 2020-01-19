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
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : ISnapshotStore<AssetState, Guid>
    {
        async Task<(AssetState Value, long Version)> ISnapshotStore<AssetState, Guid>.ReadAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var existing =
                    await Collection.Find(x => x.Id == key)
                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return (Map(existing), existing.Version);
                }

                return (null!, EtagVersion.NotFound);
            }
        }

        async Task ISnapshotStore<AssetState, Guid>.WriteAsync(Guid key, AssetState value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var entity = SimpleMapper.Map(value, new MongoAssetEntity());

                entity.Version = newVersion;
                entity.IndexedAppId = value.AppId.Id;

                await Collection.UpsertVersionedAsync(key, oldVersion, entity);
            }
        }

        async Task ISnapshotStore<AssetState, Guid>.ReadAllAsync(Func<AssetState, long, Task> callback, CancellationToken ct)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                await Collection.Find(new BsonDocument(), options: Batching.Options).ForEachPipelineAsync(x => callback(Map(x), x.Version), ct);
            }
        }

        async Task ISnapshotStore<AssetState, Guid>.RemoveAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                await Collection.DeleteOneAsync(x => x.Id == key);
            }
        }

        private static AssetState Map(MongoAssetEntity existing)
        {
            return SimpleMapper.Map(existing, new AssetState());
        }
    }
}
