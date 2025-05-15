// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Events;
using Squidex.Flows.CronJobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class CronJobUpdaterTests : GivenContext
{
    private readonly ICronJobManager<CronJobContext> cronJobs = A.Fake<ICronJobManager<CronJobContext>>();
    private readonly IRuleEnqueuer ruleEnqueuer = A.Fake<IRuleEnqueuer>();
    private readonly CronJobUpdater sut;

    public CronJobUpdaterTests()
    {
        sut = new CronJobUpdater(AppProvider, cronJobs, ruleEnqueuer);
    }

    [Fact]
    public void Should_return_rules_filter_for_events_filter()
    {
        Assert.Equal(StreamFilter.Prefix("rule-"), sut.EventsFilter);
    }

    [Fact]
    public async Task Should_register_handler_when_initialized()
    {
        await sut.InitializeAsync(default);

        A.CallTo(() => cronJobs.Subscribe(A<Func<CronJob<CronJobContext>, CancellationToken, Task>>._))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_register_new_cron_job()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleCreated
                {
                    AppId = AppId,
                    Trigger = new CronJobTrigger
                    {
                        CronExpression = "* */5 * * *",
                        CronTimezone = "Europe/Berlin",
                    },
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.AddAsync(
                A<CronJob<CronJobContext>>.That.Matches(x =>
                    x.Id == ruleId.ToString() &&
                    x.Context.RuleId == ruleId &&
                    x.Context.AppId == AppId &&
                    x.CronExpression == "* */5 * * *" &&
                    x.CronTimezone == "Europe/Berlin"),
                default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_register_updated_cron_job()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleUpdated
                {
                    AppId = AppId,
                    Trigger = new CronJobTrigger
                    {
                        CronExpression = "* */5 * * *",
                        CronTimezone = "Europe/Berlin",
                    },
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.AddAsync(
                A<CronJob<CronJobContext>>.That.Matches(x =>
                    x.Id == ruleId.ToString() &&
                    x.Context.RuleId == ruleId &&
                    x.Context.AppId == AppId &&
                    x.CronExpression == "* */5 * * *" &&
                    x.CronTimezone == "Europe/Berlin"),
                default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_unregister_when_trigger_changed()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleUpdated
                {
                    AppId = AppId,
                    Trigger = new ManualTrigger
                    {
                    },
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.RemoveAsync(ruleId.ToString(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_unregister_cron_job_when_rule_deleted()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleDeleted
                {
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(() => cronJobs.RemoveAsync(ruleId.ToString(), default))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_do_nothing_when_trigger_not_changed()
    {
        var ruleId = DomainId.NewGuid();

        var @event =
            Envelope.Create(
                new RuleUpdated
                {
                    RuleId = ruleId,
                });

        await sut.On(@event);

        A.CallTo(cronJobs)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_call_rule_enqueue_when_cron_job_due()
    {
        var rule = CreateAndSetupRule(new CronJobTrigger());

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sut.HandleCronJobAsync(job, CancellationToken);

        A.CallTo(() => ruleEnqueuer.EnqueueAsync(rule, A<Envelope<IEvent>>._, CancellationToken))
            .MustHaveHappened();
    }

    [Fact]
    public async Task Should_now_call_rule_enqueue_when_rule_has_no_cron_job_anymore()
    {
        var rule = CreateAndSetupRule();

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sut.HandleCronJobAsync(job, CancellationToken);

        A.CallTo(ruleEnqueuer)
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_now_call_rule_enqueue_when_rule_is_not_found()
    {
        var rule = CreateAndSetupRule();

        var job = new CronJob<CronJobContext>
        {
            Id = rule.Id.ToString(),
            CronExpression = "* */5 * * *",
            CronTimezone = "Europe/Berlin",
            Context = new CronJobContext(AppId, rule.Id),
        };

        await sut.HandleCronJobAsync(job, CancellationToken);

        A.CallTo(ruleEnqueuer)
            .MustNotHaveHappened();
    }
}
