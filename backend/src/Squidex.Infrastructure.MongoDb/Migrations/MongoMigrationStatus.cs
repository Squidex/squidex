// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.Migrations;

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

    public async Task<int> GetVersionAsync(
        CancellationToken ct = default)
    {
        var entity = await Collection.Find(x => x.Id == DefaultId).FirstOrDefaultAsync(ct);

        return entity.Version;
    }

    public async Task<bool> TryLockAsync(
        CancellationToken ct = default)
    {
        var entity =
            await Collection.FindOneAndUpdateAsync<MongoMigrationEntity>(x => x.Id == DefaultId,
                Update
                    .Set(x => x.IsLocked, true)
                    .SetOnInsert(x => x.Version, 0),
                UpsertFind,
                ct);

        return entity is not { IsLocked: true };
    }

    public Task CompleteAsync(int newVersion,
        CancellationToken ct = default)
    {
        return Collection.UpdateOneAsync(x => x.Id == DefaultId,
            Update
                .Set(x => x.Version, newVersion),
            cancellationToken: ct);
    }

    public Task UnlockAsync(
        CancellationToken ct = default)
    {
        return Collection.UpdateOneAsync(x => x.Id == DefaultId,
            Update
                .Set(x => x.IsLocked, false),
            cancellationToken: ct);
    }
}
