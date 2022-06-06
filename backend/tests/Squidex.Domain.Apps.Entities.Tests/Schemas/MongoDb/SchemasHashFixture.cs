// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.MongoDb.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Schemas.MongoDb
{
    public sealed class SchemasHashFixture
    {
        public MongoSchemasHash SchemasHash { get; }

        public SchemasHashFixture()
        {
            InstantSerializer.Register();

            var mongoClient = new MongoClient(TestConfig.Configuration["mongodb:configuration"]);
            var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

            var schemasHash = new MongoSchemasHash(mongoDatabase);

            Task.Run(async () =>
            {
                await schemasHash.InitializeAsync(default);
            }).Wait();

            SchemasHash = schemasHash;
        }
    }
}
