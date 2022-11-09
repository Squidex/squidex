// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Translations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Apps.DomainObject;

public sealed class AppCommandMiddleware : AggregateCommandMiddleware<AppCommandBase, AppDomainObject>
{
    private readonly IAppImageStore appImageStore;
    private readonly IAssetThumbnailGenerator assetThumbnailGenerator;
    private readonly IContextProvider contextProvider;

    public AppCommandMiddleware(IDomainObjectFactory domainObjectFactory,
        IAppImageStore appImageStore, IAssetThumbnailGenerator assetThumbnailGenerator, IContextProvider contextProvider)
        : base(domainObjectFactory)
    {
        this.appImageStore = appImageStore;
        this.assetThumbnailGenerator = assetThumbnailGenerator;
        this.contextProvider = contextProvider;
    }

    public override async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        if (context.Command is UploadAppImage uploadImage)
        {
            await UploadAsync(uploadImage, ct);
        }

        await base.HandleAsync(context, next, ct);
    }

    protected override Task<object> EnrichResultAsync(CommandContext context, CommandResult result,
        CancellationToken ct)
    {
        if (result.Payload is IAppEntity app)
        {
            contextProvider.Context.App = app;
        }

        return base.EnrichResultAsync(context, result, ct);
    }

    private async Task UploadAsync(UploadAppImage uploadImage,
        CancellationToken ct)
    {
        var file = uploadImage.File;

        await using (var uploadStream = file.OpenRead())
        {
            var image = await assetThumbnailGenerator.GetImageInfoAsync(uploadStream, file.MimeType, ct);

            if (image == null)
            {
                throw new ValidationException(T.Get("apps.notImage"));
            }
        }

        await using (var uploadStream = file.OpenRead())
        {
            await appImageStore.UploadAsync(uploadImage.AppId.Id, uploadStream, ct);
        }
    }
}
