// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.EntityFrameworkCore;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Rules;

public abstract class EFRuleRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : RuleRepositoryTests where TContext : DbContext
{
    protected override Task<IRuleRepository> CreateSutAsync()
    {
        var sut = new EFRuleRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IRuleRepository>(sut);
    }
}
