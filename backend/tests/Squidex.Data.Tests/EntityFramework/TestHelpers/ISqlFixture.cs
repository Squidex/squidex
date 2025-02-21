// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Queries;

namespace Squidex.EntityFramework.TestHelpers;

public interface ISqlFixture<TContext> where TContext : DbContext
{
    SqlDialect Dialect { get; }

    IDbContextFactory<TContext> DbContextFactory { get; }
}
