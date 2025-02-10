// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Schemas;

public abstract class EFSchemaRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : SchemaRepositoryTests where TContext : DbContext
{
    protected override Task<ISchemaRepository> CreateSutAsync()
    {
        var sut = new EFSchemaRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<ISchemaRepository>(sut);
    }
}
