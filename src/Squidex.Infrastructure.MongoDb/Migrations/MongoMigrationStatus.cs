// ==========================================================================
//  MongoMigrationStatus.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.Migrations
{
    public sealed class MongoMigrationStatus : MongoRepositoryBase<MongoMigrationEntity>, IMigrationStatus
    {
        private const string DefaultId = "Default";

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

            if (entity == null)
            {
                try
                {
                    await Collection.InsertOneAsync(new MongoMigrationEntity { Id = DefaultId });
                }
                catch (MongoWriteException ex)
                {
                    if (ex.WriteError.Category != ServerErrorCategory.DuplicateKey)
                    {
                        throw;
                    }
                }
            }

            return entity?.Version ?? 0;
        }

        public async Task<bool> TryLockAsync()
        {
            var entity = await Collection.FindOneAndUpdateAsync(x => x.Id == DefaultId, Update.Set(x => x.IsLocked, true));

            return entity?.IsLocked == false;
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
