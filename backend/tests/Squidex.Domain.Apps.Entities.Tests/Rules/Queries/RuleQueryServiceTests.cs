// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Rules.Queries;

public class RuleQueryServiceTests : GivenContext
{
    private readonly IRulesIndex rulesIndex = A.Fake<IRulesIndex>();
    private readonly IRuleEnricher ruleEnricher = A.Fake<IRuleEnricher>();
    private readonly RuleQueryService sut;

    public RuleQueryServiceTests()
    {
        sut = new RuleQueryService(rulesIndex, ruleEnricher);
    }

    [Fact]
    public async Task Should_get_rules_from_index_and_enrich()
    {
        var original = new List<Rule>
        {
            new Rule()
        };

        var enriched = new List<EnrichedRule>
        {
            CreateRule()
        };

        A.CallTo(() => rulesIndex.GetRulesAsync(AppId.Id, CancellationToken))
            .Returns(original);

        A.CallTo(() => ruleEnricher.EnrichAsync(original, ApiContext, CancellationToken))
            .Returns(enriched);

        var actual = await sut.QueryAsync(ApiContext, CancellationToken);

        Assert.Same(enriched, actual);
    }
}
