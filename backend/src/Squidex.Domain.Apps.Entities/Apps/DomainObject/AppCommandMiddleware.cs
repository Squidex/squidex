// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject
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

            await base.HandleAsync(context, next);
        }

        protected override Task<object> EnrichResultAsync(CommandContext context, CommandResult result)
        {
            if (result.Payload is IAppEntity app)
            {
                contextProvider.Context.App = app;
            }

            return base.EnrichResultAsync(context, result);
        }

        private async Task UploadAsync(UploadAppImage uploadImage)
        {
            var file = uploadImage.File;

            await using (var uploadStream = file.OpenRead())
            {
                var image = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream);

                if (image == null)
                {
                    throw new ValidationException(T.Get("apps.notImage"));
                }
            }

            await using (var uploadStream = file.OpenRead())
            {
                await appImageStore.UploadAsync(uploadImage.AppId.Id, uploadStream);
            }
        }
    }
}
