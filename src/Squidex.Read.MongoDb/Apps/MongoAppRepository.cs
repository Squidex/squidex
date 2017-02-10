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
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Apps;
using Squidex.Read.Apps.Repositories;

namespace Squidex.Read.MongoDb.Apps
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
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Name));
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
        }

        public async Task<IReadOnlyList<IAppEntity>> QueryAllAsync(string subjectId)
        {
            var entities =
                await Collection.Find(s => s.Contributors.ContainsKey(subjectId)).ToListAsync();

            return entities;
        }

        public async Task<IAppEntity> FindAppAsync(Guid id)
        {
            var entity =
                await Collection.Find(s => s.Id == id).FirstOrDefaultAsync();

            return entity;
        }

        public async Task<IAppEntity> FindAppAsync(string name)
        {
            var entity =
                await Collection.Find(s => s.Name == name).FirstOrDefaultAsync();

            return entity;
        }
    }
}
