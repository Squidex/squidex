// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets.State;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : ISnapshotStore<AssetState, Guid>
    {
        Task ISnapshotStore<AssetState, Guid>.ReadAllAsync(Func<AssetState, long, Task> callback)
        {
            throw new NotSupportedException();
        }

        public async Task<(AssetState Value, long Version)> ReadAsync(Guid key)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var existing =
                    await Collection.Find(x => x.Id == key)
                        .FirstOrDefaultAsync();

                if (existing != null)
                {
                    return (SimpleMapper.Map(existing, new AssetState()), existing.Version);
                }

                return (null, EtagVersion.NotFound);
            }
        }

        public async Task WriteAsync(Guid key, AssetState value, long oldVersion, long newVersion)
        {
            using (Profiler.TraceMethod<MongoAssetRepository>())
            {
                var entity = SimpleMapper.Map(value, new MongoAssetEntity());

                entity.Version = newVersion;
                entity.IndexedAppId = value.AppId.Id;

                await Collection.ReplaceOneAsync(x => x.Id == key && x.Version == oldVersion, entity, Upsert);
            }
        }
    }
}
