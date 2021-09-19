// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Orleans;

namespace Migrations.Migrations
{
    public sealed class StartEventConsumers : IMigration
    {
        private readonly IEventConsumerManagerGrain eventConsumerManager;

        public StartEventConsumers(IGrainFactory grainFactory)
        {
            eventConsumerManager = grainFactory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id);
        }

        public Task UpdateAsync(
            CancellationToken ct)
        {
            return eventConsumerManager.StartAllAsync();
        }
    }
}
