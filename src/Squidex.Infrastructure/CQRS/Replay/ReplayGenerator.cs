// ==========================================================================
//  ReplayGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS.Replay
{
    public sealed class ReplayGenerator
    {
        private readonly ILogger<ReplayGenerator> logger;
        private readonly IEventStore eventStore;
        private readonly IEventPublisher eventPublisher;
        private readonly IEnumerable<IReplayableStore> stores;

        public ReplayGenerator(
            ILogger<ReplayGenerator> logger,
            IEventStore eventStore, 
            IEventPublisher eventPublisher, 
            IEnumerable<IReplayableStore> stores)
        {
            Guard.NotNull(logger, nameof(logger));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventPublisher, nameof(eventPublisher));
            Guard.NotNull(stores, nameof(stores));

            this.stores = stores;
            this.logger = logger;
            this.eventStore = eventStore;
            this.eventPublisher = eventPublisher;
        }

        public async Task ReplayAllAsync()
        {
            logger.LogDebug("Starting to replay all events");

            if (!await ClearAsync())
            {
                return;
            }

            await ReplayEventsAsync();

            logger.LogDebug("Finished to replay all events");
        }

        private async Task ReplayEventsAsync()
        {
            try
            {
                logger.LogDebug("Replaying all messages");

                await eventStore.GetEventsAsync().ForEachAsync(eventData =>
                {
                    eventPublisher.Publish(eventData);
                });

                logger.LogDebug("Replayed all messages");
            }
            catch (Exception e)
            {
                logger.LogCritical(InfrastructureErrors.ReplayPublishingFailed, e, "Failed to publish events to {0}", eventPublisher);
            }
        }

        private async Task<bool> ClearAsync()
        {
            logger.LogDebug("Clearing replayable stores");

            foreach (var store in stores)
            {
                try
                {
                    await store.ClearAsync();

                    logger.LogDebug("Cleared store {0}", store);
                }
                catch (Exception e)
                {
                    logger.LogCritical(InfrastructureErrors.ReplayClearingFailed, e, "Failed to clear store {0}", store);

                    return false;
                }
            }

            logger.LogDebug("Cleared replayable stores");

            return true;
        }
    }
}
