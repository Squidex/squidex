// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using IClock = NodaTime.IClock;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public class RebuildTextIndexJobTests : GivenContext
{
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly ITextIndexRebuilder rebuilder = A.Fake<ITextIndexRebuilder>();
    private readonly ITextIndexRebuildRegistry registry = A.Fake<ITextIndexRebuildRegistry>();
    private readonly TextIndexRebuildRequest request = new TextIndexRebuildRequest { Id = DomainId.NewGuid() };
    private readonly List<List<DomainId>> batches = [];
    private readonly RebuildTextIndexJob sut;
    private TextIndexSkipList? skipList;

    public RebuildTextIndexJobTests()
    {
        skipList = new TextIndexSkipList { RequestId = request.Id, IsTakenOver = true };

        A.CallTo(() => registry.StartAsync(AppId.Id, A<CancellationToken>._))
            .Returns(request);

        A.CallTo(() => registry.GetSkipListAsync(AppId.Id, A<CancellationToken>._))
            .ReturnsLazily(() => skipList);

        A.CallTo(() => registry.UpdateAsync(AppId.Id, request.Id, A<long>._, A<TextIndexRebuildStatus>._, A<CancellationToken>._))
            .Returns(true);

        A.CallTo(() => rebuilder.RebuildAsync(AppId.Id, A<IReadOnlyCollection<DomainId>>._, A<CancellationToken>._))
            .Invokes(x => batches.Add(x.GetArgument<IReadOnlyCollection<DomainId>>(1)!.ToList()));

        sut = new RebuildTextIndexJob(AppProvider, contentRepository, rebuilder, registry)
        {
            AcknowledgeTimeout = TimeSpan.FromMilliseconds(200),
            HandBackTimeout = TimeSpan.FromMilliseconds(200),
            PollInterval = TimeSpan.FromMilliseconds(10),
        };
    }

    [Fact]
    public void Should_create_request_for_schema()
    {
        var job = RebuildTextIndexJob.BuildRequest(User, App, Schema);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["schemaId"] = Schema.Id.ToString(),
                ["schemaName"] = Schema.Name,
            });
    }

    [Fact]
    public void Should_create_recovery_request()
    {
        var job = RebuildTextIndexJob.BuildRecoveryRequest(User, App);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
                ["recovery"] = "True",
            });
    }

    [Fact]
    public async Task Should_rebuild_all_contents_in_batches_and_hand_back()
    {
        var ids = SetupContents(250);

        var job = CreateJob();

        await sut.RunAsync(CreateRunContext(job), CancellationToken);

        Assert.Equal([100, 100, 50], batches.Select(x => x.Count));
        Assert.Equal(ids, batches.SelectMany(x => x));
        Assert.Equal("Rebuild full text index", job.Description);

        A.CallTo(() => registry.UpdateAsync(AppId.Id, request.Id, A<long>._, TextIndexRebuildStatus.Completing, A<CancellationToken>._))
            .MustHaveHappened();
        A.CallTo(() => registry.RemoveAsync(AppId.Id, request.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_rebuild_skipped_contents()
    {
        var id1 = DomainId.NewGuid();
        var id2 = DomainId.NewGuid();

        skipList = new TextIndexSkipList { RequestId = request.Id, IsTakenOver = true };
        skipList.Add(id1);
        skipList.Add(id2);

        await sut.RunAsync(CreateRunContext(CreateJob(isRecovery: true)), CancellationToken);

        Assert.Equal([id1, id2], batches.Single());

        A.CallTo(() => registry.UpdateAsync(AppId.Id, request.Id, 2, TextIndexRebuildStatus.Completing, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_rebuild_all_contents_in_recovery_mode()
    {
        SetupContents(10);

        var job = CreateJob(isRecovery: true);

        await sut.RunAsync(CreateRunContext(job), CancellationToken);

        Assert.Empty(batches);
        Assert.Equal("Recover full text index", job.Description);

        A.CallTo(() => registry.RemoveAsync(AppId.Id, request.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_hand_back_if_cancelled()
    {
        A.CallTo(() => contentRepository.StreamIds(AppId.Id, null, SearchScope.All, A<CancellationToken>._))
            .Throws(new OperationCanceledException());

        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken));

        A.CallTo(() => registry.RemoveAsync(AppId.Id, request.Id, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_fail_if_text_indexer_does_not_acknowledge()
    {
        skipList = null;

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken));

        Assert.Empty(batches);

        A.CallTo(() => registry.RemoveAsync(A<DomainId>._, A<DomainId>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_fail_if_request_has_been_removed()
    {
        A.CallTo(() => registry.UpdateAsync(AppId.Id, request.Id, A<long>._, A<TextIndexRebuildStatus>._, A<CancellationToken>._))
            .Returns(false);

        SetupContents(10);

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken));

        A.CallTo(() => registry.RemoveAsync(A<DomainId>._, A<DomainId>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private List<DomainId> SetupContents(int count)
    {
        var ids = Enumerable.Range(0, count).Select(_ => DomainId.NewGuid()).ToList();

        A.CallTo(() => contentRepository.StreamIds(AppId.Id, null, SearchScope.All, A<CancellationToken>._))
            .Returns(ids.ToAsyncEnumerable());

        return ids;
    }

    private Job CreateJob(bool isRecovery = false)
    {
        var jobRequest =
            isRecovery ?
            RebuildTextIndexJob.BuildRecoveryRequest(User, App) :
            RebuildTextIndexJob.BuildRequest(User, App);

        return new Job
        {
            Id = DomainId.NewGuid(),
            Arguments = jobRequest.Arguments,
        };
    }

    private JobRunContext CreateRunContext(Job job)
    {
        var state = new SimpleState<JobsState>(A.Fake<IPersistenceFactory<JobsState>>(), GetType(), App.Id);

        return new JobRunContext(state, A.Fake<IClock>(), default) { Actor = User, Job = job, OwnerId = App.Id };
    }
}
