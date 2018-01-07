// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Rules
{
    public sealed partial class MongoRuleRepository : MongoRepositoryBase<MongoRuleEntity>, IRuleRepository
    {
        public MongoRuleRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "States_Rules";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoRuleEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId));
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.IsDeleted));
        }

        public async Task<IReadOnlyList<Guid>> QueryRuleIdsAsync(Guid appId)
        {
            var ruleEntities =
                await Collection.Find(x => x.AppId == appId && !x.IsDeleted).Only(x => x.Id)
                    .ToListAsync();

            return ruleEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }
    }
}
