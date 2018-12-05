// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSwag.AspNetCore;
using Squidex.Config;

namespace Squidex.Areas.Api.Config.Swagger
{
    public static class SwaggerExtensions
    {
        public static void UseMySwagger(this IApplicationBuilder app)
        {
            var urlOptions = app.ApplicationServices.GetRequiredService<IOptions<MyUrlsOptions>>().Value;

            app.UseSwaggerWithApiExplorer(settings =>
            {
                settings.AddAssetODataParams();
                settings.ConfigureNames();
                settings.ConfigurePaths(urlOptions);
                settings.ConfigureSchemaSettings();
                settings.ConfigureIdentity(urlOptions);
            });
        }
    }
}
