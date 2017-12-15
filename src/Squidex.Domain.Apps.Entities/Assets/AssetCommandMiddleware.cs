// ==========================================================================
//  AssetCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Assets.Guards;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetCommandMiddleware(
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
                var asset = await handler.CreateSyncedAsync<AssetDomainObject>(context, async a =>
                {
                    GuardAsset.CanCreate(command);

                    a.Create(command);

                    await assetStore.UploadTemporaryAsync(context.ContextId.ToString(), command.File.OpenRead());

                    context.Complete(EntityCreatedResult.Create(command.AssetId, a.Version));
                });

                await assetStore.CopyTemporaryAsync(context.ContextId.ToString(), command.AssetId.ToString(), asset.State.FileVersion, null);
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
                var asset = await handler.UpdateSyncedAsync<AssetDomainObject>(context, async a =>
                {
                    GuardAsset.CanUpdate(command);

                    a.Update(command);

                    await assetStore.UploadTemporaryAsync(context.ContextId.ToString(), command.File.OpenRead());

                    context.Complete(new AssetSavedResult(a.Version, a.State.FileVersion));
                });

                await assetStore.CopyTemporaryAsync(context.ContextId.ToString(), command.AssetId.ToString(), asset.State.FileVersion, null);
            }
            finally
            {
                await assetStore.DeleteTemporaryAsync(context.ContextId.ToString());
            }
        }

        protected Task On(RenameAsset command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AssetDomainObject>(context, a =>
            {
                GuardAsset.CanRename(command, a.State.FileName);

                a.Rename(command);
            });
        }

        protected Task On(DeleteAsset command, CommandContext context)
        {
            return handler.UpdateSyncedAsync<AssetDomainObject>(context, a =>
            {
                GuardAsset.CanDelete(command);

                a.Delete(command);
            });
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (!await this.DispatchActionAsync(context.Command, context))
            {
                await next();
            }
        }
    }
}
