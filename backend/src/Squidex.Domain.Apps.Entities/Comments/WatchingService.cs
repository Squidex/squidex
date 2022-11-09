// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Comments;

public sealed class WatchingService : IWatchingService
{
    private readonly IPersistenceFactory<State> persistenceFactory;

    [CollectionName("Watches")]
    public sealed class State
    {
        private static readonly Duration Timeout = Duration.FromMinutes(1);

        public Dictionary<string, Instant> Users { get; } = new Dictionary<string, Instant>();

        public (bool, string[]) Add(string watcherId, IClock clock)
        {
            var now = clock.GetCurrentInstant();

            foreach (var (userId, lastSeen) in Users.ToList())
            {
                var timeSinceLastSeen = now - lastSeen;

                if (timeSinceLastSeen > Timeout)
                {
                    Users.Remove(userId);
                }
            }

            Users[watcherId] = now;

            return (true, Users.Keys.ToArray());
        }
    }

    public IClock Clock { get; set; } = SystemClock.Instance;

    public WatchingService(IPersistenceFactory<State> persistenceFactory)
    {
        this.persistenceFactory = persistenceFactory;
    }

    public async Task<string[]> GetWatchingUsersAsync(DomainId appId, string? resource, string userId,
        CancellationToken ct = default)
    {
        var state = new SimpleState<State>(persistenceFactory, GetType(), $"{appId}_{resource}");

        await state.LoadAsync(ct);

        return await state.UpdateAsync(x => x.Add(userId, Clock), ct: ct);
    }
}
