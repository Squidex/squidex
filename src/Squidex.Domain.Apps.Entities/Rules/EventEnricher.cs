// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache userCache;
        private readonly IUserResolver userResolver;

        public EventEnricher(IGrainFactory grainFactory, IMemoryCache userCache, IUserResolver userResolver)
        {
            Guard.NotNull(grainFactory, nameof(grainFactory));
            Guard.NotNull(userCache, nameof(userCache));
            Guard.NotNull(userResolver, nameof(userResolver));

            this.grainFactory = grainFactory;
            this.userCache = userCache;
            this.userResolver = userResolver;
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
                case ContentChangesPublished _:
                case ContentUpdated _:
                    result.Type = EnrichedContentEventType.Updated;
                    break;
                case ContentStatusChanged contentStatusChanged:
                    switch (contentStatusChanged.Change)
                    {
                        case StatusChange.Published:
                            result.Type = EnrichedContentEventType.Published;
                            break;
                        case StatusChange.Unpublished:
                            result.Type = EnrichedContentEventType.Unpublished;
                            break;
                        case StatusChange.Archived:
                            result.Type = EnrichedContentEventType.Archived;
                            break;
                        case StatusChange.Restored:
                            result.Type = EnrichedContentEventType.Restored;
                            break;
                    }

                    break;
            }

            result.Name = $"{content.SchemaId.Name.ToPascalCase()}{result.Type}";
        }

        private async Task EnrichDefaultAsync(EnrichedEvent result, Envelope<AppEvent> @event)
        {
            result.Timestamp = @event.Headers.Timestamp();

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

            return userCache.GetOrCreateAsync(key, async x =>
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
