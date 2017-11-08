// ==========================================================================
//  RuleDequeuerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Read.Rules.Repositories;
using Squidex.Infrastructure.Log;
using Xunit;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void

namespace Squidex.Domain.Apps.Read.Rules
{
    public class RuleDequeuerTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly IRuleRepository ruleRepository = A.Fake<IRuleRepository>();
        private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
        private readonly RuleService ruleService = A.Fake<RuleService>();
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();

        public RuleDequeuerTests()
        {
            A.CallTo(() => clock.GetCurrentInstant()).Returns(now);
        }

        [Theory]
        [InlineData(0, 0,   RuleResult.Success, RuleJobResult.Success)]
        [InlineData(0, 5,   RuleResult.Timeout, RuleJobResult.Retry)]
        [InlineData(1, 60,  RuleResult.Timeout, RuleJobResult.Retry)]
        [InlineData(2, 360, RuleResult.Failed,  RuleJobResult.Retry)]
        [InlineData(3, 720, RuleResult.Failed,  RuleJobResult.Retry)]
        [InlineData(4, 0,   RuleResult.Failed,  RuleJobResult.Failed)]
        public void Should_set_next_attempt_based_on_num_calls(int calls, int minutes, RuleResult result, RuleJobResult jobResult)
        {
            var actionData = new RuleJobData();
            var actionName = "MyAction";

            var @event = CreateEvent(calls, actionName, actionData);

            var requestElapsed = TimeSpan.FromMinutes(1);
            var requestDump = "Dump";

            SetupSender(@event, requestDump, result, requestElapsed);
            SetupPendingEvents(@event);

            var sut = new RuleDequeuer(
                ruleService,
                ruleEventRepository,
                log,
                clock);

            sut.Next();
            sut.Dispose();

            Instant? nextCall = null;

            if (minutes > 0)
            {
                nextCall = now.Plus(Duration.FromMinutes(minutes));
            }

            VerifyRepositories(@event, requestDump, result, jobResult, requestElapsed, nextCall);
        }

        private void SetupSender(IRuleEventEntity @event, string requestDump, RuleResult requestResult, TimeSpan requestTime)
        {
            A.CallTo(() => ruleService.InvokeAsync(@event.Job.ActionName, @event.Job.ActionData))
                .Returns((requestDump, requestResult, requestTime));
        }

        private void SetupPendingEvents(IRuleEventEntity @event)
        {
            A.CallTo(() => ruleEventRepository.QueryPendingAsync(
                now,
                A<Func<IRuleEventEntity, Task>>.Ignored,
                A<CancellationToken>.Ignored))
                .Invokes(async (Instant n, Func<IRuleEventEntity, Task> callback, CancellationToken ct) =>
                {
                    await callback(@event);
                });
        }

        private void VerifyRepositories(IRuleEventEntity @event, string dump, RuleResult result, RuleJobResult jobResult, TimeSpan elapsed, Instant? nextCall)
        {
            A.CallTo(() => ruleEventRepository.MarkSendingAsync(@event.Id))
                .MustHaveHappened();

            A.CallTo(() => ruleEventRepository.MarkSentAsync(@event.Id, dump, result, jobResult, elapsed, nextCall))
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
