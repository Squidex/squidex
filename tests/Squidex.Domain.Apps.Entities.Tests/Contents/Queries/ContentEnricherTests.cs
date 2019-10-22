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
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Queries
{
    public class ContentEnricherTests
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetUrlGenerator assetUrlGenerator = A.Fake<IAssetUrlGenerator>();
        private readonly ISchemaEntity schema;
        private readonly Context requestContext;
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ContentEnricher sut;

        public ContentEnricherTests()
        {
            requestContext = new Context(Mocks.ApiUser(), Mocks.App(appId));

            schema = Mocks.Schema(appId, schemaId);

            A.CallTo(() => contentQuery.GetSchemaOrThrowAsync(A<Context>.Ignored, schemaId.Id.ToString()))
                .Returns(schema);

            sut = new ContentEnricher(assetQuery, assetUrlGenerator, new Lazy<IContentQueryService>(() => contentQuery), contentWorkflow);
        }

        [Fact]
        public async Task Should_add_app_version_and_schema_as_dependency()
        {
            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.Contains(requestContext.App.Version, result.CacheDependencies);

            Assert.Contains(schema.Id, result.CacheDependencies);
            Assert.Contains(schema.Version, result.CacheDependencies);
        }

        [Fact]
        public async Task Should_enrich_with_reference_fields()
        {
            var ctx = new Context(Mocks.FrontendUser(), requestContext.App);

            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, ctx);

            Assert.NotNull(result.ReferenceFields);
        }

        [Fact]
        public async Task Should_not_enrich_with_reference_fields_when_not_frontend()
        {
            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.Null(result.ReferenceFields);
        }

        [Fact]
        public async Task Should_enrich_with_schema_names()
        {
            var ctx = new Context(Mocks.FrontendUser(), requestContext.App);

            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, ctx);

            Assert.Equal("my-schema", result.SchemaName);
            Assert.Equal("my-schema", result.SchemaDisplayName);
        }

        [Fact]
        public async Task Should_not_enrich_with_schema_names_when_not_frontend()
        {
            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.Null(result.SchemaName);
            Assert.Null(result.SchemaDisplayName);
        }

        [Fact]
        public async Task Should_enrich_content_with_status_color()
        {
            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.Equal(StatusColors.Published, result.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_default_color_if_not_found()
        {
            var source = PublishedContent();

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(Task.FromResult<StatusInfo>(null));

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.Equal(StatusColors.Draft, result.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_can_update()
        {
            requestContext.WithResolveFlow(true);

            var source = new ContentEntity { SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.CanUpdateAsync(source))
                .Returns(true);

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.True(result.CanUpdate);
        }

        [Fact]
        public async Task Should_not_enrich_content_with_can_update_if_disabled_in_context()
        {
            requestContext.WithResolveFlow(false);

            var source = new ContentEntity { SchemaId = schemaId };

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.False(result.CanUpdate);

            A.CallTo(() => contentWorkflow.CanUpdateAsync(source))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_multiple_contents_and_cache_color()
        {
            var source1 = PublishedContent();
            var source2 = PublishedContent();

            var source = new IContentEntity[]
            {
                source1,
                source2
            };

            A.CallTo(() => contentWorkflow.GetInfoAsync(source1))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.Equal(StatusColors.Published, result[0].StatusColor);
            Assert.Equal(StatusColors.Published, result[1].StatusColor);

            A.CallTo(() => contentWorkflow.GetInfoAsync(A<IContentEntity>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        private ContentEntity PublishedContent()
        {
            return new ContentEntity { Status = Status.Published, SchemaId = schemaId };
        }
    }
}
