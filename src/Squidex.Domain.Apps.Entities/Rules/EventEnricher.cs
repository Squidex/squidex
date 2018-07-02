// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using NodaTime;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Rules
{
    public sealed class EventEnricher : IEventEnricher
    {
        private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(10);
        private readonly IGrainFactory grainFactory;
        private readonly IMemoryCache cache;
        private readonly IUserResolver userResolver;
        private readonly IClock clock;

        public EventEnricher(IGrainFactory grainFactory, IMemoryCache cache, IUserResolver userResolver, IClock clock)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(cache, nameof(cache));
            Guard.NotNull(clock, nameof(clock));
            Guard.NotNull(userResolver, nameof(userResolver));

            this.userResolver = userResolver;
            this.cache = cache;
            this.clock = clock;
            this.grainFactory = grainFactory;
        }

        public async Task<EnrichedEvent> EnrichAsync(Envelope<AppEvent> @event)
        {
            Guard.NotNull(@event, nameof(@event));

            if (@event.Payload is ContentEvent contentEvent)
            {
                var result = new EnrichedContentEvent();

                await Task.WhenAll(
                    EnrichContentAsync(result, contentEvent, @event),
                    EnrichDefaultAsync(result, @event));

                return result;
            }

            if (@event.Payload is AssetEvent assetEvent)
            {
                var result = new EnrichedAssetEvent();

                await Task.WhenAll(
                    EnrichAssetAsync(result, assetEvent, @event),
                    EnrichDefaultAsync(result, @event));

                return result;
            }

            return null;
        }

        private async Task EnrichAssetAsync(EnrichedAssetEvent result, AssetEvent assetEvent, Envelope<AppEvent> @event)
        {
            var asset =
                (await grainFactory
                    .GetGrain<IAssetGrain>(assetEvent.AssetId)
                    .GetStateAsync(@event.Headers.EventStreamNumber())).Value;

            SimpleMapper.Map(asset, result);

            switch (assetEvent)
            {
                case AssetCreated _:
                    result.Type = EnrichedAssetEventType.Created;
                    break;
                case AssetRenamed _:
                    result.Type = EnrichedAssetEventType.Renamed;
                    break;
                case AssetUpdated _:
                    result.Type = EnrichedAssetEventType.Updated;
                    break;
                case AssetDeleted _:
                    result.Type = EnrichedAssetEventType.Deleted;
                    break;
            }

            result.Name = $"Asset{result.Type}";
        }

        private async Task EnrichContentAsync(EnrichedContentEvent result, ContentEvent contentEvent, Envelope<AppEvent> @event)
        {
            var content =
                (await grainFactory
                    .GetGrain<IContentGrain>(contentEvent.ContentId)
                    .GetStateAsync(@event.Headers.EventStreamNumber())).Value;

            SimpleMapper.Map(content, result);

            result.Data = content.Data ?? content.DataDraft;

            switch (contentEvent)
            {
                case ContentCreated _:
                    result.Type = EnrichedContentEventType.Created;
                    break;
                case ContentDeleted _:
                    result.Type = EnrichedContentEventType.Deleted;
                    break;
                case ContentUpdated _:
                    result.Type = EnrichedContentEventType.Updated;
                    break;
                case ContentStatusChanged contentStatusChanged:
                    if (contentStatusChanged.Status == Status.Published)
                    {
                        result.Type = EnrichedContentEventType.Published;
                    }
                    else
                    {
                        result.Type = EnrichedContentEventType.Unpublished;
                    }

                    break;
            }

            result.Name = $"{content.SchemaId.Name.ToPascalCase()}{result.Type}";
        }

        private async Task EnrichDefaultAsync(EnrichedEvent result, Envelope<AppEvent> @event)
        {
            result.Timestamp =
                @event.Headers.Contains(CommonHeaders.Timestamp) ?
                @event.Headers.Timestamp() :
                clock.GetCurrentInstant();

            if (@event.Payload is SquidexEvent squidexEvent)
            {
                result.Actor = squidexEvent.Actor;
            }

            if (@event.Payload is AppEvent appEvent)
            {
                result.AppId = appEvent.AppId;
            }

            result.User = await FindUserAsync(result.Actor);
        }

        private Task<IUser> FindUserAsync(RefToken actor)
        {
            var key = $"EventEnrichers_Users_${actor.Identifier}";

            return cache.GetOrCreateAsync(key, async x =>
            {
                x.AbsoluteExpirationRelativeToNow = UserCacheDuration;

                try
                {
                    return await userResolver.FindByIdOrEmailAsync(actor.Identifier);
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}
