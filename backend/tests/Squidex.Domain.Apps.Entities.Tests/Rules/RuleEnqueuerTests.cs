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
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public class RuleEnqueuerTests : GivenContext
{
    private readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
    private readonly ILocalCache localCache = A.Fake<ILocalCache>();
    private readonly IRuleEventRepository ruleEventRepository = A.Fake<IRuleEventRepository>();
    private readonly IRuleService ruleService = A.Fake<IRuleService>();
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

        sut = new RuleEnqueuer(cache, localCache,
            AppProvider,
            ruleEventRepository,
            ruleService,
            options,
            A.Fake<ILogger<RuleEnqueuer>>());
    }

    [Fact]
    public void Should_return_wildcard_filter_for_events_filter()
    {
        IEventConsumer consumer = sut;

        Assert.Equal(".*", consumer.EventsFilter);
    }

    [Fact]
    public async Task Should_do_nothing_on_clear()
    {
        IEventConsumer consumer = sut;

        await consumer.ClearAsync();
    }

    [Fact]
    public void Should_return_type_name_for_name()
    {
        IEventConsumer consumer = sut;

        Assert.Equal(nameof(RuleEnqueuer), consumer.Name);
    }

    [Fact]
    public async Task Should_not_insert_event_if_job_is_null()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateRule();

        A.CallTo(() => ruleService.CreateJobsAsync(A<JobCallback>._, @event, MatchingContext(rule), default))
            .Invokes(x =>
            {
                var result = new JobResult();

                x.GetArgument<JobCallback>(0)!(rule.Id, rule.RuleDef, result, default).AsTask().Forget();
            });

        await sut.EnqueueAsync(rule.Id, rule.RuleDef, @event);

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_insert_event_if_job_has_a_skip_reason()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateRule();

        var job = new RuleJob
        {
            Created = now
        };

        A.CallTo(() => ruleService.CreateJobsAsync(A<JobCallback>._, @event, MatchingContext(rule), default))
            .Invokes(x =>
            {
                var result = new JobResult { Job = job, SkipReason = SkipReason.WrongEvent };

                x.GetArgument<JobCallback>(0)!(rule.Id, rule.RuleDef, result, default).AsTask().Forget();
            });

        await sut.EnqueueAsync(rule.Id, rule.RuleDef, @event);

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_insert_insert_from_successful_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateRule();

        var job = new RuleJob
        {
            Created = now
        };

        A.CallTo(() => ruleService.CreateJobsAsync(A<JobCallback>._, @event, MatchingContext(rule), default))
            .Invokes(x =>
            {
                var result = new JobResult { Job = job };

                x.GetArgument<JobCallback>(0)!(rule.Id, rule.RuleDef, result, default).AsTask().Forget();
            });

        await sut.EnqueueAsync(rule.Id, rule.RuleDef, @event);

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>.That.Matches(x => x.Contains(new RuleEventWrite(job, now, null))), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_insert_insert_from_failed_job()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var rule = CreateRule();

        var job = new RuleJob
        {
            Created = now
        };

        A.CallTo(() => ruleService.CreateJobsAsync(A<JobCallback>._, @event, MatchingContext(rule), default))
            .Invokes(x =>
            {
                var result = new JobResult { Job = job, SkipReason = SkipReason.Failed };

                x.GetArgument<JobCallback>(0)!(rule.Id, rule.RuleDef, result, default).AsTask().Forget();
            });

        await sut.EnqueueAsync(rule.Id, rule.RuleDef, @event);

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>.That.Matches(x => x.Contains(new RuleEventWrite(job, null, null))), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_insert_insert_from_successful_in_event_consumer()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var job = new RuleJob
        {
            Created = now
        };

        SetupRules(@event, job, default);

        await sut.On(new[] { @event });

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>.That.Matches(x => x.Contains(new RuleEventWrite(job, now, null))), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_insert_insert_from_failed_job_in_event_consumer()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var job = new RuleJob
        {
            Created = now
        };

        SetupRules(@event, job, SkipReason.Failed);

        await sut.On(new[] { @event });

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>.That.Matches(x => x.Contains(new RuleEventWrite(job, null, null))), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_not_insert_event_in_event_consumer_if_restored()
    {
        var @event = Envelope.Create<IEvent>(new ContentCreated { AppId = AppId });

        var job = new RuleJob
        {
            Created = now
        };

        SetupRules(@event, job, default);

        await sut.On(new[] { @event.SetRestored(true) });

        A.CallTo(() => ruleEventRepository.EnqueueAsync(A<List<RuleEventWrite>>._, default))
            .MustNotHaveHappened();
    }

    private void SetupRules(Envelope<IEvent> @event, RuleJob job, SkipReason skipReason)
    {
        var rule = CreateRule();

        A.CallTo(() => AppProvider.GetRulesAsync(AppId.Id, A<CancellationToken>._))
            .Returns(new List<IRuleEntity> { rule });

        A.CallTo(() => ruleService.CreateJobsAsync(A<JobCallback>._, @event, MatchingContext(rule), default))
            .Invokes(x =>
            {
                var result = new JobResult { Job = job, SkipReason = skipReason };

                x.GetArgument<JobCallback>(0)!(rule.Id, rule.RuleDef, result, default).AsTask().Forget();
            });
    }

    private static RuleEntity CreateRule()
    {
        var rule = new Rule(new ContentChangedTriggerV2(), new TestAction { Url = new Uri("https://squidex.io") });

        return new RuleEntity { RuleDef = rule, Id = DomainId.NewGuid() };
    }

    private static RulesContext MatchingContext(RuleEntity rule)
    {
        // These two properties must not be set to true for performance reasons.
        return A<RulesContext>.That.Matches(x =>
            x.Rules.Values.Contains(rule.RuleDef) &&
           !x.IncludeSkipped &&
           !x.IncludeStale);
    }
}
