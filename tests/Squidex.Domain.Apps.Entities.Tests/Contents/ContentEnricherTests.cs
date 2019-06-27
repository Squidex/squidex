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
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ContentEnricherTests
    {
        private readonly IContentWorkflow workflow = A.Fake<IContentWorkflow>();
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly ContentEnricher sut;

        public ContentEnricherTests()
        {
            sut = new ContentEnricher(workflow);
        }

        [Fact]
        public async Task Should_enrich_content_with_status_color()
        {
            var source = new ContentEntity { Status = Status.Published, SchemaId = schemaId };

            A.CallTo(() => workflow.GetInfoAsync(source))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(source);

            Assert.Equal(StatusColors.Published, result.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_default_color_if_not_found()
        {
            var source = new ContentEntity { Status = Status.Published, SchemaId = schemaId };

            A.CallTo(() => workflow.GetInfoAsync(source))
                .Returns(Task.FromResult<StatusInfo>(null));

            var result = await sut.EnrichAsync(source);

            Assert.Equal(StatusColors.Draft, result.StatusColor);
        }

        [Fact]
        public async Task Should_enrich_content_with_can_update()
        {
            var source = new ContentEntity { SchemaId = schemaId };

            A.CallTo(() => workflow.CanUpdateAsync(source))
                .Returns(true);

            var result = await sut.EnrichAsync(source);

            Assert.True(result.CanUpdate);
        }

        [Fact]
        public async Task Should_enrich_multiple_contents_and_cache_color()
        {
            var source1 = new ContentEntity { Status = Status.Published, SchemaId = schemaId };
            var source2 = new ContentEntity { Status = Status.Published, SchemaId = schemaId };

            A.CallTo(() => workflow.GetInfoAsync(source1))
                .Returns(new StatusInfo(Status.Published, StatusColors.Published));

            var result = await sut.EnrichAsync(new[] { source1, source2 });

            Assert.Equal(StatusColors.Published, result[0].StatusColor);
            Assert.Equal(StatusColors.Published, result[1].StatusColor);

            A.CallTo(() => workflow.GetInfoAsync(A<IContentEntity>.Ignored))
                .MustHaveHappenedOnceExactly();
        }
    }
}
