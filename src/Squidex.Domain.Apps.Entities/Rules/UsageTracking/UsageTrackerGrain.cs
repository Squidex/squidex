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
using Orleans.Runtime;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTrackerGrain : GrainOfString<UsageTrackerGrain.GrainState>, IRemindable, IUsageTrackerGrain
    {
        private readonly IUsageTracker usageTracker;

        public sealed class Target
        {
            public int Limits { get; set; }

            public bool Disabled { get; set; }

            public DateTime Triggered { get; set; }

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

            return Task.CompletedTask;
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            var today = DateTime.Today;

            foreach (var kvp in State.Targets)
            {
                var appId = kvp.Value.AppId;

                if (!IsSameMonth(today, kvp.Value.Triggered))
                {
                    var usage = await usageTracker.GetMonthlyCallsAsync(appId.Id.ToString(), today);

                    var limit = kvp.Value.Limits;

                    if (usage > limit)
                    {
                        kvp.Value.Triggered = today;

                        var @event = new AppUsageExceeded
                        {
                            AppId = appId,
                            CallsCurrent = usage,
                            CallsLimit = limit,
                            RuleId = kvp.Key
                        };

                        await Persistence.WriteEventAsync(@event);
                    }
                }
            }

            await WriteStateAsync();
        }

        private static bool IsSameMonth(DateTime lhs, DateTime rhs)
        {
            return lhs.Year == rhs.Year && lhs.Month == rhs.Month;
        }

        public Task AddTargetAsync(Guid ruleId, NamedId<Guid> appId, int limits)
        {
            UpdateTarget(ruleId, t => { t.Limits = limits; t.AppId = appId; });

            return WriteStateAsync();
        }

        public Task UpdateTargetAsync(Guid ruleId, int limits)
        {
            UpdateTarget(ruleId, t => t.Limits = limits);

            return WriteStateAsync();
        }

        public Task ActivateTargetAsync(Guid ruleId)
        {
            UpdateTarget(ruleId, t => t.Disabled = false);

            return WriteStateAsync();
        }

        public Task DeactivateTargetAsync(Guid ruleId)
        {
            UpdateTarget(ruleId, t => t.Disabled = true);

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
