// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    [Reentrant]
    public sealed class UsageTrackerGrain : GrainOfString, IRemindable, IUsageTrackerGrain
    {
        private readonly IGrainState<State> state;
        private readonly IApiUsageTracker usageTracker;

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

        public UsageTrackerGrain(IGrainState<State> state, IApiUsageTracker usageTracker)
        {
            this.state = state;

            this.usageTracker = usageTracker;
        }

        protected override Task OnActivateAsync(string key)
        {
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));
            RegisterTimer(x => CheckUsagesAsync(), null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        public Task ActivateAsync()
        {
            return Task.CompletedTask;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return Task.CompletedTask;
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

        public Task AddTargetAsync(DomainId ruleId, NamedId<DomainId> appId, int limits, int? numDays)
        {
            UpdateTarget(ruleId, t => t.SetApp(appId).SetLimit(limits).SetNumDays(numDays));

            return state.WriteAsync();
        }

        public Task UpdateTargetAsync(DomainId ruleId, int limits, int? numDays)
        {
            UpdateTarget(ruleId, t => t.SetLimit(limits).SetNumDays(numDays));

            return state.WriteAsync();
        }

        public Task AddTargetAsync(DomainId ruleId, int limits)
        {
            UpdateTarget(ruleId, t => t.SetLimit(limits));

            return state.WriteAsync();
        }

        public Task RemoveTargetAsync(DomainId ruleId)
        {
            state.Value.Targets.Remove(ruleId);

            return state.WriteAsync();
        }

        private void UpdateTarget(DomainId ruleId, Action<Target> updater)
        {
            updater(state.Value.Targets.GetOrAddNew(ruleId));
        }
    }
}
