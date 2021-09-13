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
using Squidex.Domain.Apps.Entities.Rules.DomainObject;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed class MongoRuleRepository : MongoSnapshotStoreBase<RuleDomainObject.State, MongoRuleEntity>, IRuleRepository, IDeleter
    {
        public MongoRuleRepository(IMongoDatabase database, JsonSerializer jsonSerializer)
            : base(database, jsonSerializer)
        {
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoRuleEntity> collection,
            CancellationToken ct)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoRuleEntity>(
                    Index
                        .Ascending(x => x.IndexedAppId))
            }, ct);
        }

        Task IDeleter.DeleteAppAsync(IAppEntity app,
            CancellationToken ct)
        {
            return Collection.DeleteManyAsync(Filter.Eq(x => x.IndexedAppId, app.Id), ct);
        }

        public async Task<List<DomainId>> QueryIdsAsync(DomainId appId,
            CancellationToken ct = default)
        {
            using (Telemetry.Activities.StartActivity("MongoSchemaRepository/QueryIdsAsync"))
            {
                var entities = await Collection.Find(x => x.IndexedAppId == appId && !x.IndexedDeleted).Only(x => x.IndexedId).ToListAsync(ct);

                return entities.Select(x => DomainId.Create(x["_ri"].AsString)).ToList();
            }
        }
    }
}
