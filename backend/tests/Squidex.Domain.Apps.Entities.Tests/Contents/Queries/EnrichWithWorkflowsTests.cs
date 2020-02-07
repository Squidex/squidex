//// ==========================================================================
////  Squidex Headless CMS
//// ==========================================================================
////  Copyright (c) Squidex UG (haftungsbeschraenkt)
////  All rights reserved. Licensed under the MIT license.
//// ==========================================================================

//using System;
//using System.Threading.Tasks;
//using FakeItEasy;
//using Squidex.Domain.Apps.Core.Contents;
//using Squidex.Domain.Apps.Entities.Contents.Queries.Steps;
//using Squidex.Domain.Apps.Entities.Schemas;
//using Squidex.Domain.Apps.Entities.TestHelpers;
//using Squidex.Infrastructure;
//using Xunit;

//namespace Squidex.Domain.Apps.Entities.Contents.Queries
//{
//    public class EnrichWithWorkflowsTests
//    {
//        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
//        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
//        private readonly ISchemaEntity schema;
//        private readonly Context requestContext;
//        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
//        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
//        private readonly ProvideSchema schemaProvider;
//        private readonly EnrichWithWorkflows sut;

//        public EnrichWithWorkflowsTests()
//        {
//            requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

//            schema = Mocks.Schema(appId, schemaId);
//            schemaProvider = x => Task.FromResult(schema);

//            A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(A<Context>.Ignored, schemaId.Id.ToString()))
//                .Returns(schema);

//            sut = new EnrichWithWorkflows(contentWorkflow);
//        }

//        [Fact]
//        public async Task Should_enrich_content_with_status_color()
//        {
//            var content = CreateContent();

//            A.CallTo(() => contentWorkflow.GetInfoAsync(content))
//                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

//            await sut.EnrichAsync(requestContext, new[] { content }, schemaProvider);

//            Assert.Equal(StatusColors.Published, content.StatusColor);
//        }

//        [Fact]
//        public async Task Should_enrich_content_with_default_color_if_not_found()
//        {
//            var content = CreateContent();

//            A.CallTo(() => contentWorkflow.GetInfoAsync(content))
//                .Returns(Task.FromResult<StatusInfo>(null!));

//            var ctx = requestContext.WithResolveFlow(true);

//            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider);

//            Assert.Equal(StatusColors.Draft, content.StatusColor);
//        }

//        [Fact]
//        public async Task Should_enrich_content_with_can_update()
//        {
//            var content = new ContentEntity { SchemaId = schemaId };

//            A.CallTo(() => contentWorkflow.CanUpdateAsync(content, requestContext.User))
//                .Returns(true);

//            var ctx = requestContext.WithResolveFlow(true);

//            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider);

//            Assert.True(content.CanUpdate);
//        }

//        [Fact]
//        public async Task Should_not_enrich_content_with_can_update_if_disabled_in_context()
//        {
//            requestContext.WithResolveFlow(false);

//            var content = new ContentEntity { SchemaId = schemaId };

//            var ctx = requestContext.WithResolveFlow(false);

//            await sut.EnrichAsync(ctx, new[] { content }, schemaProvider);

//            Assert.False(content.CanUpdate);

//            A.CallTo(() => contentWorkflow.CanUpdateAsync(content, requestContext.User))
//                .MustNotHaveHappened();
//        }

//        private ContentEntity CreateContent()
//        {
//            return new ContentEntity { SchemaId = schemaId };
//        }
//    }
//}
