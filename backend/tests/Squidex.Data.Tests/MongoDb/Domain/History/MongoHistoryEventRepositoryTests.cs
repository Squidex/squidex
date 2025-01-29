// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Entities.History.Repositories;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Domain.History;

[Trait("Category", "TestContainer")]
public class MongoHistoryEventRepositoryTests(MongoFixture fixture) : HistoryEventRepositoryTests, IClassFixture<MongoFixture>
{
    protected override async Task<IHistoryEventRepository> CreateSutAsync()
    {
        var sut = new MongoHistoryEventRepository(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
