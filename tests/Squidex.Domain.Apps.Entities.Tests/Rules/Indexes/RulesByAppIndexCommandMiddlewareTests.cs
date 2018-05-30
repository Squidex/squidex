// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
    public sealed class RulesByAppIndexCommandMiddlewareTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IRulesByAppIndex index = A.Fake<IRulesByAppIndex>();
        private readonly Guid appId = Guid.NewGuid();
        private readonly Guid ruleId = Guid.NewGuid();
        private readonly RulesByAppIndexCommandMiddleware sut;

        public RulesByAppIndexCommandMiddlewareTests()
        {
            A.CallTo(() => grainFactory.GetGrain<IRulesByAppIndex>(appId, null))
                .Returns(index);

            sut = new RulesByAppIndexCommandMiddleware(grainFactory);
        }

        [Fact]
        public async Task Should_add_rule_to_index_on_create()
        {
            var context =
                new CommandContext(new CreateRule { RuleId = appId, AppId = NamedId.Of(appId, "my-app") }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.AddRuleAsync(appId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_remove_rule_from_index_on_delete()
        {
            var ruleGrain = A.Fake<IRuleGrain>();
            var ruleState = A.Fake<IRuleEntity>();

            A.CallTo(() => grainFactory.GetGrain<IRuleGrain>(appId, null))
                .Returns(ruleGrain);

            A.CallTo(() => ruleGrain.GetStateAsync())
                .Returns(J.AsTask(ruleState));

            A.CallTo(() => ruleState.AppId)
                .Returns(NamedId.Of(appId, "my-app"));

            var context =
                new CommandContext(new DeleteRule { RuleId = appId }, commandBus)
                    .Complete();

            await sut.HandleAsync(context);

            A.CallTo(() => index.RemoveRuleAsync(appId))
                .MustHaveHappened();
        }
    }
}
