// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Images;
using Squidex.Areas.Api.Controllers.Images.Service;

namespace Squidex.Config.Domain
{
    public static class ResizeServices
    {
        public static void AddSquidexImageResizing(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingletonAs<ImagesMiddleware>()
                    .AsSelf();

            services.AddSingletonAs<InProcessImageResizer>()
                .AsSelf().As<IImageResizer>();

            var resizerUrl = config.GetValue<string>("assets:resizerUrl");

            if (!string.IsNullOrWhiteSpace(resizerUrl))
            {
                services.AddHttpClient("ImageResizer", options =>
                {
                    options.BaseAddress = new Uri(resizerUrl);
                });

                services.AddSingletonAs<RemoteImageResizer>()
                    .As<IImageResizer>();
            }
        }

        public static void UseSquidexImageResizing(this IApplicationBuilder app)
        {
            app.Map("/images/resize", builder =>
            {
                builder.UseMiddleware<ImagesMiddleware>();
            });
        }
    }
}
