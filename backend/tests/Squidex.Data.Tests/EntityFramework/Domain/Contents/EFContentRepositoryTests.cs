// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

[Trait("Category", "TestContainer")]
[Collection("Postgres")]
public class EFContentRepositoryTests(PostgresFixture fixture) : ContentRepositoryTests
{
    protected override Task<IContentRepository> CreateSutAsync()
    {
        var sut = new EFContentRepository<TestDbContextPostgres>(fixture.DbContextFactory, AppProvider, fixture.Dialect);

        return Task.FromResult<IContentRepository>(sut);
    }
}
