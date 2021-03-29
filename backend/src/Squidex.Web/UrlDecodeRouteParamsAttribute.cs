// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Squidex.Web
{
    public class UrlDecodeRouteParamsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (var (key, value) in context.ActionArguments.ToList())
            {
                if (value is string text)
                {
                    context.ActionArguments[key] = HttpUtility.UrlDecode(text);
                }
            }
        }
    }
}
