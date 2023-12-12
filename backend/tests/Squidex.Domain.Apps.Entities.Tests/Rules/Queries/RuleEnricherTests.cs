// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Rules.Queries;

public class RuleEnricherTests : GivenContext
{
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly RuleEnricher sut;

    public RuleEnricherTests()
    {
        sut = new RuleEnricher(ruleUsageTracker, requestCache);
    }

    [Fact]
    public async Task Should_not_enrich_if_statistics_not_found()
    {
        var source = CreateRule() with { Version = 13 };

        var actual = await sut.EnrichAsync(source, ApiContext, CancellationToken);

        Assert.Equal(0, actual.NumSucceeded);
        Assert.Equal(0, actual.NumFailed);

        A.CallTo(() => requestCache.AddDependency(source.UniqueId, source.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency<Instant?>(null))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_rules_with_found_statistics()
    {
        var source = CreateRule() with { Version = 13 };

        var stats = new Dictionary<DomainId, RuleCounters>
        {
            [source.Id] = new RuleCounters(42, 17, 12)
        };

        A.CallTo(() => ruleUsageTracker.GetTotalByAppAsync(AppId.Id, CancellationToken))
            .Returns(stats);

        var actual = await sut.EnrichAsync(source, ApiContext, CancellationToken);

        Assert.Equal(17, actual.NumSucceeded);
        Assert.Equal(12, actual.NumFailed);

        A.CallTo(() => requestCache.AddDependency(source.UniqueId, source.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(17L))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(12L))
            .MustHaveHappened();
    }
}
