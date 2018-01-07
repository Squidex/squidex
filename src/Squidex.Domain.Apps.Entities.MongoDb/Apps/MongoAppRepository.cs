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
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.MongoDb.Apps
{
    public sealed partial class MongoAppRepository : MongoRepositoryBase<MongoAppEntity>, IAppRepository
    {
        public MongoAppRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "States_Apps";
        }

        protected override async Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection)
        {
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.UserIds));
            await collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Name));
        }

        public async Task<IReadOnlyList<Guid>> QueryAppIdsAsync()
        {
            var appEntities =
                await Collection.Find(new BsonDocument()).Only(x => x.Id)
                    .ToListAsync();

            return appEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }

        public async Task<IReadOnlyList<Guid>> QueryUserAppIdsAsync(string userId)
        {
            var appEntities =
                await Collection.Find(x => x.UserIds.Contains(userId)).Only(x => x.Id)
                    .ToListAsync();

            return appEntities.Select(x => Guid.Parse(x["_id"].AsString)).ToList();
        }

        public async Task<Guid> FindAppIdByNameAsync(string name)
        {
            var appEntity =
                await Collection.Find(x => x.Name == name).Only(x => x.Id)
                    .FirstOrDefaultAsync();

            return appEntity != null ? Guid.Parse(appEntity["_id"].AsString) : Guid.Empty;
        }
    }
}
