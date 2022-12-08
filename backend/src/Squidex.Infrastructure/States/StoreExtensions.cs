// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.States;

public static class StoreExtensions
{
    public static Task WriteEventAsync<T>(this IPersistence<T> persistence, Envelope<IEvent> @event,
        CancellationToken ct = default)
    {
        return persistence.WriteEventsAsync(new[] { @event }, ct);
    }
}
