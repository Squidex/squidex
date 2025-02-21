// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

public abstract class EFContentRepositorySnapshotTests<TContext>(ISqlFixture<TContext> fixture) : ContentSnapshotStoreTests where TContext : DbContext
{
    protected override Task<ISnapshotStore<WriteContent>> CreateSutAsync()
    {
        var sut = new EFContentRepository<TContext>(fixture.DbContextFactory, Context.AppProvider, fixture.Dialect);

        return Task.FromResult<ISnapshotStore<WriteContent>>(sut);
    }
}
