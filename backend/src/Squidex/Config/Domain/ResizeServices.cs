// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
using Squidex.Assets.Remote;

namespace Squidex.Config.Domain;

public static class ResizeServices
{
    public static void AddSquidexImageResizing(this IServiceCollection services, IConfiguration config)
    {
        var thumbnailGenerator = new CompositeThumbnailGenerator(
            new IAssetThumbnailGenerator[]
            {
                new ImageSharpThumbnailGenerator(),
                new ImageMagickThumbnailGenerator()
            });

        var resizerUrl = config.GetValue<string>("assets:resizerUrl");

        if (!string.IsNullOrWhiteSpace(resizerUrl))
        {
            services.AddHttpClient("Resize", options =>
            {
                options.BaseAddress = new Uri(resizerUrl);
            });

            services.AddSingletonAs(c => new RemoteThumbnailGenerator(c.GetRequiredService<IHttpClientFactory>(), thumbnailGenerator))
                .As<IAssetThumbnailGenerator>();
        }
        else
        {
            services.AddSingletonAs(c => thumbnailGenerator)
                .As<IAssetThumbnailGenerator>();
        }
    }
}
