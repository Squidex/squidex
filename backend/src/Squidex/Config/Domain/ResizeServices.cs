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
        services.AddSingletonAs<ImageSharpThumbnailGenerator>()
            .AsSelf();

        services.AddSingletonAs<ImageMagickThumbnailGenerator>()
            .AsSelf();

        services.AddSingletonAs(c =>
            new CompositeThumbnailGenerator(
            [
                c.GetRequiredService<ImageSharpThumbnailGenerator>(),
                c.GetRequiredService<ImageMagickThumbnailGenerator>()
            ]))
            .AsSelf();

        var resizerUrl = config.GetValue<string>("assets:resizerUrl");

        if (!string.IsNullOrWhiteSpace(resizerUrl))
        {
            services.AddHttpClient("Resize", options =>
            {
                options.BaseAddress = new Uri(resizerUrl);
            });

            services.AddSingletonAs(c =>
                    new RemoteThumbnailGenerator(
                        c.GetRequiredService<IHttpClientFactory>(),
                        c.GetRequiredService<CompositeThumbnailGenerator>()))
                .As<IAssetThumbnailGenerator>();
        }
        else
        {
            services.AddSingletonAs(c => c.GetRequiredService<CompositeThumbnailGenerator>())
                .As<IAssetThumbnailGenerator>();
        }
    }
}
