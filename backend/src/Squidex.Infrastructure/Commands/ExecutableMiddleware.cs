// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Infrastructure.Commands
{
    public class ExecutableMiddleware<TCommand, T> : ICommandMiddleware where TCommand : IAggregateCommand where T : IExecutable
    {
        private readonly Func<DomainId, T> factory;

        public ExecutableMiddleware(IServiceProvider serviceProvider)
        {
            var objectFactory = ActivatorUtilities.CreateFactory(typeof(T), new[] { typeof(DomainId) });

            factory = id =>
            {
                return (T)objectFactory(serviceProvider, new object?[] { id });
            };
        }

        public virtual async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await ExecuteCommandAsync(context);

            await next(context);
        }

        protected async Task ExecuteCommandAsync(CommandContext context)
        {
            if (context.Command is TCommand typedCommand)
            {
                var commandResult = await ExecuteCommandAsync(typedCommand);
                var commandPayload = await EnrichResultAsync(context, commandResult);

                context.Complete(commandPayload);
            }
        }

        protected virtual Task<object> EnrichResultAsync(CommandContext context, CommandResult result)
        {
            return Task.FromResult(result.Payload is None ? result : result.Payload);
        }

        private Task<CommandResult> ExecuteCommandAsync(TCommand typedCommand)
        {
            var executable = factory(typedCommand.AggregateId);

            return executable.ExecuteAsync(typedCommand);
        }
    }
}
