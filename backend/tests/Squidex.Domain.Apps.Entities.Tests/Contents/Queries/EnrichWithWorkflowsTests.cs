// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class EnrichWithWorkflowsTests
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly Context requestContext;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly RefToken user = new RefToken(RefTokenType.Subject, "me");
        private readonly EnrichWithWorkflows sut;

        public EnrichWithWorkflowsTests()
        {
            requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

            sut = new EnrichWithWorkflows(contentWorkflow);
        }

        [Fact]
        public async Task Should_enrich_content_with_status_color()
        {
            var content = new ContentEntity { SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.GetInfoAsync(content, content.Status))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            await sut.EnrichAsync(requestContext, new[] { content }, null!);

            Assert.Equal(StatusColors.Published, content.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_new_status_color()
        {
            var content = new ContentEntity { SchemaId = schemaId, NewStatus = Status.Archived };

            A.CallTo(() => contentWorkflow.GetInfoAsync(content, content.NewStatus.Value))
                .Returns(new StatusInfo(Status.Published, StatusColors.Archived));

            await sut.EnrichAsync(requestContext, new[] { content }, null!);

            Assert.Equal(StatusColors.Archived, content.NewStatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_scheduled_status_color()
        {
            var content = new ContentEntity { SchemaId = schemaId, ScheduleJob = ScheduleJob.Build(Status.Archived, user, default) };

            A.CallTo(() => contentWorkflow.GetInfoAsync(content, content.ScheduleJob.Status))
                .Returns(new StatusInfo(Status.Published, StatusColors.Archived));

            await sut.EnrichAsync(requestContext, new[] { content }, null!);

            Assert.Equal(StatusColors.Archived, content.ScheduledStatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_default_color_if_not_found()
        {
            var content = new ContentEntity { SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.GetInfoAsync(content, content.Status))
                .Returns(Task.FromResult<StatusInfo>(null!));

            var ctx = requestContext.WithResolveFlow(true);

            await sut.EnrichAsync(ctx, new[] { content }, null!);

            Assert.Equal(StatusColors.Draft, content.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_can_update()
        {
            var content = new ContentEntity { SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.CanUpdateAsync(content, content.Status, requestContext.User))
                .Returns(true);

            var ctx = requestContext.WithResolveFlow(true);

            await sut.EnrichAsync(ctx, new[] { content }, null!);

            Assert.True(content.CanUpdate);
        }

        [Fact]
        public async Task Should_not_enrich_content_with_can_update_if_disabled_in_context()
        {
            requestContext.WithResolveFlow(false);

            var content = new ContentEntity { SchemaId = schemaId };

            var ctx = requestContext.WithResolveFlow(false);

            await sut.EnrichAsync(ctx, new[] { content }, null!);

            Assert.False(content.CanUpdate);

            A.CallTo(() => contentWorkflow.CanUpdateAsync(content, content.EditingStatus, requestContext.User))
                .MustNotHaveHappened();
        }
    }
}
