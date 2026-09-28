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

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public class RebuildTextIndexJobTests : GivenContext
{
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly ITextIndexRebuilder textIndexRebuilder = A.Fake<ITextIndexRebuilder>();
    private readonly List<List<DomainId>> batches = [];
    private readonly RebuildTextIndexJob sut;

    public RebuildTextIndexJobTests()
    {
        A.CallTo(() => textIndexRebuilder.RebuildAsync(AppId.Id, A<IReadOnlyCollection<DomainId>>._, A<CancellationToken>._))
            .Invokes(x => batches.Add(x.GetArgument<IReadOnlyCollection<DomainId>>(1)!.ToList()));

        sut = new RebuildTextIndexJob(AppProvider, contentRepository, textIndexRebuilder);
    }

    [Fact]
    public void Should_create_request_for_app()
    {
        var job = RebuildTextIndexJob.BuildRequest(User, App);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
            });
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
    public async Task Should_throw_exception_if_app_not_found()
    {
        A.CallTo(() => AppProvider.GetAppAsync(AppId.Id, A<bool>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Core.Apps.App?>(null));

        var context = CreateRunContext(CreateJob());

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RunAsync(context, CancellationToken));
    }

    [Fact]
    public async Task Should_rebuild_all_contents_of_app_in_batches()
    {
        var ids = Enumerable.Range(0, 250).Select(_ => DomainId.NewGuid()).ToList();

        A.CallTo(() => contentRepository.StreamIds(AppId.Id, null, SearchScope.All, CancellationToken))
            .Returns(ids.ToAsyncEnumerable());

        var job = CreateJob();

        await sut.RunAsync(CreateRunContext(job), CancellationToken);

        Assert.Equal([100, 100, 50], batches.Select(x => x.Count));
        Assert.Equal(ids, batches.SelectMany(x => x));
        Assert.Equal("Rebuild full text index", job.Description);
    }

    [Fact]
    public async Task Should_only_rebuild_contents_of_schema()
    {
        var ids = new List<DomainId> { DomainId.NewGuid() };

        A.CallTo(() => contentRepository.StreamIds(AppId.Id, A<HashSet<DomainId>>.That.IsSameSequenceAs(new[] { SchemaId.Id }), SearchScope.All, CancellationToken))
            .Returns(ids.ToAsyncEnumerable());

        var job = CreateJob(Schema);

        await sut.RunAsync(CreateRunContext(job), CancellationToken);

        Assert.Equal(ids, batches.SelectMany(x => x));
        Assert.Equal($"Schema {Schema.Name}: Rebuild full text index", job.Description);
    }

    [Fact]
    public async Task Should_not_rebuild_if_app_has_no_contents()
    {
        A.CallTo(() => contentRepository.StreamIds(AppId.Id, null, SearchScope.All, CancellationToken))
            .Returns(AsyncEnumerable.Empty<DomainId>());

        await sut.RunAsync(CreateRunContext(CreateJob()), CancellationToken);

        A.CallTo(() => textIndexRebuilder.RebuildAsync(A<DomainId>._, A<IReadOnlyCollection<DomainId>>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    private Job CreateJob(Core.Schemas.Schema? schema = null)
    {
        return new Job
        {
            Id = DomainId.NewGuid(),
            Arguments = RebuildTextIndexJob.BuildRequest(User, App, schema).Arguments,
        };
    }

    private JobRunContext CreateRunContext(Job job)
    {
        var state = new SimpleState<JobsState>(A.Fake<IPersistenceFactory<JobsState>>(), GetType(), App.Id);

        return new JobRunContext(state, A.Fake<IClock>(), default) { Actor = User, Job = job, OwnerId = App.Id };
    }
}
