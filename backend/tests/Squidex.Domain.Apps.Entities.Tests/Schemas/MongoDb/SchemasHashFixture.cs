// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.MongoDb.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Entities.Schemas.MongoDb;

public sealed class SchemasHashFixture
{
    public MongoSchemasHash SchemasHash { get; }

    public SchemasHashFixture()
    {
        TestUtils.SetupBson();

        var mongoClient = MongoClientFactory.Create(TestConfig.Configuration["mongoDb:configuration"]);
        var mongoDatabase = mongoClient.GetDatabase(TestConfig.Configuration["mongodb:database"]);

        var schemasHash = new MongoSchemasHash(mongoDatabase);

        Task.Run(async () =>
        {
            await schemasHash.InitializeAsync(default);
        }).Wait();

        SchemasHash = schemasHash;
    }
}
