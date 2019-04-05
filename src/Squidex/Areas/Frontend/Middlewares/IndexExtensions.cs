// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;

namespace Squidex.Areas.Frontend.Middlewares
{
    public static class IndexExtensions
    {
        public static bool IsIndex(this HttpContext context)
        {
            return context.Request.Path.Value.EndsWith("/index.html", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHtmlPath(this HttpContext context)
        {
            return context.Request.Path.Value.EndsWith(".html", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsHtml(this HttpContext context)
        {
            return context.Response.ContentType?.ToLower().Contains("text/html") == true;
        }
    }
}
