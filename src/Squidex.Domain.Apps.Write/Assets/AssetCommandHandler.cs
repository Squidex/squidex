// ==========================================================================
//  AssetCommandHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Write.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;

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
            command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());
            try
            {
                var asset = await handler.CreateAsync<AssetDomainObject>(context, async a =>
                {
                    a.Create(command);

                    await assetStore.UploadTemporaryAsync(context.ContextId.ToString(), command.File.OpenRead());

                    context.Succeed(EntityCreatedResult.Create(a.Id, a.Version));
                });

                await assetStore.CopyTemporaryAsync(context.ContextId.ToString(), asset.Id.ToString(), asset.FileVersion, null);
            }
            finally
            {
                await assetStore.DeleteTemporaryAsync(context.ContextId.ToString());
            }
        }

        protected async Task On(UpdateAsset command, CommandContext context)
        {
            command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());

            try
            {
                var asset = await handler.UpdateAsync<AssetDomainObject>(context, async a =>
                {
                    a.Update(command);

                    await assetStore.UploadTemporaryAsync(context.ContextId.ToString(), command.File.OpenRead());
                });

                await assetStore.CopyTemporaryAsync(context.ContextId.ToString(), asset.Id.ToString(), asset.FileVersion, null);
            }
            finally
            {
                await assetStore.DeleteTemporaryAsync(context.ContextId.ToString());
            }
        }

        protected Task On(RenameAsset command, CommandContext context)
        {
            return handler.UpdateAsync<AssetDomainObject>(context, a => a.Rename(command));
        }

        protected Task On(DeleteAsset command, CommandContext context)
        {
            return handler.UpdateAsync<AssetDomainObject>(context, a => a.Delete(command));
        }

        public Task<bool> HandleAsync(CommandContext context)
        {
            return context.IsHandled ? TaskHelper.False : this.DispatchActionAsync(context.Command, context);
        }
    }
}
