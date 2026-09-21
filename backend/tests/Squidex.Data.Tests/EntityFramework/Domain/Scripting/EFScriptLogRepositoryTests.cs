// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Scripting;
using Squidex.Domain.Apps.Entities.Scripting.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Scripting;

public abstract class EFScriptLogRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : ScriptLogRepositoryTests where TContext : DbContext
{
    protected override Task<IScriptLogRepository> CreateSutAsync()
    {
        var sut = new EFScriptLogRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IScriptLogRepository>(sut);
    }
}
