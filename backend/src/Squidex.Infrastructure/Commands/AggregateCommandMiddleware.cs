// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Commands
{
    public class AggregateCommandMiddleware<TCommand, TTarget> : ICommandMiddleware
        where TCommand : IAggregateCommand where TTarget : IAggregate
    {
        private readonly IDomainObjectFactory domainObjectFactory;

        public AggregateCommandMiddleware(IDomainObjectFactory domainObjectFactory)
        {
            this.domainObjectFactory = domainObjectFactory;
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

        protected virtual Task<CommandResult> ExecuteCommandAsync(TCommand command)
        {
            var executable = domainObjectFactory.Create<TTarget>(command.AggregateId);

            return ExecuteCommandAsync(executable, command);
        }

        protected virtual Task<CommandResult> ExecuteCommandAsync(TTarget executable, TCommand command)
        {
            return executable.ExecuteAsync(command);
        }
    }
}
