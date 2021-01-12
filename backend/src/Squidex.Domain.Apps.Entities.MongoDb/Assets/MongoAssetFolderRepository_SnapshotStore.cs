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
    public sealed partial class MongoAssetFolderRepository : ISnapshotStore<AssetFolderDomainObject.State, DomainId>
    {
        async Task<(AssetFolderDomainObject.State Value, long Version)> ISnapshotStore<AssetFolderDomainObject.State, DomainId>.ReadAsync(DomainId key)
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

        async Task ISnapshotStore<AssetFolderDomainObject.State, DomainId>.WriteAsync(DomainId key, AssetFolderDomainObject.State value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                var entity = SimpleMapper.Map(value, new MongoAssetFolderEntity());

                entity.IndexedAppId = value.AppId.Id;

                await Collection.UpsertVersionedAsync(key, oldVersion, newVersion, entity);
            }
        }

        async Task ISnapshotStore<AssetFolderDomainObject.State, DomainId>.ReadAllAsync(Func<AssetFolderDomainObject.State, long, Task> callback, CancellationToken ct)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.Find(new BsonDocument(), options: Batching.Options).ForEachPipedAsync(x => callback(Map(x), x.Version), ct);
            }
        }

        async Task ISnapshotStore<AssetFolderDomainObject.State, DomainId>.RemoveAsync(DomainId key)
        {
            using (Profiler.TraceMethod<MongoAssetFolderRepository>())
            {
                await Collection.DeleteOneAsync(x => x.DocumentId == key);
            }
        }

        private static AssetFolderDomainObject.State Map(MongoAssetFolderEntity existing)
        {
            return SimpleMapper.Map(existing, new AssetFolderDomainObject.State());
        }
    }
}
