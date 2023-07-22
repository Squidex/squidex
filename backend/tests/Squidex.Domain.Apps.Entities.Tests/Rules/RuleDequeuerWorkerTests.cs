// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using NodaTime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleDequeuerWorkerTests
{
    private readonly IClock clock = A.Fake<IClock>();
    private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
    private readonly IRuleService ruleService = A.Fake<IRuleService>();
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly ILogger<RuleDequeuerWorker> log = A.Dummy<ILogger<RuleDequeuerWorker>>();
    private readonly RuleDequeuerWorker sut;

    public RuleDequeuerWorkerTests()
    {
        A.CallTo(() => clock.GetCurrentInstant())
            .Returns(SystemClock.Instance.GetCurrentInstant().WithoutMs());

        sut = new RuleDequeuerWorker(ruleService, ruleUsageTracker, ruleEventRepository, log)
        {
            Clock = clock
        };
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

        A.CallTo(log).Where(x => x.Method.Name == "Log")
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_ignore_rule_service_exceptions_and_log()
    {
        var @event = CreateEvent(1, "MyAction", "{}");

        A.CallTo(() => ruleService.InvokeAsync(A<string>._, A<string>._, default))
            .Throws(new InvalidOperationException());

        await sut.HandleAsync(@event, default);

        A.CallTo(log).Where(x => x.Method.Name == "Log")
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
            sut.HandleAsync(event1, default),
            sut.HandleAsync(event2, default));

        A.CallTo(() => ruleService.InvokeAsync(A<string>._, A<string>._, default))
            .MustHaveHappenedOnceExactly();
    }

    [Theory]
    [InlineData(0, 0, RuleResult.Success, RuleJobResult.Success)]
    [InlineData(0, 5, RuleResult.Timeout, RuleJobResult.Retry)]
    [InlineData(1, 60, RuleResult.Timeout, RuleJobResult.Retry)]
    [InlineData(2, 360, RuleResult.Failed, RuleJobResult.Retry)]
    [InlineData(3, 720, RuleResult.Failed, RuleJobResult.Retry)]
    [InlineData(4, 0, RuleResult.Failed, RuleJobResult.Failed)]
    public async Task Should_set_next_attempt_based_on_num_calls(int calls, int minutes, RuleResult actual, RuleJobResult jobResult)
    {
        var actionData = "{}";
        var actionName = "MyAction";

        var @event = CreateEvent(calls, actionName, actionData);

        var requestElapsed = TimeSpan.FromMinutes(1);
        var requestDump = "Dump";

        A.CallTo(() => ruleService.InvokeAsync(@event.Job.ActionName, @event.Job.ActionData, default))
            .Returns((Result.Create(requestDump, actual), requestElapsed));

        var now = clock.GetCurrentInstant();

        await sut.HandleAsync(@event, default);

        if (actual == RuleResult.Failed)
        {
            A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Warning)
                .MustHaveHappened();

            A.CallTo(() => ruleUsageTracker.TrackAsync(@event.Job.AppId, @event.Job.RuleId, now.ToDateOnly(), 0, 0, 1, A<CancellationToken>._))
                .MustHaveHappened();
        }
        else
        {
            A.CallTo(log).Where(x => x.Method.Name == "Log" && x.GetArgument<LogLevel>(0) == LogLevel.Warning)
                .MustNotHaveHappened();

            A.CallTo(() => ruleUsageTracker.TrackAsync(@event.Job.AppId, @event.Job.RuleId, now.ToDateOnly(), 0, 1, 0, A<CancellationToken>._))
                .MustHaveHappened();
        }

        var nextCall = minutes > 0 ? now.Plus(Duration.FromMinutes(minutes)) : (Instant?)null;

        A.CallTo(() => ruleEventRepository.UpdateAsync(@event.Job,
                A<RuleJobUpdate>.That.Matches(x =>
                    x.Elapsed == requestElapsed &&
                    x.ExecutionDump == requestDump &&
                    x.ExecutionResult == actual &&
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
            AppId = DomainId.NewGuid(),
            ActionData = actionData,
            ActionName = actionName,
            Created = clock.GetCurrentInstant(),
            RuleId = DomainId.NewGuid()
        };

        A.CallTo(() => @event.Id).Returns(id);
        A.CallTo(() => @event.Job).Returns(job);
        A.CallTo(() => @event.Created).Returns(clock.GetCurrentInstant());
        A.CallTo(() => @event.NumCalls).Returns(numCalls);

        return @event;
    }
}
