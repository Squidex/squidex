﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Entities.Rules.Indexes;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Queries
{
    public class RuleQueryServiceTests
    {
        private readonly IRulesIndex rulesIndex = A.Fake<IRulesIndex>();
        private readonly IRuleEnricher ruleEnricher = A.Fake<IRuleEnricher>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly Context requestContext = Context.Anonymous();
        private readonly RuleQueryService sut;

        public RuleQueryServiceTests()
        {
            requestContext.App = Mocks.App(appId);

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

            A.CallTo(() => rulesIndex.GetRulesAsync(appId.Id))
                .Returns(original);

            A.CallTo(() => ruleEnricher.EnrichAsync(original, requestContext))
                .Returns(enriched);

            var result = await sut.QueryAsync(requestContext);

            Assert.Same(enriched, result);
        }
    }
}
