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
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class EnrichWithWorkflowsTests
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly ISchemaEntity schema;
        private readonly Context requestContext;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ProvideSchema schemaProvider;
        private readonly EnrichWithWorkflows sut;

        public EnrichWithWorkflowsTests()
        {
            requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

            schema = Mocks.Schema(appId, schemaId);
            schemaProvider = x => Task.FromResult(schema);

            A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(A<Context>.Ignored, schemaId.Id.ToString()))
                .Returns(schema);

            sut = new EnrichWithWorkflows(contentWorkflow);
        }

        [Fact]
        public async Task Should_enrich_content_with_status_color()
        {
            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            await sut.EnrichAsync(requestContext, new[] { source }, schemaProvider);

            Assert.Equal(StatusColors.Published, source.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_default_color_if_not_found()
        {
            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(Task.FromResult<StatusInfo>(null!));

            var ctx = requestContext.WithResolveFlow(true);

            await sut.EnrichAsync(ctx, new[] { source }, schemaProvider);

            Assert.Equal(StatusColors.Draft, source.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_can_update()
        {
            var source = new ContentEntity { SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.CanUpdateAsync(source, requestContext.User))
                .Returns(true);

            var ctx = requestContext.WithResolveFlow(true);

            await sut.EnrichAsync(ctx, new[] { source }, schemaProvider);

            Assert.True(source.CanUpdate);
        }

        [Fact]
        public async Task Should_not_enrich_content_with_can_update_if_disabled_in_context()
        {
            requestContext.WithResolveFlow(false);

            var source = new ContentEntity { SchemaId = schemaId };

            var ctx = requestContext.WithResolveFlow(false);

            await sut.EnrichAsync(ctx, new[] { source }, schemaProvider);

            Assert.False(source.CanUpdate);

            A.CallTo(() => contentWorkflow.CanUpdateAsync(source, requestContext.User))
                .MustNotHaveHappened();
        }

        private ContentEntity PublishedContent()
        {
            return new ContentEntity { Status = Status.Published, SchemaId = schemaId };
        }
    }
}
