// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.MongoDb.Schemas;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Schemas.MongoDb
{
    public sealed class SchemasHashFixture
    {
        private readonly IMongoClient mongoClient = new MongoClient("mongodb://localhost");
        private readonly IMongoDatabase mongoDatabase;

        public MongoSchemasHash SchemasHash { get; }

        public SchemasHashFixture()
        {
            InstantSerializer.Register();

            mongoDatabase = mongoClient.GetDatabase("Squidex_Testing");

            var schemasHash = new MongoSchemasHash(mongoDatabase);

            Task.Run(async () =>
            {
                await schemasHash.InitializeAsync(default);
            }).Wait();

            SchemasHash = schemasHash;
        }
    }
}
