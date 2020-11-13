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
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetFolderRepository : ISnapshotStore<AssetFolderState, DomainId>
    {
        async Task<(AssetFolderState Value, long Version)> ISnapshotStore<AssetFolderState, DomainId>.ReadAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
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

        async Task ISnapshotStore<AssetFolderState, DomainId>.WriteAsync(DomainId key, AssetFolderState value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var entity = SimpleMapper.Map(value, new MongoAssetFolderEntity());

                entity.IndexedAppId = value.AppId.Id;

                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, entity);
            }
        }

        async Task ISnapshotStore<AssetFolderState, DomainId>.ReadAllAsync(Func<AssetFolderState, long, Task> callback, CancellationToken ct)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.Find(new BsonDocument(), options: Batching.Options).ForEachPipedAsync(x => callback(Map(x), x.Version), ct);
            }
        }

        async Task ISnapshotStore<AssetFolderState, DomainId>.RemoveAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.DeleteOneAsync(x => x.DocumentId == key);
            }
        }

        private static AssetFolderState Map(MongoAssetFolderEntity existing)
        {
            return SimpleMapper.Map(existing, new AssetFolderState());
        }
    }
}
