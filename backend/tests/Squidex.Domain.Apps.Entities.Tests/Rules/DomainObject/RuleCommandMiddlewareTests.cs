// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject
{
    public sealed class RuleCommandMiddlewareTests : HandlerTestBase<RuleDomainObject.State>
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
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
            get => ruleId;
        }

        public RuleCommandMiddlewareTests()
        {
            requestContext = Context.Anonymous(Mocks.App(AppNamedId));

            A.CallTo(() => contextProvider.Context)
                .Returns(requestContext);

            sut = new RuleCommandMiddleware(grainFactory, ruleEnricher, contextProvider);
        }

        [Fact]
        public async Task Should_not_invoke_enricher_for_other_result()
        {
            await HandleAsync(new EnableRule(), 12);

            A.CallTo(() => ruleEnricher.EnrichAsync(A<IEnrichedRuleEntity>._, requestContext, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_not_invoke_enricher_if_already_enriched()
        {
            var result = new RuleEntity();

            var context =
                await HandleAsync(new EnableRule(),
                    result);

            Assert.Same(result, context.Result<IEnrichedRuleEntity>());

            A.CallTo(() => ruleEnricher.EnrichAsync(A<IEnrichedRuleEntity>._, requestContext, default))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_enrich_rule_result()
        {
            var result = A.Fake<IRuleEntity>();

            var enriched = new RuleEntity();

            A.CallTo(() => ruleEnricher.EnrichAsync(result, requestContext, default))
                .Returns(enriched);

            var context =
                await HandleAsync(new EnableRule(),
                    result);

            Assert.Same(enriched, context.Result<IEnrichedRuleEntity>());
        }

        private Task<CommandContext> HandleAsync(RuleCommand command, object result)
        {
            command.RuleId = ruleId;

            CreateCommand(command);

            var grain = A.Fake<IRuleGrain>();

            A.CallTo(() => grain.ExecuteAsync(A<J<CommandRequest>>._))
                .Returns(new CommandResult(command.AggregateId, 1, 0, result));

            A.CallTo(() => grainFactory.GetGrain<IRuleGrain>(command.AggregateId.ToString(), null))
                .Returns(grain);

            return HandleAsync(sut, command);
        }
    }
}
