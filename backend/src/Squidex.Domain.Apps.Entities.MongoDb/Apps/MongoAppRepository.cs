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
using MongoDB.Bson;
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
                        .Ascending(x => x.IndexedContributorIds))
            }, ct);
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return Collection.DeleteManyAsync(Filter.Eq(x => x.DocumentId, app.Id), ct);
        }

        public async Task<Dictionary<string, DomainId>> QueryIdsAsync(string contributorId, CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryIdsAsync"))
            {
                var filter =
                    Filter.And(
                        Filter.AnyEq(x => x.IndexedContributorIds, contributorId),
                        Filter.Ne(x => x.Document.IsDeleted, true));

                var entities = await Collection.Find(filter).Only(x => x.DocumentId).ToListAsync(ct);

                return CreateResult(entities);
            }
        }

        public async Task<Dictionary<string, DomainId>> QueryIdsAsync(IEnumerable<string> names, CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoAppRepository/QueryAsync"))
            {
                var filter =
                    Filter.And(
                        Filter.In(x => x.IndexedName, names),
                        Filter.Ne(x => x.Document.IsDeleted, true));

                var entities = await Collection.Find(filter).Only(x => x.DocumentId).ToListAsync(ct);

                return CreateResult(entities);
            }
        }

        private static Dictionary<string, DomainId> CreateResult(IEnumerable<BsonDocument> entities)
        {
            return entities.Select(x =>
            {
                var indexedId = DomainId.Create(x["_id"].AsString);
                var indexedName = x["_n"].AsString;

                return new { indexedName, indexedId };
            }).ToDictionary(x => x.indexedName, x => x.indexedId);
        }
    }
}
