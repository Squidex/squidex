// ==========================================================================
//  MongoDbModelSchemaRepository.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PinkParrot.Read.Models;
using PinkParrot.Read.Repositories.MongoDb.Utils;

namespace PinkParrot.Read.Repositories.MongoDb
{
    public sealed class MongoDbModelSchemaRepository : BaseRepository<ModelSchemaRM>, IModelSchemaRepository
    {
        public MongoDbModelSchemaRepository(IMongoDatabase database)
            : base(database, "ModelSchemas")
        {
            CreateIndicesAsync().Wait();
        }

        private async Task CreateIndicesAsync()
        {
            await Collection.Indexes.CreateOneAsync(IndexKeys.Ascending(x => x.SchemaId));
        }

        public IQueryable<ModelSchemaRM> QuerySchemas()
        {
            return Collection.AsQueryable();
        }

        public Task<List<ModelSchemaRM>> QueryAllAsync()
        {
            return Collection.Find(s => true).ToListAsync();
        }
    }
}
