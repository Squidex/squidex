// ==========================================================================
//  AppApiFilter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Read;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Pipeline
{
    public sealed class AppApiFilter : IAsyncActionFilter
    {
        private readonly IAppProvider appProvider;

        private sealed class AppFeature : IAppFeature
        {
            public IAppEntity App { get; }

            public AppFeature(IAppEntity app)
            {
                App = app;
            }
        }

        public AppApiFilter(IAppProvider appProvider)
        {
            this.appProvider = appProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var appName = context.RouteData.Values["app"]?.ToString();

            if (!string.IsNullOrWhiteSpace(appName))
            {
                var app = await appProvider.GetAppAsync(appName);

                if (app == null)
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
