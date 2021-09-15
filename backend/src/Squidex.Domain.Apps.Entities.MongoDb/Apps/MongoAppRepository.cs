// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
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
        public MongoAppRepository(IMongoDatabase database, JsonSerializer jsonSerializer)
            : base(database, jsonSerializer)
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
            var entities = await find.Only(x => x.DocumentId, x => x.IndexedName).ToListAsync(ct);

            return entities.Select(x =>
            {
                var indexedId = DomainId.Create(x["_id"].AsString);
                var indexedName = x["_an"].AsString;

                return new { indexedName, indexedId };
            }).ToDictionary(x => x.indexedName, x => x.indexedId);
        }
    }
}
