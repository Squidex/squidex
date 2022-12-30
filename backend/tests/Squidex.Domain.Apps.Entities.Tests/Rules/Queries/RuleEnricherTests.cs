// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Caching;

namespace Squidex.Domain.Apps.Entities.Rules.Queries;

public class RuleEnricherTests : GivenContext
{
    private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
    private readonly IRequestCache requestCache = A.Fake<IRequestCache>();
    private readonly RuleEnricher sut;

    public RuleEnricherTests()
    {
        sut = new RuleEnricher(ruleEventRepository, requestCache);
    }

    [Fact]
    public async Task Should_not_enrich_if_statistics_not_found()
    {
        var source = CreateRule();

        var actual = await sut.EnrichAsync(source, ApiContext, CancellationToken);

        Assert.Equal(0, actual.NumFailed);
        Assert.Equal(0, actual.NumSucceeded);

        Assert.Null(actual.LastExecuted);

        A.CallTo(() => requestCache.AddDependency(source.UniqueId, source.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency<Instant?>(null))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_rules_with_found_statistics()
    {
        var source = CreateRule();

        var stats = new RuleStatistics
        {
            RuleId = source.Id,
            NumFailed = 12,
            NumSucceeded = 17,
            LastExecuted = SystemClock.Instance.GetCurrentInstant()
        };

        A.CallTo(() => ruleEventRepository.QueryStatisticsByAppAsync(AppId.Id, CancellationToken))
            .Returns(new List<RuleStatistics> { stats });

        await sut.EnrichAsync(source, ApiContext, CancellationToken);

        A.CallTo(() => requestCache.AddDependency(source.UniqueId, source.Version))
            .MustHaveHappened();

        A.CallTo(() => requestCache.AddDependency(stats.LastExecuted))
            .MustHaveHappened();
    }

    private IRuleEntity CreateRule()
    {
        return new RuleEntity { AppId = AppId, Id = DomainId.NewGuid(), Version = 13 };
    }
}
