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
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    [Reentrant]
    public sealed class UsageTrackerGrain : GrainOfString<UsageTrackerGrain.GrainState>, IRemindable, IUsageTrackerGrain
    {
        private readonly IUsageTracker usageTracker;

        public sealed class Target
        {
            public int Limits { get; set; }

            public int? NumDays { get; set; }

            public DateTime? Triggered { get; set; }

            public NamedId<Guid> AppId { get; set; }
        }

        [CollectionName("UsageTracker")]
        public sealed class GrainState
        {
            public Dictionary<Guid, Target> Targets { get; set; } = new Dictionary<Guid, Target>();
        }

        public UsageTrackerGrain(IStore<string> store, IUsageTracker usageTracker)
            : base(store)
        {
            Guard.NotNull(usageTracker, nameof(usageTracker));

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
            return TaskHelper.Done;
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return TaskHelper.Done;
        }

        public async Task CheckUsagesAsync()
        {
            var today = DateTime.Today;

            foreach (var kvp in State.Targets)
            {
                var target = kvp.Value;

                var (from, _) = GetDateRange(today, target.NumDays);

                if (!target.Triggered.HasValue || target.Triggered < from)
                {
                    var usage = await usageTracker.GetMonthlyCallsAsync(target.AppId.Id.ToString(), today);

                    var limit = kvp.Value.Limits;

                    if (usage > limit)
                    {
                        kvp.Value.Triggered = today;

                        var @event = new AppUsageExceeded
                        {
                            AppId = target.AppId,
                            CallsCurrent = usage,
                            CallsLimit = limit,
                            RuleId = kvp.Key
                        };

                        await Persistence.WriteEventAsync(Envelope.Create<IEvent>(@event));
                    }
                }
            }

            await WriteStateAsync();
        }

        private static (DateTime, DateTime) GetDateRange(DateTime today, int? numDays)
        {
            if (numDays.HasValue)
            {
                return (today.AddDays(-numDays.Value).AddDays(1), today);
            }
            else
            {
                return (new DateTime(today.Year, today.Month, 1), today);
            }
        }

        public Task AddTargetAsync(Guid ruleId, NamedId<Guid> appId, int limits, int? numDays)
        {
            UpdateTarget(ruleId, t => { t.Limits = limits; t.AppId = appId; t.NumDays = numDays; });

            return WriteStateAsync();
        }

        public Task UpdateTargetAsync(Guid ruleId, int limits, int? numDays)
        {
            UpdateTarget(ruleId, t => { t.Limits = limits; t.NumDays = numDays; });

            return WriteStateAsync();
        }

        public Task AddTargetAsync(Guid ruleId, int limits)
        {
            UpdateTarget(ruleId, t => t.Limits = limits);

            return WriteStateAsync();
        }

        public Task RemoveTargetAsync(Guid ruleId)
        {
            State.Targets.Remove(ruleId);

            return WriteStateAsync();
        }

        private void UpdateTarget(Guid ruleId, Action<Target> updater)
        {
            updater(State.Targets.GetOrAddNew(ruleId));
        }
    }
}
