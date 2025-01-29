// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Migrations;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Infrastructure.Migrations;

public class MongoMigrationStatusTests(MongoFixture fixture) : MigrationStatusTests, IClassFixture<MongoFixture>
{
    protected override async Task<IMigrationStatus> CreateSutAsync()
    {
        var sut = new MongoMigrationStatus(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
