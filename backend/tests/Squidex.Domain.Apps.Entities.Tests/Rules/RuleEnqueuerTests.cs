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
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleEnqueuerTests : GivenContext
{
    private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
    private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
    private readonly IRuleService ruleService = A.Fake<IRuleService>();
    private readonly IRuleUsageTracker ruleUsageTracker = A.Fake<IRuleUsageTracker>();
    private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
    private readonly RuleEnqueuer sut;

    [RuleAction]
    public sealed record TestAction : RuleAction
    {
        public Uri Url { get; set; }
    }

    public RuleEnqueuerTests()
    {
        var options = Options.Create(new RuleOptions());

        sut = new RuleEnqueuer(cache, A.Fake<ILocalCache>(),
            AppProvider,
            ruleEventRepository,
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

        var rule = CreateRule();

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(Enumerable.Repeat(new JobResult(), 1).ToAsyncEnumerable());

        await sut.EnqueueAsync(rule.Id, rule, @event);

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_enqueue_event_if_it_has_no_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateRule();

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(Enumerable.Repeat(new JobResult { Rule = rule, SkipReason = SkipReason.WrongEvent }, 1).ToAsyncEnumerable());

        await sut.EnqueueAsync(rule.Id, rule, @event);

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enqueue_event_with_successful_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateRule();

        var job = new RuleJob
        {
            AppId = AppId.Id,
            ActionData = string.Empty,
            ActionName = string.Empty,
            Created = now
        };

        RuleEventWrite[]? writes = null;

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .Invokes(x => writes = x.GetArgument<List<RuleEventWrite>>(0)?.ToArray());

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(Enumerable.Repeat(new JobResult { Job = job, Rule = rule }, 1).ToAsyncEnumerable());

        await sut.EnqueueAsync(rule.Id, rule, @event);

        Assert.Equal(new[] { new RuleEventWrite(job, job.Created) }, writes);

        A.CallTo(() => ruleUsageTracker.TrackAsync(AppId.Id, rule.Id, now.ToDateOnly(), 1, 0, 0, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_enqueue_event_with_failed_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateRule();

        var job = new RuleJob
        {
            AppId = AppId.Id,
            ActionData = string.Empty,
            ActionName = string.Empty,
            Created = now
        };

        RuleEventWrite[]? writes = null;

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .Invokes(x => writes = x.GetArgument<List<RuleEventWrite>>(0)?.ToArray());

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(Enumerable.Repeat(new JobResult { Job = job, Rule = rule, SkipReason = SkipReason.Failed }, 1).ToAsyncEnumerable());

        await sut.EnqueueAsync(rule.Id, rule, @event);

        Assert.Equal(new[] { new RuleEventWrite(job) }, writes);

        A.CallTo(() => ruleUsageTracker.TrackAsync(AppId.Id, rule.Id, now.ToDateOnly(), 1, 0, 1, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_handle_event_with_successful_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var job = new RuleJob
        {
            AppId = AppId.Id,
            ActionData = string.Empty,
            ActionName = string.Empty,
            Created = now
        };

        SetupRules(@event, job, default);

        RuleEventWrite[]? writes = null;

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .Invokes(x => writes = x.GetArgument<List<RuleEventWrite>>(0)?.ToArray());

        await sut.On(new[] { @event });

        Assert.Equal(new[] { new RuleEventWrite(job, job.Created) }, writes);

        A.CallTo(() => ruleUsageTracker.TrackAsync(AppId.Id, A<DomainId>._, now.ToDateOnly(), 1, 0, 0, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_handle_event_with_failed_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var job = new RuleJob
        {
            AppId = AppId.Id,
            ActionData = string.Empty,
            ActionName = string.Empty,
            Created = now
        };

        SetupRules(@event, job, SkipReason.Failed);

        RuleEventWrite[]? writes = null;

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .Invokes(x => writes = x.GetArgument<List<RuleEventWrite>>(0)?.ToArray());

        await sut.On(new[] { @event });

        Assert.Equal(new[] { new RuleEventWrite(job) }, writes);

        A.CallTo(() => ruleUsageTracker.TrackAsync(AppId.Id, A<DomainId>._, now.ToDateOnly(), 1, 0, 1, default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_handle_restored_event()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var job = new RuleJob
        {
            AppId = AppId.Id,
            ActionData = string.Empty,
            ActionName = string.Empty,
            Created = now
        };

        SetupRules(@event, job, default);

        await sut.On(new[] { @event.SetRestored(true) });

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_handle_events_in_batches()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var job = new RuleJob
        {
            AppId = AppId.Id,
            ActionData = string.Empty,
            ActionName = string.Empty,
            Created = now
        };

        SetupRules(@event, job, default);

        await sut.On(Enumerable.Repeat(@event, 10));

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .MustHaveHappenedOnceExactly();

        A.CallTo(ruleUsageTracker)
            .MustHaveHappenedANumberOfTimesMatching(x => x == 10);
    }

    private void SetupRules(Envelope<IEvent> @event, RuleJob job, SkipReason skipReason)
    {
        var rule = CreateRule();

        A.CallTo(() => AppProvider.GetRulesAsync(AppId.Id, A<CancellationToken>._))
            .Returns([rule]);

        A.CallTo(() => ruleService.CreateJobsAsync(@event, MatchingContext(rule), default))
            .Returns(Enumerable.Repeat(new JobResult { Job = job, Rule = rule, SkipReason = skipReason }, 1).ToAsyncEnumerable());
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
