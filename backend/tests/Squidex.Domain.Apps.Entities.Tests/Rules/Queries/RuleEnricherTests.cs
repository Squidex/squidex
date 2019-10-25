// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Queries
{
    public class RuleEnricherTests
    {
        private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly Context requestContext = Context.Anonymous();
        private readonly RuleEnricher sut;

        public RuleEnricherTests()
        {
            sut = new RuleEnricher(ruleEventRepository);
        }

        [Fact]
        public async Task Should_not_enrich_if_statistics_not_found()
        {
            var source = new RuleEntity { AppId = appId };

            var result = await sut.EnrichAsync(source, requestContext);

            Assert.Equal(0, result.NumFailed);
            Assert.Equal(0, result.NumSucceeded);
            Assert.Null(result.LastExecuted);
        }

        [Fact]
        public async Task Should_enrich_rules_with_found_statistics()
        {
            var source1 = new RuleEntity { AppId = appId, Id = Guid.NewGuid() };
            var source2 = new RuleEntity { AppId = appId, Id = Guid.NewGuid() };

            var stats = new RuleStatistics
            {
                RuleId = source1.Id,
                NumFailed = 12,
                NumSucceeded = 17,
                LastExecuted = SystemClock.Instance.GetCurrentInstant()
            };

            A.CallTo(() => ruleEventRepository.QueryStatisticsByAppAsync(appId.Id))
                .Returns(new List<RuleStatistics> { stats });

            var result = await sut.EnrichAsync(new[] { source1, source2 }, requestContext);

            var enriched1 = result.ElementAt(0);

            Assert.Equal(12, enriched1.NumFailed);
            Assert.Equal(17, enriched1.NumSucceeded);
            Assert.Equal(stats.LastExecuted, enriched1.LastExecuted);

            var enriched2 = result.ElementAt(1);

            Assert.Equal(0, enriched2.NumFailed);
            Assert.Equal(0, enriched2.NumSucceeded);
            Assert.Null(enriched2.LastExecuted);
        }
    }
}
