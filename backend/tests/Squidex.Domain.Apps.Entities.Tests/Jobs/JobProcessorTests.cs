// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging.Abstractions;
using NodaTime;
using Squidex.Caching;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Jobs;

public class JobProcessorTests : GivenContext
{
    private readonly TestState<JobsState> state;
    private readonly IJobRunner runner = A.Fake<IJobRunner>();
    private readonly JobProcessor sut;

    public JobProcessorTests()
    {
        state = new TestState<JobsState>(AppId.Id);

        A.CallTo(() => runner.Name)
            .Returns("job1");

        A.CallTo(() => runner.MaxJobs)
            .Returns(3);

        sut = new JobProcessor(AppId.Id,
            [runner],
            A.Fake<ILocalCache>(),
            A.Fake<ICollaborationService>(),
            state.PersistenceFactory,
            A.Fake<IUrlGenerator>(),
            NullLogger<JobProcessor>.Instance);
    }

    [Fact]
    public async Task Should_store_reference_from_request()
    {
        await sut.LoadAsync(CancellationToken);

        var request = JobRequest.Create(User, "job1") with { Reference = "my-reference" };

        await sut.RunAsync(request, CancellationToken);

        var job = Assert.Single(state.Snapshot.Jobs);

        Assert.Equal("my-reference", job.Reference);
        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public async Task Should_mark_interrupted_jobs_as_failed()
    {
        var job = new Job { Id = DomainId.NewGuid(), TaskName = "job1", Status = JobStatus.Started };

        state.Snapshot = new JobsState { Jobs = [job] };

        await sut.LoadAsync(CancellationToken);

        var actual = Assert.Single(state.Snapshot.Jobs);

        Assert.Equal(JobStatus.Failed, actual.Status);
        Assert.NotNull(actual.Stopped);
        Assert.Single(actual.Log);

        A.CallTo(() => runner.CleanupAsync(job))
            .MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task Should_not_change_completed_jobs()
    {
        var job = new Job { Id = DomainId.NewGuid(), TaskName = "job1", Status = JobStatus.Completed, Stopped = default(Instant) };

        state.Snapshot = new JobsState { Jobs = [job] };

        await sut.LoadAsync(CancellationToken);

        var actual = Assert.Single(state.Snapshot.Jobs);

        Assert.Equal(JobStatus.Completed, actual.Status);
        Assert.Empty(actual.Log);

        A.CallTo(() => runner.CleanupAsync(A<Job>._))
            .MustNotHaveHappened();
    }
}
