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

public class MongoMigrationStatusTests(MongoFixture fixture) : MigrationStatusTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoMigrationStatus sut = new MongoMigrationStatus(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IMigrationStatus> CreateSutAsync()
    {
        return Task.FromResult<IMigrationStatus>(sut);
    }
}
