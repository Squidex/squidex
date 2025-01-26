// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Apps.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Apps;

[Trait("Category", "TestContainer")]
public class EFAppRepositoryTests(PostgresFixture fixture) : AppRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<IAppRepository> CreateSutAsync()
    {
        var sut = new EFAppRepository<TestDbContext>(fixture.DbContextFactory);

        return Task.FromResult<IAppRepository>(sut);
    }
}
