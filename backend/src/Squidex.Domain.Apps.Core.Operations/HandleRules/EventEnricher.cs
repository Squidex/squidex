﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.HandleRules
{
    public sealed class EventEnricher : IEventEnricher
    {
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(10);
        private readonly IMemoryCache userCache;
        private readonly IUserResolver userResolver;

        public EventEnricher(IMemoryCache userCache, IUserResolver userResolver)
        {
            Guard.NotNull(userCache);
            Guard.NotNull(userResolver);

            this.userCache = userCache;
            this.userResolver = userResolver;
        }

        public async Task EnrichAsync(EnrichedEvent enrichedEvent, Envelope<AppEvent> @event)
        {
            enrichedEvent.Timestamp = @event.Headers.Timestamp();

            if (enrichedEvent is EnrichedUserEventBase userEvent)
            {
                if (@event.Payload is SquidexEvent squidexEvent)
                {
                    userEvent.Actor = squidexEvent.Actor;
                }

                userEvent.User = await FindUserAsync(userEvent.Actor);
            }

            enrichedEvent.AppId = @event.Payload.AppId;
        }

        private Task<IUser?> FindUserAsync(RefToken actor)
        {
            var key = $"EventEnrichers_Users_${actor.Identifier}";

            return userCache.GetOrCreateAsync(key, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = UserCacheDuration;

                IUser? user;
                try
                {
                     user = await userResolver.FindByIdOrEmailAsync(actor.Identifier);
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
}
