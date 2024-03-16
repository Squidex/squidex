// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.Commands
{
    public static class CommandExtensions
    {
        public static Task HandleAsync(this ICommandMiddleware commandMiddleware, CommandContext context)
        {
            return commandMiddleware.HandleAsync(context, x => Task.CompletedTask);
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
}
