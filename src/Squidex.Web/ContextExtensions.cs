// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;

namespace Squidex.Web
{
    public static class ContextExtensions
    {
        public static Context Context(this HttpContext httpContext)
        {
            var context = httpContext.Features.Get<Context>();

            if (context == null)
            {
                context = new Context { User = httpContext.User };

                foreach (var header in httpContext.Request.Headers)
                {
                    if (header.Key.StartsWith("X-", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Headers.Add(header.Key, header.Value.ToString());
                    }
                }

                httpContext.Features.Set(context);
            }

            return context;
        }
    }
}
