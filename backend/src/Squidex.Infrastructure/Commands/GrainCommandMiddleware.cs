// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Commands
{
    public class GrainCommandMiddleware<TCommand, TGrain> : ICommandMiddleware where TCommand : IAggregateCommand where TGrain : IDomainObjectGrain
    {
        private readonly IGrainFactory grainFactory;

        public GrainCommandMiddleware(IGrainFactory grainFactory)
        {
            Guard.NotNull(grainFactory);

            this.grainFactory = grainFactory;
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
                var result = await ExecuteCommandAsync(typedCommand);

                context.Complete(result);
            }
        }

        private async Task<object?> ExecuteCommandAsync(TCommand typedCommand)
        {
            var grain = grainFactory.GetGrain<TGrain>(typedCommand.AggregateId);

            var result = await grain.ExecuteAsync(typedCommand);

            return result.Value;
        }
    }
}
