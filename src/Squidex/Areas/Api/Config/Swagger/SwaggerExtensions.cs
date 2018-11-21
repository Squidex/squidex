// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Squidex.Config;

namespace Squidex.Areas.Api.Config.Swagger
{
    public static class SwaggerExtensions
    {
        public static void UseMySwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(settings =>
            {
                settings.Path = $"{Constants.ApiPrefix}/swagger/{{documentName}}/swagger.json";
            });
        }
    }
}
