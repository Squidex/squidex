﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Infrastructure.EventSourcing.Grains;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Orleans;

namespace Migrate_01.Migrations
{
    public sealed class StopEventConsumers : IMigration
    {
        private readonly IEventConsumerManagerGrain eventConsumerManager;

        public StopEventConsumers(IGrainFactory grainFactory)
        {
            eventConsumerManager = grainFactory.GetGrain<IEventConsumerManagerGrain>(SingleGrain.Id);
        }

        public Task UpdateAsync()
        {
            return eventConsumerManager.StopAllAsync();
        }
    }
}
