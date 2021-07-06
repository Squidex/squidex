// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject
{
    public sealed class ContentCommandMiddlewareTests : HandlerTestBase<ContentDomainObject.State>
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
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
            get => contentId;
        }

        public ContentCommandMiddlewareTests()
        {
            requestContext = Context.Anonymous(Mocks.App(AppNamedId));

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new ContentCommandMiddleware(grainFactory, contentEnricher, contextProvider);
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            await HandleAsync(new CreateContent(), 12);

            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>._, A<bool>._, requestContext, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_enricher_if_already_enriched()
        {
            var result = new ContentEntity();

            var context =
                await HandleAsync(new CreateContent(),
                    result);

            Assert.Same(result, context.Result<IEnrichedContentEntity>());

            A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>._, A<bool>._, requestContext, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_content_result()
        {
            var result = A.Fake<IContentEntity>();

            var enriched = new ContentEntity();

            A.CallTo(() => contentEnricher.EnrichAsync(result, true, requestContext, A<CancellationToken>._))
                .Returns(enriched);

            var context =
                await HandleAsync(new CreateContent(),
                    result);

            Assert.Same(enriched, context.Result<IEnrichedContentEntity>());
        }

        private Task<CommandContext> HandleAsync(ContentCommand command, object result)
        {
            command.ContentId = contentId;

            CreateCommand(command);

            var grain = A.Fake<IContentGrain>();

            A.CallTo(() => grain.ExecuteAsync(A<J<CommandRequest>>._))
                .Returns(new CommandResult(command.AggregateId, 1, 0, result));

            A.CallTo(() => grainFactory.GetGrain<IContentGrain>(command.AggregateId.ToString(), null))
                .Returns(grain);

            return HandleAsync(sut, command);
        }
    }
}
