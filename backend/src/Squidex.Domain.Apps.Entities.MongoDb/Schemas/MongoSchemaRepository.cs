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
using Squidex.Domain.Apps.Entities.Schemas.DomainObject;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Schemas
{
    public sealed class MongoSchemaRepository : MongoSnapshotStoreBase<SchemaDomainObject.State, MongoSchemaEntity>, ISchemaRepository, IDeleter
    {
        public MongoSchemaRepository(IMongoDatabase database, JsonSerializer jsonSerializer)
            : base(database, jsonSerializer)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection,
            CancellationToken ct)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoSchemaEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId)
                        .Ascending(x => x.IndexedName))
            }, ct);
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
        }

        public async Task<Dictionary<string, DomainId>> QueryIdsAsync(DomainId appId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoSchemaRepository/QueryAsync"))
            {
                var entities = await Collection.Find(x => x.IndexedAppId == appId && !x.Document.IsDeleted).Only(x => x.IndexedAppId, x => x.IndexedId).ToListAsync(ct);

                return entities.Select(x =>
                {
                    var indexedId = DomainId.Create(x["_id"].AsString);
                    var indexedName = x["_n"].AsString;

                    return new { indexedName, indexedId };
                }).ToDictionary(x => x.indexedName, x => x.indexedId);
            }
        }
    }
}
