// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;

namespace Squidex.Infrastructure;

public interface IDbContextNamedFactory<TContext> where TContext : DbContext
{
    Task<TContext> CreateDbContextAsync(string name,
        CancellationToken ct = default);
}
