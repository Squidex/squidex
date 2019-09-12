// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public class RulesByAppIndexGrainTests
    {
        private readonly IGrainState<RulesByAppIndexGrain.GrainState> grainState = A.Fake<IGrainState<RulesByAppIndexGrain.GrainState>>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid ruleId1 = Guid.NewGuid();
        private readonly Guid ruleId2 = Guid.NewGuid();
        private readonly RulesByAppIndexGrain sut;

        public RulesByAppIndexGrainTests()
        {
            A.CallTo(() => grainState.ClearAsync())
                .Invokes(() => grainState.Value = new RulesByAppIndexGrain.GrainState());

            sut = new RulesByAppIndexGrain(grainState);
            sut.ActivateAsync(appId).Wait();
        }

        [Fact]
        public async Task Should_add_rule_id_to_index()
        {
            await sut.AddRuleAsync(ruleId1);
            await sut.AddRuleAsync(ruleId2);

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(new List<Guid> { ruleId1, ruleId2 }, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_remove_rule_id_from_index()
        {
            await sut.AddRuleAsync(ruleId1);
            await sut.AddRuleAsync(ruleId2);
            await sut.RemoveRuleAsync(ruleId1);

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(new List<Guid> { ruleId2 }, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappenedTwiceOrMore();
        }

        [Fact]
        public async Task Should_replace_rule_ids_on_rebuild()
        {
            var state = new HashSet<Guid>
            {
                ruleId1,
                ruleId2
            };

            await sut.RebuildAsync(state);

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(new List<Guid> { ruleId1, ruleId2 }, result);

            A.CallTo(() => grainState.WriteAsync())
                .MustHaveHappened();
        }
    }
}
