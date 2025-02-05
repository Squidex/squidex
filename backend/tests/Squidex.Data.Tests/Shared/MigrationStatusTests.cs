// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Migrations;

namespace Squidex.Shared;

public abstract class MigrationStatusTests
{
    protected abstract Task<IMigrationStatus> CreateSutAsync();

    private async Task<IMigrationStatus> CreateAndPrepareSutAsync()
    {
        var sut = await CreateSutAsync();

        await sut.UnlockAsync();
        return sut;
    }

    [Fact]
    public async Task Should_update_version()
    {
        var sut = await CreateAndPrepareSutAsync();

        var version_0 = await sut.GetVersionAsync();
        await sut.CompleteAsync(version_0 + 1);

        var version_1 = await sut.GetVersionAsync();

        Assert.Equal(version_0 + 1, version_1);
    }

    [Fact]
    public async Task Should_acquire_lock()
    {
        var sut = await CreateAndPrepareSutAsync();

        var lockTaken = 0;
        await Parallel.ForEachAsync(Enumerable.Range(0, 50), async (_, ct) =>
        {
            if (await sut.TryLockAsync(ct))
            {
                Interlocked.Increment(ref lockTaken);
            }
        });

        Assert.Equal(1, lockTaken);
    }

    [Fact]
    public async Task Should_unlock_after_lock_taken()
    {
        var sut = await CreateAndPrepareSutAsync();

        var taken_0 = await sut.TryLockAsync();
        var taken_1 = await sut.TryLockAsync();

        await sut.UnlockAsync();

        var taken_2 = await sut.TryLockAsync();

        Assert.True(taken_0);
        Assert.True(taken_2);

        Assert.False(taken_1);
    }
}
