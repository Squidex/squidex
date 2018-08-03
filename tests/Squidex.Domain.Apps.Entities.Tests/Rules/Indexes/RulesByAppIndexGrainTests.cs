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
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public class RulesByAppIndexGrainTests
    {
        private readonly IStore<Guid> store = A.Fake<IStore<Guid>>();
        private readonly IPersistence<RulesByAppIndexGrain.State> persistence = A.Fake<IPersistence<RulesByAppIndexGrain.State>>();
        private readonly Guid ruleId1 = Guid.NewGuid();
        private readonly Guid ruleId2 = Guid.NewGuid();
        private readonly RulesByAppIndexGrain sut;

        public RulesByAppIndexGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(A<Type>.Ignored, A<Guid>.Ignored, A<Func<RulesByAppIndexGrain.State, Task>>.Ignored))
                .Returns(persistence);

            sut = new RulesByAppIndexGrain(store);
            sut.OnActivateAsync(Guid.NewGuid()).Wait();
        }

        [Fact]
        public async Task Should_add_rule_id_to_index()
        {
            await sut.AddRuleAsync(ruleId1);
            await sut.AddRuleAsync(ruleId2);

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(new List<Guid> { ruleId1, ruleId2 }, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<RulesByAppIndexGrain.State>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_delete_and_reset_state_when_cleaning()
        {
            await sut.AddRuleAsync(ruleId1);
            await sut.AddRuleAsync(ruleId2);
            await sut.ClearAsync();

            var ids = await sut.GetRuleIdsAsync();

            Assert.Empty(ids);

            A.CallTo(() => persistence.DeleteAsync())
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_rule_id_from_index()
        {
            await sut.AddRuleAsync(ruleId1);
            await sut.AddRuleAsync(ruleId2);
            await sut.RemoveRuleAsync(ruleId1);

            var result = await sut.GetRuleIdsAsync();

            Assert.Equal(new List<Guid> { ruleId2 }, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<RulesByAppIndexGrain.State>.Ignored))
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

            A.CallTo(() => persistence.WriteSnapshotAsync(A<RulesByAppIndexGrain.State>.Ignored))
                .MustHaveHappened();
        }
    }
}
