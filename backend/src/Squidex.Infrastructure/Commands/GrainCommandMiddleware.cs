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

                var payload = await EnrichResultAsync(context, result);

                context.Complete(payload);
            }
        }

        protected virtual Task<object> EnrichResultAsync(CommandContext context, CommandResult result)
        {
            return Task.FromResult(result.Payload is None ? result : result.Payload);
        }

        private async Task<CommandResult> ExecuteCommandAsync(TCommand typedCommand)
        {
            var grain = grainFactory.GetGrain<TGrain>(typedCommand.AggregateId.ToString());

            var result = await grain.ExecuteAsync(CommandRequest.Create(typedCommand));

            return result.Value;
        }
    }
}
