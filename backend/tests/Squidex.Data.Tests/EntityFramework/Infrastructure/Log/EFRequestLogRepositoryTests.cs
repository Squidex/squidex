// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Log;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.Log;

[Trait("Category", "TestContainer")]
public class EFRequestLogRepositoryTests(PostgresFixture fixture) : RequestLogRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<IRequestLogRepository> CreateSutAsync()
    {
        var sut =
            new EFRequestLogRepository<TestDbContextPostgres>(
                fixture.DbContextFactory, Options.Create(new RequestLogStoreOptions()));

        return Task.FromResult<IRequestLogRepository>(sut);
    }
}
