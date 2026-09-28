// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Tasks;

public class AsyncKeyedLockTests
{
    private readonly AsyncKeyedLock<string> sut = new AsyncKeyedLock<string>();

    [Fact]
    public async Task Should_block_same_key()
    {
        var handle = await sut.EnterAsync("key");

        var second = sut.EnterAsync("key");

        await Task.Delay(50);
        Assert.False(second.IsCompleted);

        handle.Dispose();

        (await second).Dispose();
    }

    [Fact]
    public async Task Should_not_block_other_key()
    {
        using var handle = await sut.EnterAsync("key1");

        var second = sut.EnterAsync("key2");

        await Task.Delay(50);
        Assert.True(second.IsCompleted);

        (await second).Dispose();
    }

    [Fact]
    public async Task Should_remove_entry_if_released()
    {
        var handle1 = await sut.EnterAsync("key");
        var handle2 = sut.EnterAsync("key");

        handle1.Dispose();
        (await handle2).Dispose();

        Assert.Equal(0, sut.Count);
    }

    [Fact]
    public async Task Should_ignore_duplicate_dispose()
    {
        var handle = await sut.EnterAsync("key");

        handle.Dispose();
        handle.Dispose();

        using var next = await sut.EnterAsync("key");

        Assert.Equal(1, sut.Count);
    }

    [Fact]
    public async Task Should_remove_entry_if_waiting_is_cancelled()
    {
        using var cts = new CancellationTokenSource();

        var handle = await sut.EnterAsync("key");

        var waiting = sut.EnterAsync("key", cts.Token);

        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);

        handle.Dispose();

        Assert.Equal(0, sut.Count);
    }
}
