// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Entities;

namespace Squidex.Web
{
    public static class ContextExtensions
    {
        public static Context Context(this HttpContext httpContext)
        {
            var result = httpContext.Features.Get<Context>();

            if (result == null)
            {
                httpContext.Features.Set(new Context { User = httpContext.User });
            }

            return result;
        }
    }
}
