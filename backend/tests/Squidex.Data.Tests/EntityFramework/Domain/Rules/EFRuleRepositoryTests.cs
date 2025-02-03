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
[Collection("Postgres")]
public class EFRuleRepositoryTests(PostgresFixture fixture) : RuleRepositoryTests
{
    protected override Task<IRuleRepository> CreateSutAsync()
    {
        var sut = new EFRuleRepository<TestDbContextPostgres>(fixture.DbContextFactory);

        return Task.FromResult<IRuleRepository>(sut);
    }
}
