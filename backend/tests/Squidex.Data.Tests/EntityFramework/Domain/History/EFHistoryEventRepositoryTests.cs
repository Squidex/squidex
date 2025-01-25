// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.History;

public class EFHistoryEventRepositoryTests(PostgresFixture fixture) : HistoryEventRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<IHistoryEventRepository> CreateSutAsync()
    {
        var sut = new EFHistoryEventRepository<TestDbContext>(fixture.DbContextFactory);

        return Task.FromResult<IHistoryEventRepository>(sut);
    }
}
