// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Orleans.Core;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Orleans;

namespace Squidex.Domain.Apps.Entities.Comments
{
    public sealed class WatchingGrain : GrainBase, IWatchingGrain
    {
        private static readonly Duration Timeout = Duration.FromMinutes(1);
        private readonly Dictionary<string, Dictionary<string, Instant>> users = new Dictionary<string, Dictionary<string, Instant>>();
        private readonly IClock clock;

        public WatchingGrain(IGrainIdentity grainIdentity, IClock clock)
            : base(grainIdentity)
        {
            this.clock = clock;
        }

        public override Task OnActivateAsync()
        {
            var time = TimeSpan.FromSeconds(30);

            RegisterTimer(x =>
            {
                Cleanup();

                return Task.CompletedTask;
            }, null, time, time);

            return Task.CompletedTask;
        }

        public Task<string[]> GetWatchingUsersAsync(string resource, string userId)
        {
            Guard.NotNullOrEmpty(resource);
            Guard.NotNullOrEmpty(userId);

            var usersByResource = users.GetOrAddNew(resource);

            usersByResource[userId] = clock.GetCurrentInstant();

            return Task.FromResult(usersByResource.Keys.ToArray());
        }

        public void Cleanup()
        {
            if (users.Count == 0)
            {
                return;
            }

            var now = clock.GetCurrentInstant();

            foreach (var (resource, usersByResource) in users.ToList())
            {
                foreach (var (userId, lastSeen) in usersByResource.ToList())
                {
                    var timeSinceLastSeen = now - lastSeen;

                    if (timeSinceLastSeen > Timeout)
                    {
                        usersByResource.Remove(userId);
                    }
                }

                if (usersByResource.Count == 0)
                {
                    users.Remove(resource);
                }
            }

            if (users.Count == 0)
            {
                TryDeactivateOnIdle();
            }
        }
    }
}
