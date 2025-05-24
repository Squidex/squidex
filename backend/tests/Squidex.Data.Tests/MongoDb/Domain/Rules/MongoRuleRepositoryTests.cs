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

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoRuleRepositoryTests(MongoFixture fixture) : RuleRepositoryTests
{
    protected override async Task<IRuleRepository> CreateSutAsync()
    {
        var sut = new MongoRuleRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
