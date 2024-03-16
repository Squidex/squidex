// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using RequestContext = Squidex.Domain.Apps.Entities.Context;

namespace Squidex.Web
{
    public static class ContextExtensions
    {
        public static RequestContext Context(this HttpContext httpContext)
        {
            var context = httpContext.Features.Get<RequestContext>();

            if (context == null)
            {
                context = RequestContext.Anonymous(null!);

                httpContext.Features.Set(context);
            }

            return context;
        }
    }
}
