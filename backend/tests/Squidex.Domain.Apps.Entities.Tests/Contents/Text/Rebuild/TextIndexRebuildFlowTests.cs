// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;
using IClock = NodaTime.IClock;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

public class TextIndexRebuildFlowTests : TextIndexTestBase
{
    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
    private readonly TextIndexingProcess process;
    private readonly RebuildTextIndexJob job;

    public TextIndexRebuildFlowTests()
    {
        A.CallTo(() => contentRepository.StreamIds(AppId.Id, null, SearchScope.All, A<CancellationToken>._))
            .ReturnsLazily(() => new[] { ContentId }.ToAsyncEnumerable());

        process = CreateProcess();

        job = new RebuildTextIndexJob(AppProvider, contentRepository, CreateRebuilder(), Registry)
        {
            PollInterval = TimeSpan.FromMilliseconds(10),
        };
    }

    [Fact]
    public async Task Should_rebuild_app_and_include_changes_during_rebuild()
    {
        var created = Created(TextData("field", "Version1"), 0);
        var updated = Updated(TextData("field", "Version2"), 1);

        SetupStream(created);

        var running = job.RunAsync(CreateRunContext(), CancellationToken);

        await WaitForAsync(async () => (await Registry.GetSkipListAsync(AppId.Id, CancellationToken))?.RequestId != null);

        // The content is changed while the app is rebuilt, therefore the indexer skips the event.
        SetupStream(created, updated);

        await process.On([updated]);

        await WaitForAsync(() => Task.FromResult(running.IsCompleted));
        await running;

        Assert.Equal("Version2", Commands.OfType<UpsertIndexEntry>().Last().Texts?[InvariantPartitioning.Key]);

        // The app is indexed normally again.
        Commands.Clear();

        await process.On([Updated(TextData("field", "Version3"), 2)]);

        Assert.Equal("Version3", GetText());
    }

    private async Task WaitForAsync(Func<Task<bool>> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        while (!await condition())
        {
            // Simulates the background synchronization of the text indexer.
            await Coordinator.SynchronizeAsync(cts.Token);
            await Task.Delay(10, cts.Token);
        }
    }

    private JobRunContext CreateRunContext()
    {
        var state = new SimpleState<JobsState>(A.Fake<IPersistenceFactory<JobsState>>(), GetType(), App.Id);

        var jobRequest = RebuildTextIndexJob.BuildRequest(User, App);

        return new JobRunContext(state, A.Fake<IClock>(), default)
        {
            Actor = User,
            Job = new Job { Id = DomainId.NewGuid(), Arguments = jobRequest.Arguments },
            OwnerId = App.Id,
        };
    }
}
