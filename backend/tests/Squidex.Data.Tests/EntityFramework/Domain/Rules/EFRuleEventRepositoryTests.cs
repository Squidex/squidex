// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.EntityFramework.TestHelpers;
using Squidex.Shared;

namespace Squidex.EntityFramework.Domain.Rules;

[Trait("Category", "TestContainer")]
public class EFRuleEventRepositoryTests(PostgresFixture fixture) : RuleEventRepositoryTests, IClassFixture<PostgresFixture>
{
    protected override Task<IRuleEventRepository> CreateSutAsync()
    {
        var sut = new EFRuleEventRepository<TestDbContext>(fixture.DbContextFactory);

        return Task.FromResult<IRuleEventRepository>(sut);
    }
}
