// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.EntityFramework.TestHelpers;

namespace Squidex.EntityFramework.Domain.Schemas;

public class EFSchemaRepositoryTests(PostgresFixture fixture) : Shared.SchemaRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<ISchemaRepository> CreateSutAsync()
    {
        var sut = new EFSchemaRepository<TestContext>(fixture.DbContextFactory);

        return Task.FromResult<ISchemaRepository>(sut);
    }
}
