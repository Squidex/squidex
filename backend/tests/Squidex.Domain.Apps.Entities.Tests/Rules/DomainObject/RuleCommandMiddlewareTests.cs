// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Rules.DomainObject;

public sealed class RuleCommandMiddlewareTests : HandlerTestBase<RuleDomainObject.State>
{
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IRuleEnricher ruleEnricher = A.Fake<IRuleEnricher>();
    private readonly DomainId ruleId = DomainId.NewGuid();
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
        sut = new RuleCommandMiddleware(domainObjectFactory, ruleEnricher, ApiContextProvider);
    }

    [Fact]
    public async Task Should_not_invoke_enricher_for_other_actual()
    {
        await HandleAsync(new EnableRule(), 12);

        A.CallTo(() => ruleEnricher.EnrichAsync(A<IEnrichedRuleEntity>._, ApiContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_enricher_if_already_enriched()
    {
        var rule = new RuleEntity();

        var context =
            await HandleAsync(new EnableRule(),
                rule);

        Assert.Same(rule, context.Result<IEnrichedRuleEntity>());

        A.CallTo(() => ruleEnricher.EnrichAsync(A<IEnrichedRuleEntity>._, ApiContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_rule()
    {
        var rule = A.Fake<IRuleEntity>();

        var enriched = new RuleEntity();

        A.CallTo(() => ruleEnricher.EnrichAsync(rule, ApiContext, CancellationToken))
            .Returns(enriched);

        var context =
            await HandleAsync(new EnableRule(),
                rule);

        Assert.Same(enriched, context.Result<IEnrichedRuleEntity>());
    }

    private Task<CommandContext> HandleAsync(RuleCommand command, object actual)
    {
        command.RuleId = ruleId;

        CreateCommand(command);

        var domainObject = A.Fake<RuleDomainObject>();

        A.CallTo(() => domainObject.ExecuteAsync(A<IAggregateCommand>._, CancellationToken))
            .Returns(new CommandResult(command.AggregateId, 1, 0, actual));

        A.CallTo(() => domainObjectFactory.Create<RuleDomainObject>(command.AggregateId))
            .Returns(domainObject);

        return HandleAsync(sut, command, CancellationToken);
    }
}
