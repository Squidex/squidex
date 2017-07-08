// ==========================================================================
//  AssetCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;
using Squidex.Domain.Apps.Write.Assets.Commands;

namespace Squidex.Domain.Apps.Write.Assets
{
    public class AssetCommandHandler : ICommandHandler
    {
        private readonly IAggregateHandler handler;
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetCommandHandler(
            IAggregateHandler handler, 
            IAssetStore assetStore, 
            IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));

            this.handler = handler;
            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        protected async Task On(CreateAsset command, CommandContext context)
        {
            await handler.CreateAsync<AssetDomainObject>(context, async c =>
            {
                command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());

                c.Create(command);

                await assetStore.UploadAsync(c.Id.ToString(), c.FileVersion, null, command.File.OpenRead());

                context.Succeed(EntityCreatedResult.Create(c.Id, c.Version));
            });
        }

        protected async Task On(UpdateAsset command, CommandContext context)
        {
            await handler.UpdateAsync<AssetDomainObject>(context, async c =>
            {
                command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());

                c.Update(command);

                await assetStore.UploadAsync(c.Id.ToString(), c.FileVersion, null, command.File.OpenRead());
            });
        }

        protected Task On(RenameAsset command, CommandContext context)
        {
            return handler.UpdateAsync<AssetDomainObject>(context, c => c.Rename(command));
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
