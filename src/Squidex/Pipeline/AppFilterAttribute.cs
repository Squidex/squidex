// ==========================================================================
//  AppFilterAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Read.Apps.Services;

namespace Squidex.Pipeline
{
    public sealed class AppFilterAttribute : ActionFilterAttribute
    {
        private readonly IAppProvider appProvider;

        public AppFilterAttribute(IAppProvider appProvider)
        {
            this.appProvider = appProvider;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appName = context.RouteData.Values["app"]?.ToString();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                var appId = await appProvider.FindAppIdByNameAsync(appName);

                if (!appId.HasValue)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                if (!context.HttpContext.User.HasClaim("app", appName))
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(appId.Value));
            }

            await next();
        }
    }
}
