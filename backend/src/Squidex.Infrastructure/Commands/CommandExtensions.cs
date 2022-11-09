// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.Commands;

public static class CommandExtensions
{
    private static readonly NextDelegate End = (c, ct) => Task.CompletedTask;

    public static Task HandleAsync(this ICommandMiddleware commandMiddleware, CommandContext context,
        CancellationToken ct)
    {
        return commandMiddleware.HandleAsync(context, End, ct);
    }

    public static Envelope<IEvent> Migrate<T>(this Envelope<IEvent> @event, T snapshot)
    {
        if (@event.Payload is IMigratedStateEvent<T> migratable)
        {
            var payload = migratable.Migrate(snapshot);

            @event = new Envelope<IEvent>(payload, @event.Headers);
        }

        return @event;
    }
}
