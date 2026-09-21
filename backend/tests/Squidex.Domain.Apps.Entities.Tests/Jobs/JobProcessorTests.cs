// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging.Abstractions;
using Squidex.Caching;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Collaboration;
using Squidex.Domain.Apps.Entities.TestHelpers;
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
}
