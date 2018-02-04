﻿// ==========================================================================
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
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Assets
{
    public sealed partial class MongoAssetRepository : ISnapshotStore<AssetState, Guid>
    {
        public async Task<(AssetState Value, long Version)> ReadAsync(Guid key)
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

        public async Task WriteAsync(Guid key, AssetState value, long oldVersion, long newVersion)
        {
            var entity = SimpleMapper.Map(value, new MongoAssetEntity());

            entity.Version = newVersion;

            await Collection.ReplaceOneAsync(x => x.Id == key && x.Version == oldVersion, entity, Upsert);
        }
    }
}
