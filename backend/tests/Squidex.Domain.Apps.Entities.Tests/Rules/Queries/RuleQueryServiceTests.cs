// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Queries;

public class RuleQueryServiceTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IRulesIndex rulesIndex = A.Fake<IRulesIndex>();
    private readonly IRuleEnricher ruleEnricher = A.Fake<IRuleEnricher>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly Context requestContext;
    private readonly RuleQueryService sut;

    public RuleQueryServiceTests()
    {
        ct = cts.Token;

        requestContext = Context.Anonymous(Mocks.App(appId));

        sut = new RuleQueryService(rulesIndex, ruleEnricher);
    }

    [Fact]
    public async Task Should_get_rules_from_index_and_enrich()
    {
        var original = new List<IRuleEntity>
        {
            new RuleEntity()
        };

        var enriched = new List<IEnrichedRuleEntity>
        {
            new RuleEntity()
        };

        A.CallTo(() => rulesIndex.GetRulesAsync(appId.Id, ct))
            .Returns(original);

        A.CallTo(() => ruleEnricher.EnrichAsync(original, requestContext, ct))
            .Returns(enriched);

        var actual = await sut.QueryAsync(requestContext, ct);

        Assert.Same(enriched, actual);
    }
}
