// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.States;

public abstract class EFSnapshotStoreTests<TContext>(ISqlFixture<TContext> fixture) : SnapshotStoreTests where TContext : DbContext
{
    protected override Task<ISnapshotStore<SnapshotValue>> CreateSutAsync()
    {
        var sut = new EFSnapshotStore<TContext, SnapshotValue, EFState<SnapshotValue>>(fixture.DbContextFactory);

        return Task.FromResult<ISnapshotStore<SnapshotValue>>(sut);
    }
}
