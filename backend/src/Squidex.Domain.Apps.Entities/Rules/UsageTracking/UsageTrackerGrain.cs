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
        private readonly IGrainState<GrainState> state;
        private readonly IApiUsageTracker usageTracker;

        public sealed class Target
        {
            public NamedId<Guid> AppId { get; set; }

            public int Limits { get; set; }

            public int? NumDays { get; set; }

            public DateTime? Triggered { get; set; }
        }

        [CollectionName("UsageTracker")]
        public sealed class GrainState
        {
            public Dictionary<Guid, Target> Targets { get; set; } = new Dictionary<Guid, Target>();
        }

        public UsageTrackerGrain(IGrainState<GrainState> state, IApiUsageTracker usageTracker)
        {
            Guard.NotNull(state);
            Guard.NotNull(usageTracker);

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

                if (!target.Triggered.HasValue || target.Triggered < from)
                {
                    var costs = await usageTracker.GetMonthCostsAsync(target.AppId.Id.ToString(), today);

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
            if (numDays.HasValue)
            {
                return today.AddDays(-numDays.Value).AddDays(1);
            }
            else
            {
                return new DateTime(today.Year, today.Month, 1);
            }
        }

        public Task AddTargetAsync(Guid ruleId, NamedId<Guid> appId, int limits, int? numDays)
        {
            UpdateTarget(ruleId, t => { t.Limits = limits; t.AppId = appId; t.NumDays = numDays; });

            return state.WriteAsync();
        }

        public Task UpdateTargetAsync(Guid ruleId, int limits, int? numDays)
        {
            UpdateTarget(ruleId, t => { t.Limits = limits; t.NumDays = numDays; });

            return state.WriteAsync();
        }

        public Task AddTargetAsync(Guid ruleId, int limits)
        {
            UpdateTarget(ruleId, t => t.Limits = limits);

            return state.WriteAsync();
        }

        public Task RemoveTargetAsync(Guid ruleId)
        {
            state.Value.Targets.Remove(ruleId);

            return state.WriteAsync();
        }

        private void UpdateTarget(Guid ruleId, Action<Target> updater)
        {
            updater(state.Value.Targets.GetOrAddNew(ruleId));
        }
    }
}
