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
        var (appId, ruleId) = job.Context;

        var rule = await appProvider.GetRuleAsync(appId.Id, ruleId, ct);

        // The rule might have been updated or deleted in the meantime, but we are running asynchronously.
        if (rule == null || rule.Trigger is not CronJobTrigger cronJob)
        {
            return;
        }

        // The rule enqueue needs an event.
        var @event = new RuleCronJobTriggered { AppId = appId, RuleId = ruleId, Value = cronJob.Value };

        await ruleEnqueuer.EnqueueAsync(rule, Envelope.Create(@event), ct);
    }

    public async Task On(Envelope<IEvent> @event)
    {
        if (@event.Payload is RuleCreated created)
        {
            if (created.Trigger is CronJobTrigger cronJob)
            {
                await AddCronJobAsync(created.AppId, created.RuleId, cronJob, default);
            }
        }
        else if (@event.Payload is RuleUpdated updated && updated.Trigger != null)
        {
            if (updated.Trigger is CronJobTrigger cronJob)
            {
                await AddCronJobAsync(updated.AppId, updated.RuleId, cronJob, default);
            }
            else
            {
                await cronJobs.RemoveAsync(updated.RuleId.ToString());
            }
        }
        else if (@event.Payload is RuleDeleted deleted)
        {
            await cronJobs.RemoveAsync(deleted.RuleId.ToString());
        }
    }

    private async Task AddCronJobAsync(NamedId<DomainId> appId, DomainId id, CronJobTrigger trigger,
        CancellationToken ct)
    {
        await cronJobs.AddAsync(new CronJob<CronJobContext>
        {
            Id = id.ToString(),
            CronExpression = trigger.CronExpression,
            CronTimezone = trigger.CronTimezone,
            Context = new CronJobContext(appId, id),
        }, ct);
    }
}
