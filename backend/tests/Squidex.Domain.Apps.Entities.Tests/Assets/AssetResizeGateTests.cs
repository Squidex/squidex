// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetResizeGateTests
{
    private static AssetResizeGate CreateSut(int maxConcurrency)
    {
        return new AssetResizeGate(Options.Create(new AssetOptions { MaxConcurrentResizes = maxConcurrency }));
    }

    [Fact]
    public async Task Should_serialize_same_key()
    {
        using var sut = CreateSut(10);

        var first = await sut.AcquireAsync("key");
        var second = sut.AcquireAsync("key");

        Assert.False(second.IsCompleted);

        first.Dispose();

        (await second).Dispose();
    }

    [Fact]
    public async Task Should_not_serialize_other_key()
    {
        using var sut = CreateSut(10);

        using var first = await sut.AcquireAsync("key1");
        using var second = await sut.AcquireAsync("key2");
    }

    [Fact]
    public async Task Should_limit_total_concurrency()
    {
        using var sut = CreateSut(1);

        var first = await sut.AcquireAsync("key1");
        var second = sut.AcquireAsync("key2");

        Assert.False(second.IsCompleted);

        first.Dispose();

        (await second).Dispose();
    }

    [Fact]
    public async Task Should_release_key_when_cancelled()
    {
        using var sut = CreateSut(10);
        using var cts = new CancellationTokenSource();

        var first = await sut.AcquireAsync("key");
        var second = sut.AcquireAsync("key", cts.Token);

        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => second);

        first.Dispose();

        // The key must be usable again, even though the waiting request has been cancelled.
        using var third = await sut.AcquireAsync("key");
    }

    [Fact]
    public async Task Should_handle_concurrent_requests()
    {
        using var sut = CreateSut(2);

        await Parallel.ForEachAsync(Enumerable.Range(0, 100), async (i, ct) =>
        {
            using var lease = await sut.AcquireAsync($"key{i % 5}", ct);

            await Task.Yield();
        });
    }
}
