// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.States;
using Squidex.MongoDb.TestHelpers;
using Squidex.Shared;

namespace Squidex.MongoDb.Infrastructure.States;

[Trait("Category", "TestContainer")]
[Collection(MongoFixtureCollection.Name)]
public class MongoSnapshotStoreTests(MongoFixture fixture) : SnapshotStoreTests
{
    protected override async Task<ISnapshotStore<SnapshotValue>> CreateSutAsync()
    {
        var sut = new MongoSnapshotStore<SnapshotValue>(fixture.Database);

        await sut.InitializeAsync(default);
        return sut;
    }
}
