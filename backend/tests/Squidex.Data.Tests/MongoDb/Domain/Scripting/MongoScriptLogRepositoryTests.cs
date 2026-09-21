// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Scripting;
using Squidex.Domain.Apps.Entities.Scripting.Repositories;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.Scripting;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoScriptLogRepositoryTests(MongoFixture fixture) : ScriptLogRepositoryTests
{
    protected override async Task<IScriptLogRepository> CreateSutAsync()
    {
        var sut = new MongoScriptLogRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
