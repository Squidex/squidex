// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Orleans;

namespace Migrations.Migrations
{
    public sealed class StopEventConsumers : IMigration
    {
        private readonly IEventConsumerManager eventConsumerManager;

        public StopEventConsumers(IGrainFactory grainFactory)
        {
            eventConsumerManager = grainFactory.GetGrain<IEventConsumerManager>(SingleGrain.Id);
        }

        public Task UpdateAsync(
            CancellationToken ct)
        {
            return eventConsumerManager.StopAllAsync();
        }
    }
}
