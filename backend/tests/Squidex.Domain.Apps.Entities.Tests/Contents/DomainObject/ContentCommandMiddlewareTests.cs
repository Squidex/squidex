// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed class ContentCommandMiddlewareTests : HandlerTestBase<ContentDomainObject.State>
    {
        private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly DomainId contentId = DomainId.NewGuid();
        private readonly Context requestContext;
        private readonly ContentCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override DomainId Id
        {
            get { return contentId; }
        }

        public ContentCommandMiddlewareTests()
        {
            requestContext = Context.Anonymous(Mocks.App(AppNamedId));

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new ContentCommandMiddleware(A.Fake<IGrainFactory>(), contentEnricher, contextProvider);
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            var context =
                CreateCommandContext(
                    new MyCommand());

            context.Complete(12);

            await sut.HandleAsync(context);

            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>._, A<bool>._, requestContext))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_enricher_if_already_enriched()
        {
            var result = new ContentEntity();

            var context =
                CreateCommandContext(
                    new MyCommand());

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(result, context.Result<IEnrichedContentEntity>());

            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>._, A<bool>._, requestContext))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_content_result()
        {
            var result = A.Fake<IContentEntity>();

            var context =
                CreateCommandContext(
                    new MyCommand());

            context.Complete(result);

            var enriched = new ContentEntity();

            A.CallTo(() => contentEnricher.EnrichAsync(result, true, requestContext))
                .Returns(enriched);

            await sut.HandleAsync(context);

            Assert.Same(enriched, context.Result<IEnrichedContentEntity>());
        }
    }
}
