// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.DomainObject;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed class MongoAppRepository : MongoSnapshotStoreBase<AppDomainObject.State, MongoAppEntity>, IAppRepository, IDeleter
    {
        public MongoAppRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection,
            CancellationToken ct)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoAppEntity>(
                    Index
                        .Ascending(x => x.IndexedName)),
                new CreateIndexModel<MongoAppEntity>(
                    Index
                        .Ascending(x => x.IndexedUserIds))
            }, ct);
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return Collection.DeleteManyAsync(Filter.Eq(x => x.DocumentId, app.Id), ct);
        }

        public async Task<Dictionary<string, DomainId>> QueryIdsAsync(string contributorId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryIdsAsync"))
            {
                var find = Collection.Find(x => x.IndexedUserIds.Contains(contributorId) && !x.IndexedDeleted);

                return await QueryAsync(find, ct);
            }
        }

        public async Task<Dictionary<string, DomainId>> QueryIdsAsync(IEnumerable<string> names,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAsync"))
            {
                var find = Collection.Find(x => names.Contains(x.IndexedName) && !x.IndexedDeleted);

                return await QueryAsync(find, ct);
            }
        }

        private static async Task<Dictionary<string, DomainId>> QueryAsync(IFindFluent<MongoAppEntity, MongoAppEntity> find,
            CancellationToken ct)
        {
            var entities = await find.SortBy(x => x.IndexedCreated).Only(x => x.DocumentId, x => x.IndexedName).ToListAsync(ct);

            var result = new Dictionary<string, DomainId>();

            foreach (var entity in entities)
            {
                var indexedId = DomainId.Create(entity["_id"].AsString);
                var indexedName = entity["_an"].AsString;

                result[indexedName] = indexedId;
            }

            return result;
        }

        public async Task<List<IAppEntity>> QueryAllAsync(string contributorId, IEnumerable<string> names,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAllAsync"))
            {
                var entities =
                    await Collection.Find(x => (x.IndexedUserIds.Contains(contributorId) || names.Contains(x.IndexedName)) && !x.IndexedDeleted)
                        .ToListAsync(ct);

                return entities.Select(x => (IAppEntity)x.Document).ToList();
            }
        }

        public async Task<IAppEntity?> FindAsync(DomainId id,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoAppRepository/FindAsync"))
            {
                var entity =
                    await Collection.Find(x => x.DocumentId == id && !x.IndexedDeleted)
                        .FirstOrDefaultAsync(ct);

                return entity?.Document;
            }
        }

        public async Task<IAppEntity?> FindAsync(string name,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoAppRepository/FindAsyncByName"))
            {
                var entity =
                    await Collection.Find(x => x.IndexedName == name && !x.IndexedDeleted)
                        .FirstOrDefaultAsync(ct);

                return entity?.Document;
            }
        }
    }
}
