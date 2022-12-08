// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes;

public class RulesIndexTests
{
    private readonly CancellationTokenSource cts = new CancellationTokenSource();
    private readonly CancellationToken ct;
    private readonly IRuleRepository ruleRepository = A.Fake<IRuleRepository>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly RulesIndex sut;

    public RulesIndexTests()
    {
        ct = cts.Token;

        sut = new RulesIndex(ruleRepository);
    }

    [Fact]
    public async Task Should_resolve_rules_by_id()
    {
        var rule = SetupRule(0);

        A.CallTo(() => ruleRepository.QueryAllAsync(appId.Id, ct))
            .Returns(new List<IRuleEntity> { rule });

        var actual = await sut.GetRulesAsync(appId.Id, ct);

        Assert.Same(actual[0], rule);
    }

    [Fact]
    public async Task Should_return_empty_rules_if_rule_not_created()
    {
        var rule = SetupRule(-1);

        A.CallTo(() => ruleRepository.QueryAllAsync(appId.Id, ct))
            .Returns(new List<IRuleEntity> { rule });

        var actual = await sut.GetRulesAsync(appId.Id, ct);

        Assert.Empty(actual);
    }

    [Fact]
    public async Task Should_return_empty_rules_if_rule_deleted()
    {
        var rule = SetupRule(0, true);

        A.CallTo(() => ruleRepository.QueryAllAsync(appId.Id, ct))
            .Returns(new List<IRuleEntity> { rule });

        var actual = await sut.GetRulesAsync(appId.Id, ct);

        Assert.Empty(actual);
    }

    private IRuleEntity SetupRule(long version, bool isDeleted = false)
    {
        return new RuleEntity { AppId = appId, Version = version, IsDeleted = isDeleted };
    }
}
