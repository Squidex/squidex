// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public class RuleDequeuerGrainTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly ISemanticLog log = A.Dummy<ISemanticLog>();
        private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
        private readonly IRuleService ruleService = A.Fake<IRuleService>();
        private readonly RuleDequeuerGrain sut;

        public RuleDequeuerGrainTests()
        {
            A.CallTo(() => clock.GetCurrentInstant())
                .Returns(SystemClock.Instance.GetCurrentInstant().WithoutMs());

            sut = new RuleDequeuerGrain(ruleService, ruleEventRepository, log, clock);
        }

        [Fact]
        public async Task Should_query_repository()
        {
            await sut.QueryAsync();

            A.CallTo(() => ruleEventRepository.QueryPendingAsync(A<Instant>._, A<Func<IRuleEventEntity, Task>>._, default))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_repository_exceptions_and_log()
        {
            A.CallTo(() => ruleEventRepository.QueryPendingAsync(A<Instant>._, A<Func<IRuleEventEntity, Task>>._, default))
                .Throws(new InvalidOperationException());

            await sut.QueryAsync();

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_ignore_rule_service_exceptions_and_log()
        {
            var @event = CreateEvent(1, "MyAction", "{}");

            A.CallTo(() => ruleService.InvokeAsync(A<string>._, A<string>._, default))
                .Throws(new InvalidOperationException());

            await sut.HandleAsync(@event);

            A.CallTo(() => log.Log(A<SemanticLogLevel>._, A<Exception?>._, A<LogFormatter>._!))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_execute_if_already_running()
        {
            var id = DomainId.NewGuid();

            var event1 = CreateEvent(1, "MyAction", "{}", id);
            var event2 = CreateEvent(1, "MyAction", "{}", id);

            A.CallTo(() => ruleService.InvokeAsync(A<string>._, A<string>._, default))
                .ReturnsLazily(async () =>
                {
                    await Task.Delay(500);

                    return (Result.Ignored(), TimeSpan.Zero);
                });

            await Task.WhenAll(
                sut.HandleAsync(event1),
                sut.HandleAsync(event2));

            A.CallTo(() => ruleService.InvokeAsync(A<string>._, A<string>._, default))
                .MustHaveHappenedOnceExactly();
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
            var actionData = "{}";
            var actionName = "MyAction";

            var @event = CreateEvent(calls, actionName, actionData);

            var requestElapsed = TimeSpan.FromMinutes(1);
            var requestDump = "Dump";

            A.CallTo(() => ruleService.InvokeAsync(@event.Job.ActionName, @event.Job.ActionData, default))
                .Returns((Result.Create(requestDump, result), requestElapsed));

            var now = clock.GetCurrentInstant();

            Instant? nextCall = null;

            if (minutes > 0)
            {
                nextCall = now.Plus(Duration.FromMinutes(minutes));
            }

            await sut.HandleAsync(@event);

            if (result == RuleResult.Failed)
            {
                A.CallTo(() => log.Log(SemanticLogLevel.Warning, A<Exception?>._, A<LogFormatter>._))
                    .MustHaveHappened();
            }
            else
            {
                A.CallTo(() => log.Log(SemanticLogLevel.Warning, A<Exception?>._, A<LogFormatter>._))
                    .MustNotHaveHappened();
            }

            A.CallTo(() => ruleEventRepository.UpdateAsync(@event.Job,
                    A<RuleJobUpdate>.That.Matches(x =>
                        x.Elapsed == requestElapsed &&
                        x.ExecutionDump == requestDump &&
                        x.ExecutionResult == result &&
                        x.Finished == now &&
                        x.JobNext == nextCall &&
                        x.JobResult == jobResult),
                    A<CancellationToken>._))
                .MustHaveHappened();
        }

        private IRuleEventEntity CreateEvent(int numCalls, string actionName, string actionData)
        {
            return CreateEvent(numCalls, actionName, actionData, DomainId.NewGuid());
        }

        private IRuleEventEntity CreateEvent(int numCalls, string actionName, string actionData, DomainId id)
        {
            var @event = A.Fake<IRuleEventEntity>();

            var job = new RuleJob
            {
                Id = id,
                ActionData = actionData,
                ActionName = actionName,
                Created = clock.GetCurrentInstant()
            };

            A.CallTo(() => @event.Id).Returns(id);
            A.CallTo(() => @event.Job).Returns(job);
            A.CallTo(() => @event.Created).Returns(clock.GetCurrentInstant());
            A.CallTo(() => @event.NumCalls).Returns(numCalls);

            return @event;
        }
    }
}
