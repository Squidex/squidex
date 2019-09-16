// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppCommandMiddleware : GrainCommandMiddleware<AppCommand, IAppGrain>
    {
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IContextProvider contextProvider;

        public AppCommandMiddleware(
            IGrainFactory grainFactory,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IContextProvider contextProvider)
            : base(grainFactory)
        {
            Guard.NotNull(contextProvider, nameof(contextProvider));
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));

            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
            this.contextProvider = contextProvider;
        }

        public override async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is UploadAppImage uploadImage)
            {
                await UploadAsync(uploadImage);
            }

            await ExecuteCommandAsync(context);

            if (context.PlainResult is IAppEntity app)
            {
                contextProvider.Context.App = app;
            }

            await next();
        }

        private async Task UploadAsync(UploadAppImage uploadImage)
        {
            var file = uploadImage.File;

            var image = await assetThumbnailGenerator.GetImageInfoAsync(file.OpenRead());

            if (image == null)
            {
                throw new ValidationException("File is not an image.");
            }

            await assetStore.UploadAsync(uploadImage.AppId.ToString(), file.OpenRead(), true);
        }
    }
}
