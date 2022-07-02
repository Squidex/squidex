// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MassTransit;
using Squidex.Domain.Apps.Events;
using Squidex.Hosting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Timers;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTrackerWorker :
        IConsumer<UsageTrackingAdd>,
        IConsumer<UsageTrackingUpdate>,
        IConsumer<UsageTrackingRemove>,
        IBackgroundProcess
    {
        private readonly SimpleState<State> state;
        private readonly IApiUsageTracker usageTracker;
        private CompletionTimer timer;

        public sealed class Target
        {
            public NamedId<DomainId> AppId { get; set; }

            public int Limits { get; set; }

            public int? NumDays { get; set; }

            public DateTime? Triggered { get; set; }

            public Target SetApp(NamedId<DomainId> appId)
            {
                AppId = appId;

                return this;
            }

            public Target SetLimit(int value)
            {
                Limits = value;

                return this;
            }

            public Target SetNumDays(int? value)
            {
                NumDays = value;

                return this;
            }
        }

        [CollectionName("UsageTracker")]
        public sealed class State
        {
            public Dictionary<DomainId, Target> Targets { get; set; } = new Dictionary<DomainId, Target>();
        }

        public UsageTrackerWorker(IPersistenceFactory<State> persistenceFactory, IApiUsageTracker usageTracker)
        {
            this.usageTracker = usageTracker;

            state = new SimpleState<State>(persistenceFactory, GetType(), "Default");
        }

        public async Task StartAsync(
            CancellationToken ct)
        {
            await state.LoadAsync(ct);

            timer = new CompletionTimer((int)TimeSpan.FromMinutes(10).TotalMilliseconds, _ => CheckUsagesAsync());
        }

        public Task StopAsync(
            CancellationToken ct)
        {
            return timer?.StopAsync() ?? Task.CompletedTask;
        }

        public async Task CheckUsagesAsync()
        {
            var today = DateTime.Today;

            foreach (var (key, target) in state.Value.Targets)
            {
                var from = GetFromDate(today, target.NumDays);

                if (target.Triggered == null || target.Triggered < from)
                {
                    var costs = await usageTracker.GetMonthCallsAsync(target.AppId.Id.ToString(), today, null);

                    var limit = target.Limits;

                    if (costs > limit)
                    {
                        target.Triggered = today;

                        var @event = new AppUsageExceeded
                        {
                            AppId = target.AppId,
                            CallsCurrent = costs,
                            CallsLimit = limit,
                            RuleId = key
                        };

                        await state.WriteEventAsync(Envelope.Create<IEvent>(@event));
                    }
                }
            }

            await state.WriteAsync();
        }

        private static DateTime GetFromDate(DateTime today, int? numDays)
        {
            if (numDays != null)
            {
                return today.AddDays(-numDays.Value).AddDays(1);
            }
            else
            {
                return new DateTime(today.Year, today.Month, 1);
            }
        }

        public Task Consume(ConsumeContext<UsageTrackingAdd> context)
        {
            UpdateTarget(context.Message.RuleId, t => t.SetApp(context.Message.AppId).SetLimit(context.Message.Limits).SetNumDays(context.Message.NumDays));

            return state.WriteAsync();
        }

        public Task Consume(ConsumeContext<UsageTrackingUpdate> context)
        {
            UpdateTarget(context.Message.RuleId, t => t.SetLimit(context.Message.Limits).SetNumDays(context.Message.NumDays));

            return state.WriteAsync();
        }

        public Task Consume(ConsumeContext<UsageTrackingRemove> context)
        {
            state.Value.Targets.Remove(context.Message.RuleId);

            return state.WriteAsync();
        }

        private void UpdateTarget(DomainId ruleId, Action<Target> updater)
        {
            updater(state.Value.Targets.GetOrAddNew(ruleId));
        }
    }
}
