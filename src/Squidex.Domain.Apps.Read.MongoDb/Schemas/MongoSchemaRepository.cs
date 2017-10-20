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
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Schemas
{
    public partial class MongoSchemaRepository : MongoRepositoryBase<MongoSchemaEntity>, ISchemaRepository, IEventConsumer
    {
        private readonly JsonSerializer serializer;
        private readonly FieldRegistry registry;

        public MongoSchemaRepository(IMongoDatabase database, JsonSerializer serializer, FieldRegistry registry)
            : base(database)
        {
            Guard.NotNull(registry, nameof(registry));
            Guard.NotNull(serializer, nameof(serializer));

            this.registry = registry;
            this.serializer = serializer;
        }

        protected override string CollectionName()
        {
            return "Projections_Schemas";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoSchemaEntity> collection)
        {
            return Task.WhenAll(
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.Name)),
                collection.Indexes.CreateOneAsync(Index.Ascending(x => x.AppId).Ascending(x => x.IsDeleted).Ascending(x => x.Name)));
        }

        public async Task<IReadOnlyList<ISchemaEntity>> QueryAllAsync(Guid appId)
        {
            var schemaEntities =
                await Collection.Find(s => s.AppId == appId && !s.IsDeleted)
                    .ToListAsync();

            schemaEntities.ForEach(x => x.DeserializeSchema(serializer));

            return schemaEntities.OfType<ISchemaEntity>().ToList();
        }

        public async Task<ISchemaEntity> FindSchemaAsync(Guid appId, string name)
        {
            var schemaEntity =
                await Collection.Find(s => s.AppId == appId && !s.IsDeleted && s.Name == name)
                    .FirstOrDefaultAsync();

            schemaEntity?.DeserializeSchema(serializer);

            return schemaEntity;
        }

        public async Task<ISchemaEntity> FindSchemaAsync(Guid schemaId)
        {
            var schemaEntity =
                await Collection.Find(s => s.Id == schemaId)
                    .FirstOrDefaultAsync();

            schemaEntity?.DeserializeSchema(serializer);

            return schemaEntity;
        }
    }
}
