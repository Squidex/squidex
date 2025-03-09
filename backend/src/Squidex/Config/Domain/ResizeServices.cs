// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Assets;
#if INCLUDE_MAGICK
using Squidex.Assets.ImageMagick;
#endif
using Squidex.Assets.ImageSharp;
using Squidex.Assets.Remote;

namespace Squidex.Config.Domain;

public static class ResizeServices
{
    public static void AddSquidexImageResizing(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingletonAs<ImageSharpThumbnailGenerator>()
            .AsSelf();

#if INCLUDE_MAGICK
        services.AddSingletonAs<ImageMagickThumbnailGenerator>()
            .AsSelf();
#endif

        services.AddSingletonAs(c =>
            new CompositeThumbnailGenerator(
            [
                c.GetRequiredService<ImageSharpThumbnailGenerator>(),
#if INCLUDE_MAGICK
                c.GetRequiredService<ImageMagickThumbnailGenerator>(),
#endif
            ]))
            .AsSelf();

        var resizerUrl = config.GetValue<string>("assets:resizerUrl");

        if (!string.IsNullOrWhiteSpace(resizerUrl))
        {
            services.AddHttpClient("Resize", options =>
            {
                options.BaseAddress = new Uri(resizerUrl);
            });

            services.AddSingletonAs<IAssetThumbnailGenerator>(c =>
                    new RemoteThumbnailGenerator(
                        c.GetRequiredService<IHttpClientFactory>(),
                        c.GetRequiredService<CompositeThumbnailGenerator>()))
                .AsSelf();
        }
        else
        {
            services.AddSingletonAs<IAssetThumbnailGenerator>(c => c.GetRequiredService<CompositeThumbnailGenerator>())
                .AsSelf();
        }
    }
}
