// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Migrations;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.Migrations;

public abstract class EFMigrationStatusTests<TContext>(ISqlFixture<TContext> fixture) : MigrationStatusTests where TContext : DbContext
{
    protected override async Task<IMigrationStatus> CreateSutAsync()
    {
        var sut = new EFMigrationStatus<TContext>(fixture.DbContextFactory);

        await sut.InitializeAsync(default);

        return sut;
    }
}
