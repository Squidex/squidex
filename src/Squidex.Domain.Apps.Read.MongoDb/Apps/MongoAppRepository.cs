// ==========================================================================
//  MongoAppRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Apps
{
    public partial class MongoAppRepository : MongoRepositoryBase<MongoAppEntity>, IAppRepository, IEventConsumer
    {
        public MongoAppRepository(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_Apps";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoAppEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Name));
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
        }

        public async Task<IReadOnlyList<IAppEntity>> QueryAllAsync(string subjectId)
        {
            var appEntities =
                await Collection.Find(s => s.Contributors.ContainsKey(subjectId))
                    .ToListAsync();

            return appEntities;
        }

        public async Task<IAppEntity> FindAppAsync(Guid id)
        {
            var appEntity =
                await Collection.Find(s => s.Id == id)
                    .FirstOrDefaultAsync();

            return appEntity;
        }

        public async Task<IAppEntity> FindAppAsync(string name)
        {
            var appEntity =
                await Collection.Find(s => s.Name == name)
                    .FirstOrDefaultAsync();

            return appEntity;
        }
    }
}
