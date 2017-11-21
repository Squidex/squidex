// ==========================================================================
//  RuleDequeuerGrainTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Rules.Orleans.Grains;
using Squidex.Domain.Apps.Read.Rules.Orleans.Grains.Implementation;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure.Log;
using Xunit;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

namespace Squidex.Domain.Apps.Read.Rules
{
    public class RuleDequeuerGrainTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
        private readonly RuleService ruleService = A.Fake<RuleService>();
        private readonly MyRuleDequeuerGrain sut;
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();

        public sealed class MyRuleDequeuerGrain : RuleDequeuerGrain
        {
            public MyRuleDequeuerGrain(RuleService ruleService, IRuleEventRepository ruleEventRepository, ISemanticLog log, IClock clock,
                IGrainIdentity identity,
                IGrainRuntime runtime)
                : base(ruleService, ruleEventRepository, log, clock, identity, runtime)
            {
            }

            protected override IRuleDequeuerGrain GetSelf()
            {
                return this;
            }
        }

        public RuleDequeuerGrainTests()
        {
            A.CallTo(() => clock.GetCurrentInstant()).Returns(now);

            sut = new MyRuleDequeuerGrain(
                ruleService,
                ruleEventRepository,
                log,
                clock,
                A.Fake<IGrainIdentity>(),
                A.Fake<IGrainRuntime>());
        }

        [Theory]
        [InlineData(0, 0,   RuleResult.Success, RuleJobResult.Success)]
        [InlineData(0, 5,   RuleResult.Timeout, RuleJobResult.Retry)]
        [InlineData(1, 60,  RuleResult.Timeout, RuleJobResult.Retry)]
        [InlineData(2, 360, RuleResult.Failed,  RuleJobResult.Retry)]
        [InlineData(3, 720, RuleResult.Failed,  RuleJobResult.Retry)]
        [InlineData(4, 0,   RuleResult.Failed,  RuleJobResult.Failed)]
        public async Task Should_set_next_attempt_based_on_num_calls(int calls, int minutes, RuleResult result, RuleJobResult jobResult)
        {
            var actionData = new RuleJobData();
            var actionName = "MyAction";

            var @event = CreateEvent(calls, actionName, actionData);

            var requestElapsed = TimeSpan.FromMinutes(1);
            var requestDump = "Dump";

            A.CallTo(() => ruleService.InvokeAsync(@event.Job.ActionName, @event.Job.ActionData))
                .Returns((requestDump, result, requestElapsed));

            Instant? nextCall = null;

            if (minutes > 0)
            {
                nextCall = now.Plus(Duration.FromMinutes(minutes));
            }

            await sut.OnActivateAsync();
            await sut.HandleAsync(@event.AsImmutable());
            await sut.OnDeactivateAsync();

            A.CallTo(() => ruleEventRepository.MarkSentAsync(@event.Id, requestDump, result, jobResult, requestElapsed, nextCall))
                .MustHaveHappened();
        }

        private IRuleEventEntity CreateEvent(int numCalls, string actionName, RuleJobData actionData)
        {
            var @event = A.Fake<IRuleEventEntity>();

            var job = new RuleJob
            {
                RuleId = Guid.NewGuid(),
                ActionData = actionData,
                ActionName = actionName,
                Created = now
            };

            A.CallTo(() => @event.Id).Returns(Guid.NewGuid());
            A.CallTo(() => @event.Job).Returns(job);
            A.CallTo(() => @event.Created).Returns(now);
            A.CallTo(() => @event.NumCalls).Returns(numCalls);

            return @event;
        }
    }
}
