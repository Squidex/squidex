// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Infrastructure.Commands
{
    public class SyncedGrainCommandMiddleware<TCommand, TGrain> : ICommandMiddleware where TCommand : IAggregateCommand where TGrain : IDomainObjectGrain
    {
        private readonly AsyncLockPool lockPool = new AsyncLockPool(10000);
        private readonly IStateFactory stateFactory;

        public SyncedGrainCommandMiddleware(IStateFactory stateFactory)
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
            var id = typedCommand.AggregateId;

            using (await lockPool.LockAsync(typedCommand.AggregateId))
            {
                try
                {
                    var grain = await stateFactory.GetSingleAsync<TGrain>(id);

                    var result = await grain.ExecuteAsync(typedCommand);

                    stateFactory.Synchronize<TGrain, Guid>(id);

                    return result;
                }
                catch
                {
                    stateFactory.Remove<TGrain, Guid>(id);
                    throw;
                }
            }
        }
    }
}
