// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Events;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public class RebuildTextIndexJobTests : TextIndexTestBase
{
    private readonly TextIndexingProcess process;
    private readonly RebuildTextIndexJob sut;

    public RebuildTextIndexJobTests()
    {
        process = CreateProcess();

        sut = CreateJob(process);
    }

    [Fact]
    public void Should_create_request()
    {
        var job = RebuildTextIndexJob.BuildRequest(User, App);

        job.Arguments.Should().BeEquivalentTo(
            new Dictionary<string, string>
            {
                ["appId"] = App.Id.ToString(),
                ["appName"] = App.Name,
            });

        Assert.Equal(App.NamedId(), job.AppId);
        Assert.Equal(RebuildTextIndexJob.TaskName, job.TaskName);
    }

    [Fact]
    public async Task Should_throw_exception_if_app_not_found()
    {
        A.CallTo(() => AppProvider.GetAppAsync(AppId.Id, true, A<CancellationToken>._))
            .Returns(Task.FromResult<App?>(null));

        await Assert.ThrowsAsync<DomainObjectNotFoundException>(() => sut.RunAsync(CreateRunContext(), CancellationToken));
    }

    [Fact]
    public async Task Should_throw_exception_if_app_is_already_rebuilt()
    {
        await Coordinator.TakeOverAsync(AppId.Id, CancellationToken);

        await Assert.ThrowsAsync<DomainException>(() => sut.RunAsync(CreateRunContext(), CancellationToken));
    }

    [Fact]
    public async Task Should_replay_events_of_app()
    {
        StoreEvents(Created(TextData("field", "Version1"), 0), Updated(TextData("field", "Version2"), 1));

        await sut.RunAsync(CreateRunContext(), CancellationToken);

        Assert.Equal("Version2", GetText());

        A.CallTo(() => EventStore.QueryAllAsync(StreamFilter.Prefix($"content-{AppId.Id}{DomainId.IdSeparator}"), A<StreamPosition>.That.Matches(x => x.Token == null), A<int>._, A<CancellationToken>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_replay_events_that_have_been_skipped_during_rebuild()
    {
        var updated = Updated(TextData("field", "Version2"), 1);

        StoreEvents(Created(TextData("field", "Version1"), 0));

        var isFirst = true;

        A.CallTo(() => TextIndex.ExecuteAsync(A<IndexCommand[]>._, A<CancellationToken>._))
            .ReturnsLazily(async x =>
            {
                Commands.AddRange(x.GetArgument<IndexCommand[]>(0)!);

                // The content is changed while the app is rebuilt, therefore the indexer skips the event.
                if (isFirst)
                {
                    isFirst = false;

                    StoreEvents(updated);
                    await process.On([updated]);
                }
            });

        await sut.RunAsync(CreateRunContext(), CancellationToken);

        Assert.Equal("Version2", GetText());
    }

    [Fact]
    public async Task Should_index_app_again_after_rebuild()
    {
        StoreEvents(Created(TextData("field", "Version1"), 0));

        await sut.RunAsync(CreateRunContext(), CancellationToken);

        await process.On([Updated(TextData("field", "Version2"), 1)]);

        Assert.Equal("Version2", GetText());
    }

    [Fact]
    public async Task Should_replay_events_and_hand_back_if_rebuild_failed()
    {
        StoreEvents(Created(TextData("field", "Version1"), 0));

        A.CallTo(() => EventStore.QueryAllAsync(A<StreamFilter>._, A<StreamPosition>._, A<int>._, A<CancellationToken>._))
            .Throws(new OperationCanceledException()).Once();

        await Assert.ThrowsAsync<OperationCanceledException>(() => sut.RunAsync(CreateRunContext(), CancellationToken));

        await process.On([Updated(TextData("field", "Version2"), 1)]);

        Assert.Equal("Version2", GetText());
        Assert.Equal(2, Commands.OfType<UpsertIndexEntry>().Count());
    }

    [Fact]
    public async Task Should_release_app_if_hand_back_failed()
    {
        A.CallTo(() => EventStore.QueryAllAsync(A<StreamFilter>._, A<StreamPosition>._, A<int>._, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        await Assert.ThrowsAsync<InvalidOperationException>(() => sut.RunAsync(CreateRunContext(), CancellationToken));

        await process.On([Created(TextData("field", "Hello"), 0)]);

        Assert.Equal("Hello", GetText());
    }
}
