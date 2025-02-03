// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.States;

[Trait("Category", "TestContainer")]
[Collection("Postgres")]
public class EFSnapshotStoreTests(PostgresFixture fixture) : SnapshotStoreTests
{
    protected override Task<ISnapshotStore<SnapshotValue>> CreateSutAsync()
    {
        var sut = new EFSnapshotStore<TestDbContextPostgres, SnapshotValue, EFState<SnapshotValue>>(fixture.DbContextFactory);

        return Task.FromResult<ISnapshotStore<SnapshotValue>>(sut);
    }
}
