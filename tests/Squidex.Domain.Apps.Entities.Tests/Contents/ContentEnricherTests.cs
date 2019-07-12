// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentEnricherTests
    {
        private readonly IContentWorkflow contentWorkflow = A.Fake<IContentWorkflow>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly ClaimsPrincipal user = new ClaimsPrincipal();
        private readonly Context context = new Context();
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ContentEnricher sut;

        public ContentEnricherTests()
        {
            A.CallTo(() => contextProvider.Context)
                .Returns(context);

            sut = new ContentEnricher(contentWorkflow, contextProvider);
        }

        [Fact]
        public async Task Should_enrich_content_with_status_color()
        {
            var source = new ContentEntity { Status = Status.Published, SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source, user);

            Assert.Equal(StatusColors.Published, result.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_default_color_if_not_found()
        {
            var source = new ContentEntity { Status = Status.Published, SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.GetInfoAsync(source))
                .Returns(Task.FromResult<StatusInfo>(null));

            var result = await sut.EnrichAsync(source, user);

            Assert.Equal(StatusColors.Draft, result.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_can_update()
        {
            context.WithResolveFlow(true);

            var source = new ContentEntity { SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.CanUpdateAsync(source))
                .Returns(true);

            var result = await sut.EnrichAsync(source, user);

            Assert.True(result.CanUpdate);
        }

        [Fact]
        public async Task Should_not_enrich_content_with_can_update_if_disabled_in_context()
        {
            context.WithResolveFlow(false);

            var source = new ContentEntity { SchemaId = schemaId };

            var result = await sut.EnrichAsync(source, user);

            Assert.False(result.CanUpdate);

            A.CallTo(() => contentWorkflow.CanUpdateAsync(source))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_multiple_contents_and_cache_color()
        {
            var source1 = new ContentEntity { Status = Status.Published, SchemaId = schemaId };
            var source2 = new ContentEntity { Status = Status.Published, SchemaId = schemaId };

            A.CallTo(() => contentWorkflow.GetInfoAsync(source1))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(new[] { source1, source2 }, user);

            Assert.Equal(StatusColors.Published, result[0].StatusColor);
            Assert.Equal(StatusColors.Published, result[1].StatusColor);

            A.CallTo(() => contentWorkflow.GetInfoAsync(A<IContentEntity>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }
}
