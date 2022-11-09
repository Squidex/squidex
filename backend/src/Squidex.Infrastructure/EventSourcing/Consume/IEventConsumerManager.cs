// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.EventSourcing.Consume;

public interface IEventConsumerManager
{
    Task<List<EventConsumerInfo>> GetConsumersAsync(
        CancellationToken ct = default);

    Task<EventConsumerInfo> ResetAsync(string consumerName,
        CancellationToken ct = default);

    Task<EventConsumerInfo> StopAsync(string consumerName,
        CancellationToken ct = default);

    Task<EventConsumerInfo> StartAsync(string consumerName,
        CancellationToken ct = default);
}
