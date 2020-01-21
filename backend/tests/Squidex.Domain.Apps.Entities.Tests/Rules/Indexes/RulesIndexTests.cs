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
using Orleans;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules.Indexes
{
    public class RulesIndexTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IRulesByAppIndexGrain index = A.Fake<IRulesByAppIndexGrain>();
        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
        private readonly RulesIndex sut;

        public RulesIndexTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IRulesByAppIndexGrain>(appId.Id, null))
                .Returns(index);

            sut = new RulesIndex(grainFactory);
        }

        [Fact]
        public async Task Should_resolve_rules_by_id()
        {
            var rule = SetupRule(0, false);

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<Guid> { rule.Id });

            var actual = await sut.GetRulesAsync(appId.Id);

            Assert.Same(actual[0], rule);
        }

        [Fact]
        public async Task Should_return_empty_rule_if_rule_not_created()
        {
            var rule = SetupRule(-1, false);

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<Guid> { rule.Id });

            var actual = await sut.GetRulesAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_return_empty_rule_if_rule_deleted()
        {
            var rule = SetupRule(-1, false);

            A.CallTo(() => index.GetIdsAsync())
                .Returns(new List<Guid> { rule.Id });

            var actual = await sut.GetRulesAsync(appId.Id);

            Assert.Empty(actual);
        }

        [Fact]
        public async Task Should_add_rule_to_index_on_create()
        {
            var ruleId = Guid.NewGuid();

            var command = new CreateRule { RuleId = ruleId, AppId = appId };

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddAsync(ruleId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_rule_from_index_on_delete()
        {
            var rule = SetupRule(0, false);

            var command = new DeleteRule { RuleId = rule.Id };

            var context =
                new CommandContext(command, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveAsync(rule.Id))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_forward_call_when_rebuilding()
        {
            var rules = new HashSet<Guid>();

            await sut.RebuildAsync(appId.Id, rules);

            A.CallTo(() => index.RebuildAsync(rules))
                .MustHaveHappened();
        }

        private IRuleEntity SetupRule(long version, bool deleted)
        {
            var ruleId = Guid.NewGuid();

            var ruleEntity = new RuleEntity { Id = ruleId, AppId = appId, Version = version, IsDeleted = deleted };
            var ruleGrain = A.Fake<IRuleGrain>();

            A.CallTo(() => ruleGrain.GetStateAsync())
                .Returns(J.Of<IRuleEntity>(ruleEntity));

            A.CallTo(() => grainFactory.GetGrain<IRuleGrain>(ruleId, null))
                .Returns(ruleGrain);

            return ruleEntity;
        }
    }
}
