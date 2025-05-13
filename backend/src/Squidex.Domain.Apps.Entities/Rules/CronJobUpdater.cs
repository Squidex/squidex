// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.Triggers;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Events;
using Squidex.Flows.CronJobs;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Rules;

public sealed class CronJobUpdater(
    IAppProvider appProvider,
    ICronJobManager<CronJobContext> cronJobs,
    IRuleEnqueuer ruleEnqueuer)
    : IEventConsumer, IInitializable
{
    public StreamFilter EventsFilter => StreamFilter.Prefix("rule-");

    public Task InitializeAsync(
        CancellationToken ct)
    {
        cronJobs.Subscribe(HandleCronJobAsync);
        return Task.CompletedTask;
    }

    public async Task HandleCronJobAsync(CronJob<CronJobContext> job,
        CancellationToken ct)
    {
        var context = job.Context;

        var rule = await appProvider.GetRuleAsync(context.AppId.Id, context.RuleId, ct);
        if (rule == null || rule.Trigger is not CronJobTrigger cronJob)
        {
            return;
        }

        var @event =
            Envelope.Create(
                new RuleCronJobTriggered { AppId = context.AppId, RuleId = context.RuleId, Value = cronJob.Value });

        await ruleEnqueuer.EnqueueAsync(context.RuleId, rule, @event, ct);
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (@event.Payload is RuleCreated ruleCreated)
        {
            if (ruleCreated.Trigger is CronJobTrigger cronJob)
            {
                await AddCronJobAsync(ruleCreated.AppId, ruleCreated.RuleId, cronJob, default);
            }
        }
        else if (@event.Payload is RuleUpdated ruleUpdated && ruleUpdated.Trigger != null)
        {
            if (ruleUpdated.Trigger is CronJobTrigger cronJob)
            {
                await AddCronJobAsync(ruleUpdated.AppId, ruleUpdated.RuleId, cronJob, default);
            }
            else
            {
                await cronJobs.RemoveAsync(ruleUpdated.RuleId.ToString());
            }
        }
        else if (@event.Payload is RuleDeleted ruleDeleted)
        {
            await cronJobs.RemoveAsync(ruleDeleted.RuleId.ToString());
        }
    }

    private async Task AddCronJobAsync(NamedId<DomainId> appId, DomainId id, CronJobTrigger trigger,
        CancellationToken ct)
    {
        await cronJobs.AddAsync(new CronJob<CronJobContext>
        {
            Id = id.ToString(),
            Context = new CronJobContext { AppId = appId, RuleId = id },
            CronExpression = trigger.CronExpression,
            CronTimezone = trigger.CronTimezone,
        }, ct);
    }
}
