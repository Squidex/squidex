// ==========================================================================
//  AppFilterAttribute.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Security.Claims;
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
                var app = await appProvider.FindAppByNameAsync(appName);

                if (app == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var subject = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (subject == null || app.Contributors.Any(x => x.SubjectId == subject))
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                context.HttpContext.Features.Set<IAppFeature>(new AppFeature(app));
            }

            await next();
        }
    }
}
