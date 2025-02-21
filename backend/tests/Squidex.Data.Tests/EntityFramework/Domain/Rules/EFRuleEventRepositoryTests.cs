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

public abstract class EFRuleEventRepositoryTests<TContext>(ISqlFixture<TContext> fixture) : RuleEventRepositoryTests where TContext : DbContext
{
    protected override Task<IRuleEventRepository> CreateSutAsync()
    {
        var sut = new EFRuleEventRepository<TContext>(fixture.DbContextFactory);

        return Task.FromResult<IRuleEventRepository>(sut);
    }
}
