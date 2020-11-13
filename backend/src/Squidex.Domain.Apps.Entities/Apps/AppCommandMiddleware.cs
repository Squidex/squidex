// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps
{
    public sealed class AppCommandMiddleware : GrainCommandMiddleware<AppCommand, IAppGrain>
    {
        private readonly IAppImageStore appImageStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
        private readonly IContextProvider contextProvider;

        public AppCommandMiddleware(
            IGrainFactory grainFactory,
            IAppImageStore appImageStore,
            IAssetThumbnailGenerator assetThumbnailGenerator,
            IContextProvider contextProvider)
            : base(grainFactory)
        {
            Guard.NotNull(contextProvider, nameof(contextProvider));
            Guard.NotNull(appImageStore, nameof(appImageStore));
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));

            this.appImageStore = appImageStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
            this.contextProvider = contextProvider;
        }

        public override async Task HandleAsync(CommandContext context, NextDelegate next)
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

            await next(context);
        }

        private async Task UploadAsync(UploadAppImage uploadImage)
        {
            var file = uploadImage.File;

            using (var uploadStream = file.OpenRead())
            {
                var image = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream);

                if (image == null)
                {
                    throw new ValidationException(T.Get("apps.notImage"));
                }
            }

            using (var uploadStream = file.OpenRead())
            {
                await appImageStore.UploadAsync(uploadImage.AppId.Id, uploadStream);
            }
        }
    }
}
