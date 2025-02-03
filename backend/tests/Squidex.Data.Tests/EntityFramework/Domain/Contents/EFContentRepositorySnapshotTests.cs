// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

[Collection("Postgres")]
public class EFContentRepositorySnapshotTests(PostgresFixture fixture) : ContentSnapshotStoreTests
{
    protected override Task<ISnapshotStore<WriteContent>> CreateSutAsync()
    {
        var sut = new EFContentRepository<TestDbContextPostgres>(fixture.DbContextFactory, Context.AppProvider, fixture.Dialect);

        return Task.FromResult<ISnapshotStore<WriteContent>>(sut);
    }
}
