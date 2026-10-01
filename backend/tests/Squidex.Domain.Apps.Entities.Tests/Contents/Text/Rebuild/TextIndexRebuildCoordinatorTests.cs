// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public class TextIndexRebuildCoordinatorTests
{
    private readonly DomainId appId = DomainId.NewGuid();
    private readonly TextIndexRebuildCoordinator sut = new TextIndexRebuildCoordinator();

    [Fact]
    public async Task Should_throw_exception_if_app_is_already_taken_over()
    {
        await sut.TakeOverAsync(appId);

        await Assert.ThrowsAsync<DomainException>(() => sut.TakeOverAsync(appId));
    }

    [Fact]
    public async Task Should_not_skip_events_if_app_is_not_taken_over()
    {
        using var batch = await sut.BeginBatchAsync();

        Assert.False(batch.TrySkip(appId));
    }

    [Fact]
    public async Task Should_skip_and_count_events_if_app_is_taken_over()
    {
        await sut.TakeOverAsync(appId);

        using (var batch = await sut.BeginBatchAsync())
        {
            Assert.True(batch.TrySkip(appId));
            Assert.True(batch.TrySkip(appId));
            Assert.False(batch.TrySkip(DomainId.NewGuid()));
        }

        var skipped = await sut.GetSkippedEventsAsync(appId);

        Assert.Equal(2, skipped);
    }

    [Fact]
    public async Task Should_not_hand_back_if_events_have_been_skipped_in_the_meantime()
    {
        await sut.TakeOverAsync(appId);

        using (var batch = await sut.BeginBatchAsync())
        {
            batch.TrySkip(appId);
        }

        var handedBack = await sut.TryHandBackAsync(appId, 0);

        Assert.False(handedBack);
    }

    [Fact]
    public async Task Should_hand_back_if_no_events_have_been_skipped_in_the_meantime()
    {
        await sut.TakeOverAsync(appId);

        using (var batch = await sut.BeginBatchAsync())
        {
            batch.TrySkip(appId);
        }

        var handedBack = await sut.TryHandBackAsync(appId, 1);

        using (var batch = await sut.BeginBatchAsync())
        {
            Assert.False(batch.TrySkip(appId));
        }

        Assert.True(handedBack);
    }

    [Fact]
    public async Task Should_not_skip_events_after_release()
    {
        await sut.TakeOverAsync(appId);
        await sut.ReleaseAsync(appId);

        using (var batch = await sut.BeginBatchAsync())
        {
            Assert.False(batch.TrySkip(appId));
        }

        await sut.TakeOverAsync(appId);
    }

    [Fact]
    public async Task Should_not_hand_back_while_batch_is_processed()
    {
        await sut.TakeOverAsync(appId);

        var batch = await sut.BeginBatchAsync();

        var handBack = sut.TryHandBackAsync(appId, 0);

        Assert.False(handBack.IsCompleted);

        batch.Dispose();

        Assert.True(await handBack);
    }
}
