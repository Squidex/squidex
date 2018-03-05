// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;

namespace Squidex.Pipeline
{
    public sealed class AppApiFilter : IAsyncActionFilter
    {
        private readonly IAppProvider appProvider;

        public class AppFeature : IAppFeature
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
