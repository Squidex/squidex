// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Infrastructure.Json;

namespace Squidex.Infrastructure;

public sealed class DelegatingDbNamedContextFactory<TContext>(
    IJsonSerializer jsonSerializer,
    Func<IJsonSerializer, string, TContext> factory)
    : IDbContextNamedFactory<TContext> where TContext : DbContext
{
    public Task<TContext> CreateDbContextAsync(string name,
        CancellationToken ct = default)
    {
        return Task.FromResult(factory(jsonSerializer, name));
    }
}
