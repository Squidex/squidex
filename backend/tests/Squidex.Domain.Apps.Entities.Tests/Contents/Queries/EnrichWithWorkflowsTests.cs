// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;

#pragma warning disable CA2012 // Use ValueTasks correctly

namespace Squidex.Domain.Apps.Entities.Contents.Queries;

public class EnrichWithWorkflowsTests : GivenContext
{
    private readonly IContentWorkflow workflow = A.Fake<IContentWorkflow>();
    private readonly IContentWorkflows workflows = A.Fake<IContentWorkflows>();
    private readonly EnrichWithWorkflows sut;

    public EnrichWithWorkflowsTests()
    {
        A.CallTo(() => workflows.GetWorkflowAsync(App, Schema, A<CancellationToken>._))
            .Returns(workflow);

        sut = new EnrichWithWorkflows(workflows);
    }

    [Fact]
    public async Task Should_enrich_content_with_next_statuses()
    {
        var content = CreateContent();

        var nexts = new[]
        {
            new StatusInfo(Status.Published, StatusColors.Published),
        };

        A.CallTo(() => workflow.GetNext(content, content.Status, FrontendContext.UserPrincipal))
            .Returns(nexts);

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(nexts, content.NextStatuses);
    }

    [Fact]
    public async Task Should_enrich_content_with_next_statuses_if_draft_singleton()
    {
        var content = CreateContent() with { IsSingleton = true, Status = Status.Draft };

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), default);

        Assert.Equal(Status.Published, content.NextStatuses?.Single().Status);

        A.CallTo(() => workflow.GetNext(content, A<Status>._, FrontendContext.UserPrincipal))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_content_with_next_statuses_if_published_singleton()
    {
        var content = CreateContent() with { IsSingleton = true };

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.Empty(content.NextStatuses!);

        A.CallTo(() => workflow.GetNext(content, A<Status>._, FrontendContext.UserPrincipal))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_content_with_status_color()
    {
        var content = CreateContent();

        A.CallTo(() => workflow.GetInfo(content.Status))
            .Returns(new StatusInfo(Status.Published, StatusColors.Published));

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(StatusColors.Published, content.StatusColor);
    }

    [Fact]
    public async Task Should_enrich_content_with_new_status_color()
    {
        var content = CreateContent() with { NewStatus = Status.Archived };

        A.CallTo(() => workflow.GetInfo(content.NewStatus!.Value))
            .Returns(new StatusInfo(Status.Published, StatusColors.Archived));

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(StatusColors.Archived, content.NewStatusColor);
    }

    [Fact]
    public async Task Should_enrich_content_with_scheduled_status_color()
    {
        var content = CreateContent() with { ScheduleJob = ScheduleJob.Build(Status.Archived, User, Timestamp()) };

        A.CallTo(() => workflow.GetInfo(content.ScheduleJob.Status))
            .Returns(new StatusInfo(Status.Published, StatusColors.Archived));

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(StatusColors.Archived, content.ScheduledStatusColor);
    }

    [Fact]
    public async Task Should_enrich_content_with_default_color_if_not_found()
    {
        var content = CreateContent();

        A.CallTo(() => workflow.GetInfo(content.Status))
            .Returns(null);

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.Equal(StatusColors.Draft, content.StatusColor);
    }

    [Fact]
    public async Task Should_enrich_content_with_can_update()
    {
        var content = CreateContent();

        A.CallTo(() => workflow.CanUpdate(content, content.Status, FrontendContext.UserPrincipal))
            .Returns(true);

        await sut.EnrichAsync(FrontendContext, [content], SchemaProvider(), CancellationToken);

        Assert.True(content.CanUpdate);
    }

    [Fact]
    public async Task Should_not_enrich_content_with_can_update_if_disabled_in_context()
    {
        var content = CreateContent();

        await sut.EnrichAsync(ApiContext.Clone(b => b.WithResolveFlow(false)), [content], SchemaProvider(), CancellationToken);

        Assert.False(content.CanUpdate);

        A.CallTo(() => workflow.CanUpdate(content, A<Status>._, FrontendContext.UserPrincipal))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_resolve_workflow_once_per_schema()
    {
        var content1 = CreateContent();
        var content2 = CreateContent();

        await sut.EnrichAsync(FrontendContext, [content1, content2], SchemaProvider(), CancellationToken);

        A.CallTo(() => workflows.GetWorkflowAsync(App, Schema, A<CancellationToken>._))
            .MustHaveHappenedOnceExactly();
    }

    private ProvideSchema SchemaProvider()
    {
        return x => Task.FromResult((Schema, ResolvedComponents.Empty));
    }
}
