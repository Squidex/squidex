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
            public int Limit { get; set; }

            public bool Enabled { get; set; }

            public DateTime Triggered { get; set; }
        }

        [CollectionName("UsageTracker")]
        public sealed class GrainState
        {
            public Dictionary<NamedId<Guid>, Target> Targets { get; set; } = new Dictionary<NamedId<Guid>, Target>();
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
                var appId = kvp.Key;

                if (!IsSameMonth(today, kvp.Value.Triggered))
                {
                    var usage = await usageTracker.GetMonthlyCallsAsync(appId.Id.ToString(), today);

                    var limit = kvp.Value.Limit;

                    if (usage > limit)
                    {
                        kvp.Value.Triggered = today;

                        var @event = new AppUsageExceeded { AppId = appId, Current = usage, Limit = limit };

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

        public Task ActivateTargetAsync(NamedId<Guid> appId)
        {
            UpdateTarget(appId, t => t.Enabled = true);

            return WriteStateAsync();
        }

        public Task DeactivateTargetAsync(NamedId<Guid> appId)
        {
            UpdateTarget(appId, t => t.Enabled = false);

            return WriteStateAsync();
        }

        public Task AddTargetAsync(NamedId<Guid> appId, int limits)
        {
            UpdateTarget(appId, t => t.Limit = limits);

            return WriteStateAsync();
        }

        public Task RemoveTargetAsync(NamedId<Guid> appId)
        {
            State.Targets.Remove(appId);

            return WriteStateAsync();
        }

        private void UpdateTarget(NamedId<Guid> appId, Action<Target> updater)
        {
            updater(State.Targets.GetOrAddNew(appId));;
        }
    }
}
