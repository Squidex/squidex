// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Web
{
    public sealed class ClearCookiesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var cookies = context.HttpContext.Response.Cookies;

            foreach (var cookie in context.HttpContext.Request.Cookies.Keys)
            {
                cookies.Delete(cookie);
            }
        }
    }
}
