// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json.Linq;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure.Log;
using Xunit;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDequeuerTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly ISemanticLog log = A.Dummy<ISemanticLog>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
        private readonly RuleService ruleService = A.Fake<RuleService>();
        private readonly RuleDequeuerGrain sut;

        public RuleDequeuerTests()
        {
            A.CallTo(() => clock.GetCurrentInstant()).Returns(now);

            sut = new RuleDequeuerGrain(
                ruleService,
                ruleEventRepository,
                log,
                clock);
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
            var actionData = new JObject();
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

            await sut.HandleAsync(@event);

            A.CallTo(() => ruleEventRepository.MarkSentAsync(@event.Id, requestDump, result, jobResult, requestElapsed, nextCall))
                .MustHaveHappened();
        }

        private IRuleEventEntity CreateEvent(int numCalls, string actionName, JObject actionData)
        {
            var @event = A.Fake<IRuleEventEntity>();

            var job = new RuleJob
            {
                JobId = Guid.NewGuid(),
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
