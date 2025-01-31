// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Migrations;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.Migrations;

[Trait("Category", "TestContainer")]
public class EFMigrationStatusTests(PostgresFixture fixture) : MigrationStatusTests, IClassFixture<PostgresFixture>
{
    protected override async Task<IMigrationStatus> CreateSutAsync()
    {
        var sut = new EFMigrationStatus<TestDbContextPostgres>(fixture.DbContextFactory);

        await sut.InitializeAsync(default);

        return sut;
    }
}
