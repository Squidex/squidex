// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Rules;

public class MongoRuleRepositoryTests(MongoFixture fixture) : RuleRepositoryTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoRuleRepository sut = new MongoRuleRepository(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<IRuleRepository> CreateSutAsync()
    {
        return Task.FromResult<IRuleRepository>(sut);
    }
}
