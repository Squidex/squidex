// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing.Grains
{
    public interface IEventConsumerManager
    {
        Task<List<EventConsumerInfo>> GetConsumersAsync(
            CancellationToken ct = default);

        Task ResetAsync(string consumerName);

        Task StopAsync(string consumerName);

        Task StartAsync(string consumerName);
    }
}
