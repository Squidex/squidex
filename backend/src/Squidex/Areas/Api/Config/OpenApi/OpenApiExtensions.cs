// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Areas.Api.Config.OpenApi
{
    public static class OpenApiExtensions
    {
        public static void UseSquidexOpenApi(this IApplicationBuilder app)
        {
            app.UseOpenApi();
        }
    }
}
