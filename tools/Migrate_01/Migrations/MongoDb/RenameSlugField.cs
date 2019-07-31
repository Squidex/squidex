﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure.Migrations;

namespace Migrate_01.Migrations.MongoDb
{
    public sealed class RenameSlugField : IMigration
    {
        private readonly IMongoDatabase database;

        public RenameSlugField(IMongoDatabase database)
        {
            this.database = database;
        }

        public Task UpdateAsync()
        {
            var collection = database.GetCollection<BsonDocument>("States_Assets");

            var update = Builders<BsonDocument>.Update.Rename("FileNameSlug", "Slug");

            return collection.UpdateManyAsync(new BsonDocument(), update);
        }
    }
}
