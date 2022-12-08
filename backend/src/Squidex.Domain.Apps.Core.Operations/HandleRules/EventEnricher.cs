// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Caching.Memory;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class EventEnricher : IEventEnricher
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private readonly IMemoryCache userCache;
    private readonly IUserResolver userResolver;

    public EventEnricher(IMemoryCache userCache, IUserResolver userResolver)
    {
        this.userCache = userCache;
        this.userResolver = userResolver;
    }

    public async Task EnrichAsync(EnrichedEvent enrichedEvent, Envelope<AppEvent>? @event)
    {
        if (@event != null)
        {
            enrichedEvent.Timestamp = @event.Headers.Timestamp();

            enrichedEvent.AppId = @event.Payload.AppId;
        }

        if (enrichedEvent is EnrichedUserEventBase userEvent)
        {
            if (@event?.Payload is SquidexEvent squidexEvent)
            {
                userEvent.Actor = squidexEvent.Actor;
            }

            if (userEvent.Actor != null)
            {
                userEvent.User = await FindUserAsync(userEvent.Actor);
            }
        }
    }

    private Task<IUser?> FindUserAsync(RefToken actor)
    {
        var cacheKey = $"{typeof(EventEnricher)}_Users_{actor.Identifier}";

        return userCache.GetOrCreateAsync(cacheKey, async x =>
        {
            x.AbsoluteExpirationRelativeToNow = CacheDuration;

            IUser? user;
            try
            {
                user = await userResolver.FindByIdAsync(actor.Identifier);
            }
            catch
            {
                user = null;
            }

            if (user == null && actor.IsClient)
            {
                user = new ClientUser(actor);
            }

            return user;
        });
    }
}
