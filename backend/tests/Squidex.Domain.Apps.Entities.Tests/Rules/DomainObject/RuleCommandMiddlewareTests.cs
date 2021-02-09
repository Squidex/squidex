// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject
{
    public sealed class RuleCommandMiddlewareTests : HandlerTestBase<RuleDomainObject.State>
    {
        private readonly IRuleEnricher ruleEnricher = A.Fake<IRuleEnricher>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly DomainId ruleId = DomainId.NewGuid();
        private readonly Context requestContext;
        private readonly RuleCommandMiddleware sut;

        public sealed class MyCommand : SquidexCommand
        {
        }

        protected override DomainId Id
        {
            get { return ruleId; }
        }

        public RuleCommandMiddlewareTests()
        {
            requestContext = Context.Anonymous(Mocks.App(AppNamedId));

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new RuleCommandMiddleware(A.Fake<IGrainFactory>(), ruleEnricher, contextProvider);
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            var context =
                CreateCommandContext(
                     new MyCommand());

            context.Complete(12);

            await sut.HandleAsync(context);

            A.CallTo(() => ruleEnricher.EnrichAsync(A<IEnrichedRuleEntity>._, requestContext))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_enricher_if_already_enriched()
        {
            var result = new RuleEntity();

            var context =
                CreateCommandContext(
                    new MyCommand());

            context.Complete(result);

            await sut.HandleAsync(context);

            Assert.Same(result, context.Result<IEnrichedRuleEntity>());

            A.CallTo(() => ruleEnricher.EnrichAsync(A<IEnrichedRuleEntity>._, requestContext))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_rule_result()
        {
            var result = A.Fake<IRuleEntity>();

            var context =
                CreateCommandContext(
                    new MyCommand());

            context.Complete(result);

            var enriched = new RuleEntity();

            A.CallTo(() => ruleEnricher.EnrichAsync(result, requestContext))
                .Returns(enriched);

            await sut.HandleAsync(context);

            Assert.Same(enriched, context.Result<IEnrichedRuleEntity>());
        }
    }
}
