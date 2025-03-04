// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.States;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Contents;

public abstract class EFContentRepositorySnapshotTests<TContext, TContentContext>(ISqlContentFixture<TContext, TContentContext> fixture)
    : ContentSnapshotStoreTests
    where TContext : DbContext where TContentContext : ContentDbContext
{
    protected override Task<ISnapshotStore<WriteContent>> CreateSutAsync()
    {
        var sut =
            new EFContentRepository<TContext, TContentContext>(
                fixture.DbContextFactory,
                fixture.DbContextNamedFactory,
                Context.AppProvider,
                Options.Create(new ContentsOptions()),
                fixture.Dialect);

        return Task.FromResult<ISnapshotStore<WriteContent>>(sut);
    }
}
