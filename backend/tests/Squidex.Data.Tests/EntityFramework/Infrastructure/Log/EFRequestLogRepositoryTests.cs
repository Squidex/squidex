// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Infrastructure.Log;
using Squidex.Shared;

namespace Squidex.EntityFramework.Infrastructure.Log;

public abstract class EFRequestLogRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : RequestLogRepositoryTests where TContext : DbContext
{
    protected override Task<IRequestLogRepository> CreateSutAsync()
    {
        var sut =
            new EFRequestLogRepository<TContext>(
                fixture.DbContextFactory, Options.Create(new RequestLogStoreOptions()));

        return Task.FromResult<IRequestLogRepository>(sut);
    }
}
