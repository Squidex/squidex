﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.Migrations
{
    public sealed class MongoMigrationStatus : MongoRepositoryBase<MongoMigrationEntity>, IMigrationStatus
    {
        private const string DefaultId = "Default";
        private static readonly FindOneAndUpdateOptions<MongoMigrationEntity> UpsertFind = new FindOneAndUpdateOptions<MongoMigrationEntity> { IsUpsert = true };

        public MongoMigrationStatus(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Migration";
        }

        public async Task<int> GetVersionAsync()
        {
            var entity = await Collection.Find(x => x.Id == DefaultId).FirstOrDefaultAsync();

            return entity.Version;
        }

        public async Task<bool> TryLockAsync()
        {
            var entity =
                await Collection.FindOneAndUpdateAsync<MongoMigrationEntity>(x => x.Id == DefaultId,
                    Update
                        .Set(x => x.IsLocked, true)
                        .SetOnInsert(x => x.Version, 0),
                    UpsertFind);

            return entity == null || entity.IsLocked == false;
        }

        public Task UnlockAsync(int newVersion)
        {
            return Collection.UpdateOneAsync(x => x.Id == DefaultId,
                Update
                    .Set(x => x.IsLocked, false)
                    .Set(x => x.Version, newVersion));
        }
    }
}
