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

public class MongoSnapshotStoreTests(MongoFixture fixture) : SnapshotStoreTests, IClassFixture<MongoFixture>, IAsyncLifetime
{
    private readonly MongoSnapshotStore<SnapshotValue> sut = new MongoSnapshotStore<SnapshotValue>(fixture.Database);

    public async Task InitializeAsync()
    {
        await sut.InitializeAsync(default);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected override Task<ISnapshotStore<SnapshotValue>> CreateSutAsync()
    {
        return Task.FromResult<ISnapshotStore<SnapshotValue>>(sut);
    }
}
