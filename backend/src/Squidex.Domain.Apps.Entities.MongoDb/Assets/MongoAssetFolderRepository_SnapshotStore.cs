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
    public sealed partial class MongoAssetFolderRepository : ISnapshotStore<AssetFolderState, Guid>
    {
        async Task<(AssetFolderState Value, long Version)> ISnapshotStore<AssetFolderState, Guid>.ReadAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
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

        async Task ISnapshotStore<AssetFolderState, Guid>.WriteAsync(Guid key, AssetFolderState value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var entity = SimpleMapper.Map(value, new MongoAssetFolderEntity());

                entity.Version = newVersion;
                entity.IndexedAppId = value.AppId.Id;

                await Collection.ReplaceOneAsync(x => x.Id == key && x.Version == oldVersion, entity, UpsertReplace);
            }
        }

        async Task ISnapshotStore<AssetFolderState, Guid>.ReadAllAsync(Func<AssetFolderState, long, Task> callback, CancellationToken ct)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.Find(new BsonDocument(), options: Batching.Options).ForEachPipelineAsync(x => callback(Map(x), x.Version), ct);
            }
        }

        async Task ISnapshotStore<AssetFolderState, Guid>.RemoveAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.DeleteOneAsync(x => x.Id == key);
            }
        }

        private static AssetFolderState Map(MongoAssetFolderEntity existing)
        {
            return SimpleMapper.Map(existing, new AssetFolderState());
        }
    }
}
