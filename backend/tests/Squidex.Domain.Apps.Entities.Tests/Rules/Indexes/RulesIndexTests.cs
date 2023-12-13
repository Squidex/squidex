// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes;

public class RulesIndexTests : GivenContext
{
    private readonly IRuleRepository ruleRepository = A.Fake<IRuleRepository>();
    private readonly RulesIndex sut;

    public RulesIndexTests()
    {
        sut = new RulesIndex(ruleRepository);
    }

    [Fact]
    public async Task Should_resolve_rules_by_id()
    {
        var rule = CreateRule();

        A.CallTo(() => ruleRepository.QueryAllAsync(AppId.Id, CancellationToken))
            .Returns([rule]);

        var actual = await sut.GetRulesAsync(AppId.Id, CancellationToken);

        Assert.Same(actual[0], rule);
    }

    [Fact]
    public async Task Should_return_empty_rules_if_rule_not_created()
    {
        var rule = CreateRule() with { Version = -1 };

        A.CallTo(() => ruleRepository.QueryAllAsync(AppId.Id, CancellationToken))
            .Returns([rule]);

        var actual = await sut.GetRulesAsync(AppId.Id, CancellationToken);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_empty_rules_if_rule_deleted()
    {
        var rule = CreateRule() with { IsDeleted = true };

        A.CallTo(() => ruleRepository.QueryAllAsync(AppId.Id, CancellationToken))
            .Returns([rule]);

        var actual = await sut.GetRulesAsync(AppId.Id, CancellationToken);

        Assert.Empty(actual);
    }
}
