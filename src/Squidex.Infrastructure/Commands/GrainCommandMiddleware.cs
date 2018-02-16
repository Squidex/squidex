// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;

namespace Squidex.Infrastructure.Commands
{
    public class GrainCommandMiddleware<TCommand, TGrain> : ICommandMiddleware where TCommand : IAggregateCommand where TGrain : IDomainObjectGrain
    {
        private readonly IStateFactory stateFactory;

        public GrainCommandMiddleware(IStateFactory stateFactory)
        {
            Guard.NotNull(stateFactory, nameof(stateFactory));

            this.stateFactory = stateFactory;
        }

        public async virtual Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is TCommand typedCommand)
            {
                var result = await ExecuteCommandAsync(typedCommand);

                context.Complete(result);
            }

            await next();
        }

        protected async Task<object> ExecuteCommandAsync(TCommand typedCommand)
        {
            var grain = await stateFactory.CreateAsync<TGrain>(typedCommand.AggregateId);

            var result = await grain.ExecuteAsync(typedCommand);

            return result;
        }
    }
}
