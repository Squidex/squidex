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

public class EFSnapshotStoreTests(PostgresFixture fixture) : SnapshotStoreTests, IClassFixture<PostgresFixture>
{
    protected override Task<ISnapshotStore<SnapshotValue>> CreateSutAsync()
    {
        var sut = new EFSnapshotStore<TestDbContext, SnapshotValue, EFState<SnapshotValue>>(fixture.DbContextFactory);

        return Task.FromResult<ISnapshotStore<SnapshotValue>>(sut);
    }
}
