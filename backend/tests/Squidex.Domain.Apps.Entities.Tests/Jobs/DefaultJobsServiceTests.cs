// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.TestHelpers;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Jobs;

public class DefaultJobsServiceTests : GivenContext
{
    private readonly TestState<JobsState> state;
    private readonly IJobRunner runner1 = A.Fake<IJobRunner>();
    private readonly IJobRunner runner2 = A.Fake<IJobRunner>();
    private readonly IMessageBus messaging = A.Fake<IMessageBus>();
    private readonly Stream stream = new MemoryStream();
    private readonly DomainId jobId = DomainId.NewGuid();
    private readonly DefaultJobService sut;

    public DefaultJobsServiceTests()
    {
        state = new TestState<JobsState>(AppId.Id);

        A.CallTo(() => runner1.Name)
            .Returns("job1");

        A.CallTo(() => runner1.MaxJobs)
            .Returns(2);

        A.CallTo(() => runner2.Name)
            .Returns("job2");

        sut = new DefaultJobService(messaging, new[] { runner1, runner2 }, state.PersistenceFactory);
    }

    [Fact]
    public async Task Should_send_message_to_start_job()
    {
        var request = JobRequest.Create(User, "job1");

        await sut.StartAsync(AppId.Id, request, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(new JobStart(AppId.Id, request), null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_message_to_delete_backup()
    {
        await sut.DeleteJobAsync(AppId.Id, jobId, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(new JobDelete(AppId.Id, jobId), null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_send_message_to_clear_backups()
    {
        await ((IDeleter)sut).DeleteAppAsync(App, CancellationToken);

        A.CallTo(() => messaging.PublishAsync(new JobClear(AppId.Id), null, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_when_job_is_invalid()
    {
        var request = JobRequest.Create(User, "unknown");

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartAsync(App.Id, request, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_when_job_is_already_running()
    {
        state.Snapshot.Jobs.Add(new Job { Status = JobStatus.Started });

        var request = JobRequest.Create(User, "job1");

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartAsync(App.Id, request, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_when_backup_has_too_many_jobs()
    {
        state.Snapshot.Jobs.Add(new Job { TaskName = "job1", File = new JobFile("file", "type") });
        state.Snapshot.Jobs.Add(new Job { TaskName = "job1", File = new JobFile("file", "type") });

        var request = JobRequest.Create(User, "job1");

        await Assert.ThrowsAnyAsync<DomainException>(() => sut.StartAsync(App.Id, request, CancellationToken));
    }

    [Fact]
    public async Task Should_not_throw_exception_when_backup_has_too_many_jobs_without_files()
    {
        state.Snapshot.Jobs.Add(new Job { TaskName = "job1" });
        state.Snapshot.Jobs.Add(new Job { TaskName = "job1" });

        var request = JobRequest.Create(User, "job1");

        await sut.StartAsync(App.Id, request, CancellationToken);
    }

    [Fact]
    public async Task Should_get_backups_state_from_store()
    {
        var job = new Job
        {
            Id = jobId,
            Started = SystemClock.Instance.GetCurrentInstant(),
            Stopped = SystemClock.Instance.GetCurrentInstant()
        };

        state.Snapshot.Jobs.Add(job);

        var actual = await sut.GetJobsAsync(AppId.Id, CancellationToken);

        actual.Should().BeEquivalentTo(state.Snapshot.Jobs);
    }

    [Fact]
    public async Task Should_download_file()
    {
        var job = new Job { TaskName = "job2", Status = JobStatus.Completed, File = new JobFile("file", "type") };

        await sut.DownloadAsync(job, stream, CancellationToken);

        A.CallTo(() => runner2.DownloadAsync(job, stream, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_throw_exception_if_job_to_download_has_no_file()
    {
        var job = new Job { TaskName = "job2", Status = JobStatus.Completed, File = null };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DownloadAsync(job, stream, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_job_is_not_completed()
    {
        var job = new Job { TaskName = "job2", Status = JobStatus.Started, File = new JobFile("file", "type") };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DownloadAsync(job, stream, CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_job_has_invalid_task_name()
    {
        var job = new Job { TaskName = "invalid", Status = JobStatus.Completed, File = new JobFile("file", "type") };

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.DownloadAsync(job, stream, CancellationToken));
    }
}
