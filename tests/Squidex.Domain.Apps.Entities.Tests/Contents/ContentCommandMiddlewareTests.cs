// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentCommandMiddlewareTests : HandlerTestBase<ContentState>
    {
        private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
        private readonly Guid contentId = Guid.NewGuid();
        private readonly ContentCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override Guid Id
        {
            get { return contentId; }
        }

        public ContentCommandMiddlewareTests()
        {
            sut = new ContentCommandMiddleware(A.Fake<IGrainFactory>(), contentEnricher);
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(12);

            await sut.HandleAsync(context);

            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>.Ignored, User))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_enricher_if_already_enriched()
        {
            var result = new ContentEntity();

            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(result, context.Result<IEnrichedContentEntity>());

            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>.Ignored, User))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_content_result()
        {
            var result = A.Fake<IContentEntity>();

            var command = CreateCommand(new MyCommand());
            var context = CreateContextForCommand(command);

            context.Complete(result);

            var enriched = new ContentEntity();

            A.CallTo(() => contentEnricher.EnrichAsync(result, User))
                .Returns(enriched);

            await sut.HandleAsync(context);

            Assert.Same(enriched, context.Result<IEnrichedContentEntity>());
        }
    }
}
