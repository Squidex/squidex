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
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.UsageTracking;

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking
{
    public sealed class UsageTrackerGrain : GrainOfString, IRemindable, IUsageTrackerGrain
    {
        private readonly IStore<string> store;
        private readonly IUsageTracker usageTracker;
        private IPersistence<State> persistence;
        private State state;

        public sealed class Target
        {
            public int Limit { get; set; }

            public bool Enabled { get; set; }

            public DateTime Triggered { get; set; }
        }

        [CollectionName("UsageTracker")]
        public sealed class State
        {
            public Dictionary<NamedId<Guid>, Target> Targets { get; set; } = new Dictionary<NamedId<Guid>, Target>();
        }

        public UsageTrackerGrain(IStore<string> store, IUsageTracker usageTracker)
        {
            Guard.NotNull(store, nameof(store));
            Guard.NotNull(usageTracker, nameof(usageTracker));

            this.store = store;

            this.usageTracker = usageTracker;
        }

        public override Task OnActivateAsync(string key)
        {
            DelayDeactivation(TimeSpan.FromDays(1));

            RegisterOrUpdateReminder("Default", TimeSpan.Zero, TimeSpan.FromMinutes(10));

            persistence = store.WithSnapshotsAndEventSourcing<State, string>(GetType(), key, ApplySnapshot, ApplyEvent);

            return persistence.ReadAsync();
        }

        private void ApplySnapshot(State s)
        {
            state = s;
        }

        private void ApplyEvent(Envelope<IEvent> @event)
        {
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            var today = DateTime.Today;

            foreach (var kvp in state.Targets)
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

                        await persistence.WriteEventsAsync(new[]
                        {
                            Envelope.Create(@event)
                        });
                    }
                }
            }

            await persistence.WriteSnapshotAsync(state);
        }

        private static bool IsSameMonth(DateTime lhs, DateTime rhs)
        {
            return lhs.Year == rhs.Year && lhs.Month == rhs.Month;
        }

        public Task ActivateTargetAsync(NamedId<Guid> appId)
        {
            UpdateTarget(appId, t => t.Enabled = true);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task DeactivateTargetAsync(NamedId<Guid> appId)
        {
            UpdateTarget(appId, t => t.Enabled = false);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task AddTargetAsync(NamedId<Guid> appId, int limits)
        {
            UpdateTarget(appId, t => t.Limit = limits);

            return persistence.WriteSnapshotAsync(state);
        }

        public Task RemoveTargetAsync(NamedId<Guid> appId)
        {
            state.Targets.Remove(appId);

            return persistence.WriteSnapshotAsync(state);
        }

        private void UpdateTarget(NamedId<Guid> appId, Action<Target> updater)
        {
            updater(state.Targets.GetOrAddNew(appId));;
        }
    }
}
