// ==========================================================================
//  AssetCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;
using Squidex.Write.Assets.Commands;

namespace Squidex.Write.Assets
{
    public class AssetCommandHandler : ICommandHandler
    {
        private readonly IAggregateHandler handler;

        public AssetCommandHandler(IAggregateHandler handler)
        {
            Guard.NotNull(handler, nameof(handler));

            this.handler = handler;
        }

        protected async Task On(CreateAsset command, CommandContext context)
        {
            await handler.CreateAsync<AssetDomainObject>(context, c =>
            {
                c.Create(command);

                context.Succeed(EntityCreatedResult.Create(c.Id, c.Version));
            });
        }

        protected async Task On(RenameAsset command, CommandContext context)
        {
            await handler.UpdateAsync<AssetDomainObject>(context, c => c.Rename(command));
        }

        protected async Task On(UpdateAsset command, CommandContext context)
        {
            await handler.UpdateAsync<AssetDomainObject>(context, c => c.Update(command));
        }

        protected Task On(DeleteAsset command, CommandContext context)
        {
            return handler.UpdateAsync<AssetDomainObject>(context, c => c.Delete(command));
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? TaskHelper.False : this.DispatchActionAsync(context.Command, context);
        }
    }
}
