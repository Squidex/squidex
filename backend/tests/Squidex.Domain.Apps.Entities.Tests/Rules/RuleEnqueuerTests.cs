// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Flows;
using Squidex.Flows.Internal;
using Squidex.Flows.Internal.Execution;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleEnqueuerTests : GivenContext
{
    private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
    private readonly IRuleService ruleService = A.Fake<IRuleService>();
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly IFlowManager<FlowEventContext> flowManager = A.Fake<IFlowManager<FlowEventContext>>();
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
    private readonly RuleEnqueuer sut;

    public RuleEnqueuerTests()
    {
        var options = Options.Create(new RulesOptions());

        sut = new RuleEnqueuer(cache, A.Fake<ILocalCache>(),
            AppProvider,
            flowManager,
            ruleService,
            ruleUsageTracker,
            options,
            A.Fake<ILogger<RuleEnqueuer>>());
    }

    [Fact]
    public void Should_return_wildcard_filter_for_events_filter()
    {
        Assert.Equal(default, ((IEventConsumer)sut).EventsFilter);
    }

    [Fact]
    public async Task Should_do_nothing_on_clear()
    {
        await ((IEventConsumer)sut).ClearAsync();
    }

    [Fact]
    public void Should_return_type_name_for_name()
    {
        Assert.Equal(nameof(RuleEnqueuer), ((IEventConsumer)sut).Name);
    }

    [Fact]
    public void Should_process_in_batches()
    {
        Assert.True(sut.BatchSize > 1);
    }

    [Fact]
    public async Task Should_not_enqueue_event_if_job_is_null()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), CancellationToken))
            .Returns(Enumerable.Repeat(new JobResult(), 1).ToAsyncEnumerable());

        await sut.EnqueueAsync(rule, @event, CancellationToken);

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enqueue_result_with_successful_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = rule,
        };

        var writes = Array.Empty<CreateFlowInstanceRequest<FlowEventContext>>();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .Invokes(x => writes = x.GetArgument<CreateFlowInstanceRequest<FlowEventContext>[]>(0)!);

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), CancellationToken))
            .Returns(new[] { result }.ToAsyncEnumerable());

        await sut.EnqueueAsync(rule, @event, CancellationToken);

        Assert.Equal(new[] { result.Job.Value }, writes);

        A.CallTo(() => ruleUsageTracker.TrackAsync(AppId.Id, rule.Id, now.ToDateOnly(), 1, 0, 0, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_enqueue_result_without_rule()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        var result = new JobResult
        {
            SkipReason = SkipReason.Failed,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
        };

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), CancellationToken))
            .Returns(new[] { result }.ToAsyncEnumerable());

        await sut.EnqueueAsync(rule, @event, CancellationToken);

        A.CallTo(ruleUsageTracker)
            .MustNotHaveHappened();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_enqueue_result_without_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        var result = new JobResult
        {
            SkipReason = SkipReason.Failed,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Rule = rule,
        };

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), CancellationToken))
            .Returns(new[] { result }.ToAsyncEnumerable());

        await sut.EnqueueAsync(rule, @event, CancellationToken);

        A.CallTo(ruleUsageTracker)
            .MustNotHaveHappened();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, CancellationToken))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_handle_event_with_successful_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = rule,
        };

        var writes = Array.Empty<CreateFlowInstanceRequest<FlowEventContext>>();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .Invokes(x => writes = x.GetArgument<CreateFlowInstanceRequest<FlowEventContext>[]>(0)!);

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(new[] { result }.ToAsyncEnumerable());

        await sut.On([@event]);

        Assert.Equal(new[] { result.Job.Value }, writes);

        A.CallTo(() => ruleUsageTracker.TrackAsync(AppId.Id, rule.Id, now.ToDateOnly(), 1, 0, 0, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_handle_event_with_results_in_result_without_rule()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        var result = new JobResult
        {
            SkipReason = SkipReason.Failed,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
        };

        var writes = Array.Empty<CreateFlowInstanceRequest<FlowEventContext>>();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .Invokes(x => writes = x.GetArgument<CreateFlowInstanceRequest<FlowEventContext>[]>(0)!);

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(new[] { result }.ToAsyncEnumerable());

        await sut.On([@event]);

        A.CallTo(ruleUsageTracker)
            .MustNotHaveHappened();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_handle_event_with_results_in_result_without_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        var result = new JobResult
        {
            SkipReason = SkipReason.Failed,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Rule = rule,
        };

        var writes = Array.Empty<CreateFlowInstanceRequest<FlowEventContext>>();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .Invokes(x => writes = x.GetArgument<CreateFlowInstanceRequest<FlowEventContext>[]>(0)!);

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(new[] { result }.ToAsyncEnumerable());

        await sut.On([@event]);

        A.CallTo(ruleUsageTracker)
            .MustNotHaveHappened();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_handle_restored_event()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        await sut.On([@event.SetRestored(true)]);

        A.CallTo(ruleUsageTracker)
            .MustNotHaveHappened();

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_handle_apps_without_rules()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        await sut.On([@event]);

        A.CallTo(ruleService)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_handle_events_in_batches()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateAndSetupRule();

        var result = new JobResult
        {
            SkipReason = SkipReason.None,
            EnrichedEvent = new EnrichedManualEvent(),
            EnrichmentError = null,
            Job = new CreateFlowInstanceRequest<FlowEventContext>
            {
                Context = new FlowEventContext(),
                Definition = new FlowDefinition(),
                DefinitionId = Guid.NewGuid().ToString(),
                OwnerId = Guid.NewGuid().ToString(),
            },
            Rule = rule,
        };

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(new[] { result }.ToAsyncEnumerable());

        await sut.On(Enumerable.Repeat(@event, 10));

        A.CallTo(() => flowManager.EnqueueAsync(A<CreateFlowInstanceRequest<FlowEventContext>[]>._, default))
            .MustHaveHappenedOnceExactly();

        A.CallTo(ruleUsageTracker)
            .MustHaveHappenedANumberOfTimesMatching(x => x == 10);
    }

    [Fact]
    public async Task Should_handle_events_of_multiple_apps_with_the_rules_of_each_app()
    {
        var rule1 = CreateAndSetupRule();

        var otherAppId = NamedId.Of(DomainId.NewGuid(), "other-app");
        var rule2 = CreateRule();

        A.CallTo(() => AppProvider.GetRulesAsync(otherAppId.Id, A<CancellationToken>._))
            .Returns([rule2]);

        var event1 = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });
        var event2 = Envelope.Create<IEvent>(new ContentCreated { AppId = otherAppId });
        var event3 = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });
        var event4 = Envelope.Create<IEvent>(new ContentCreated { AppId = otherAppId });

        var contexts = new List<(Envelope<IEvent> Event, RulesContext Context)>();

        A.CallTo(() => ruleService.CreateJobsAsync(A<Envelope<IEvent>>._, A<RulesContext>._, default))
            .Invokes((Envelope<IEvent> source, RulesContext context, CancellationToken _) =>
            {
                contexts.Add((source, context));
            })
            .Returns(Array.Empty<JobResult>().ToAsyncEnumerable());

        await sut.On([event1, event2, event3, event4]);

        // Every event must be handled, even though they are grouped by app.
        Assert.Equal([event1, event3, event2, event4], contexts.Select(x => x.Event).ToArray());

        // Every event must be handled with the rules of its own app.
        Assert.All(contexts.Where(x => x.Event == event1 || x.Event == event3), x =>
        {
            Assert.Equal(AppId, x.Context.AppId);
            Assert.Equal([rule1], x.Context.Rules.Values.ToArray());
        });

        Assert.All(contexts.Where(x => x.Event == event2 || x.Event == event4), x =>
        {
            Assert.Equal(otherAppId, x.Context.AppId);
            Assert.Equal([rule2], x.Context.Rules.Values.ToArray());
        });

        // The rules must only be indexed once per app, not once per event.
        Assert.Same(contexts[0].Context.Rules, contexts[1].Context.Rules);
        Assert.Same(contexts[2].Context.Rules, contexts[3].Context.Rules);
    }

    private static RulesContext MatchingContext(Rule rule)
    {
        // These two properties must not be set to true for performance reasons.
        return A<RulesContext>.That.Matches(x =>
            x.Rules.Values.Contains(rule) &&
           !x.IncludeSkipped &&
           !x.IncludeStale);
    }
}
