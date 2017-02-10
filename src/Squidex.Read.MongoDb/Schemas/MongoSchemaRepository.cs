// ==========================================================================
//  MongoSchemaRepository.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Core.Schemas;
using Squidex.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;
using Squidex.Read.Schemas;
using Squidex.Read.Schemas.Repositories;
using Squidex.Read.Schemas.Services;

namespace Squidex.Read.MongoDb.Schemas
{
    public partial class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository, IEventConsumer
    {
        private readonly SchemaJsonSerializer serializer;
        private readonly FieldRegistry registry;
        private readonly ISchemaProvider schemaProvider;

        public MongoSchemaRepository(IMongoDatabase database, SchemaJsonSerializer serializer, FieldRegistry registry, ISchemaProvider schemaProvider)
            : base(database)
        {
            Guard.NotNull(registry, nameof(registry));
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(schemaProvider, nameof(schemaProvider));

            this.registry = registry;
            this.serializer = serializer;
            this.schemaProvider = schemaProvider;
        }

        protected override string CollectionName()
        {
            return "Projections_Schemas";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection)
        {
            return collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.Name));
        }

        public Task ClearAsync()
        {
            return TryDropCollectionAsync();
        }

        public async Task<IReadOnlyList<ISchemaEntity>> QueryAllAsync(Guid appId)
        {
            var entities = await Collection.Find(s => s.AppId == appId && !s.IsDeleted).ToListAsync();

            return entities.OfType<ISchemaEntity>().ToList();
        }

        public async Task<IReadOnlyList<ISchemaEntityWithSchema>> QueryAllWithSchemaAsync(Guid appId)
        {
            var entities = await Collection.Find(s => s.AppId == appId && !s.IsDeleted).ToListAsync();

            entities.ForEach(x => x.DeserializeSchema(serializer));

            return entities.OfType<ISchemaEntityWithSchema>().ToList();
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaAsync(Guid appId, string name)
        {
            var entity = 
                await Collection.Find(s => s.Name == name && s.AppId == appId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

            entity?.DeserializeSchema(serializer);

            return entity;
        }

        public async Task<ISchemaEntityWithSchema> FindSchemaAsync(Guid schemaId)
        {
            var entity = 
                await Collection.Find(s => s.Id == schemaId && !s.IsDeleted)
                    .FirstOrDefaultAsync();

            entity?.DeserializeSchema(serializer);

            return entity;
        }

        public async Task<Guid?> FindSchemaIdAsync(Guid appId, string name)
        {
            var entity = 
                await Collection.Find(s => s.Name == name & s.AppId == appId && !s.IsDeleted)
                    .Project<MongoSchemaEntity>(Projection.Include(x => x.Id)).FirstOrDefaultAsync();

            return entity?.Id;
        }
    }
}
