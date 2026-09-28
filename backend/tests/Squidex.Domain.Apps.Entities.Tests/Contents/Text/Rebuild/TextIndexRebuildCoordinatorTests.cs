// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public class TextIndexRebuildCoordinatorTests : GivenContext
{
    private readonly IJobService jobService = A.Fake<IJobService>();
    private readonly IClock clock = A.Fake<IClock>();
    private readonly TextIndexRebuildRegistry registry;
    private readonly TextIndexRebuildCoordinator sut;
    private Instant now = SystemClock.Instance.GetCurrentInstant();

    public TextIndexRebuildCoordinatorTests()
    {
        A.CallTo(() => clock.GetCurrentInstant())
            .ReturnsLazily(() => now);

        registry =
            new TextIndexRebuildRegistry(
                new InMemoryPersistenceFactory<TextIndexRebuildRequests>(),
                new InMemoryPersistenceFactory<TextIndexSkipList>())
            {
                Clock = clock,
            };

        sut = new TextIndexRebuildCoordinator(registry, AppProvider, jobService, A.Fake<ILogger<TextIndexRebuildCoordinator>>())
        {
            Clock = clock,
        };
    }

    [Fact]
    public async Task Should_not_skip_without_requests()
    {
        await sut.SynchronizeAsync(CancellationToken);

        Assert.False(await TrySkipAsync(DomainId.NewGuid()));
    }

    [Fact]
    public async Task Should_acknowledge_request_and_skip_app()
    {
        var request = await registry.StartAsync(AppId.Id, CancellationToken);

        Assert.False(await TrySkipAsync(DomainId.NewGuid()));

        await sut.SynchronizeAsync(CancellationToken);

        var skipList = await registry.GetSkipListAsync(AppId.Id, CancellationToken);

        Assert.Equal(request.Id, skipList?.RequestId);
        Assert.True(await TrySkipAsync(DomainId.NewGuid()));
    }

    [Fact]
    public async Task Should_record_skipped_contents_once_per_content()
    {
        var contentId1 = DomainId.NewGuid();
        var contentId2 = DomainId.NewGuid();

        await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        await TrySkipAsync(contentId1);
        await TrySkipAsync(contentId2);
        await TrySkipAsync(contentId1);

        var skipList = await registry.GetSkipListAsync(AppId.Id, CancellationToken);

        Assert.Equal(3, skipList?.Sequence);
        Assert.Equal([(contentId2, 2L), (contentId1, 3L)], skipList!.Contents.Select(x => (x.ContentId, x.Sequence)).OrderBy(x => x.Sequence));
    }

    [Fact]
    public async Task Should_carry_over_skipped_contents_to_new_request()
    {
        var contentId = DomainId.NewGuid();

        await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);
        await TrySkipAsync(contentId);

        // For example when the job has crashed and a recovery job has been started.
        var request = await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        var skipList = await registry.GetSkipListAsync(AppId.Id, CancellationToken);

        Assert.Equal(request.Id, skipList?.RequestId);
        Assert.Equal(contentId, Assert.Single(skipList!.Contents).ContentId);
    }

    [Fact]
    public async Task Should_take_over_if_request_is_completing_and_all_contents_are_processed()
    {
        var request = await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);
        await TrySkipAsync(DomainId.NewGuid());

        await registry.UpdateAsync(AppId.Id, request.Id, 1, TextIndexRebuildStatus.Completing, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        var skipList = await registry.GetSkipListAsync(AppId.Id, CancellationToken);

        Assert.True(skipList?.IsTakenOver);
        Assert.False(await TrySkipAsync(DomainId.NewGuid()));
    }

    [Fact]
    public async Task Should_not_take_over_if_contents_are_not_processed()
    {
        var request = await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);
        await TrySkipAsync(DomainId.NewGuid());

        await registry.UpdateAsync(AppId.Id, request.Id, 0, TextIndexRebuildStatus.Completing, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        Assert.True(await TrySkipAsync(DomainId.NewGuid()));
    }

    [Fact]
    public async Task Should_start_recovery_once_if_heartbeat_has_expired()
    {
        await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        now = now.Plus(Duration.FromMinutes(11));

        await sut.SynchronizeAsync(CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        A.CallTo(() => jobService.StartAsync(AppId.Id, A<JobRequest>.That.Matches(x => x.Arguments.ContainsKey(RebuildTextIndexJob.ArgRecovery)), A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_remove_skip_list_if_request_is_removed()
    {
        var request = await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        await registry.RemoveAsync(AppId.Id, request.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        Assert.Null(await registry.GetSkipListAsync(AppId.Id, CancellationToken));
    }

    [Fact]
    public async Task Should_remove_requests_and_skip_lists_on_reset()
    {
        var request = await registry.StartAsync(AppId.Id, CancellationToken);
        await sut.SynchronizeAsync(CancellationToken);

        await sut.ResetAsync(CancellationToken);

        Assert.False(await registry.UpdateAsync(AppId.Id, request.Id, 0, TextIndexRebuildStatus.Running, CancellationToken));
        Assert.Null(await registry.GetSkipListAsync(AppId.Id, CancellationToken));
        Assert.False(await TrySkipAsync(DomainId.NewGuid()));
    }

    [Fact]
    public async Task Should_wait_for_batch_before_synchronizing()
    {
        var batch = await sut.BeginBatchAsync(CancellationToken);

        var synchronize = sut.SynchronizeAsync(CancellationToken);

        await Task.Delay(50, CancellationToken);
        Assert.False(synchronize.IsCompleted);

        batch.Dispose();

        await synchronize;
    }

    private async Task<bool> TrySkipAsync(DomainId contentId)
    {
        using var batch = await sut.BeginBatchAsync(CancellationToken);

        var result = batch.TrySkip(AppId.Id, contentId);

        await batch.CommitAsync(CancellationToken);
        return result;
    }
}
