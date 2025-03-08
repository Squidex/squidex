// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.EntityFramework.TestHelpers;

public interface ISqlFixture<TContext> where TContext : DbContext
{
    IDbContextFactory<TContext> DbContextFactory { get; }
}

public interface ISqlContentFixture<TContext, TContentContext>
    : ISqlFixture<TContext>
    where TContext : DbContext where TContentContext : ContentDbContext
{
    IDbContextNamedFactory<TContentContext> DbContextNamedFactory { get; }
}
