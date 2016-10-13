// ==========================================================================
//  AppMiddleware.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PinkParrot.Read.Apps.Services;

namespace PinkParrot.Pipeline
{
    public sealed class AppMiddleware
    {
        private readonly IAppProvider appProvider;
        private readonly RequestDelegate next;

        public AppMiddleware(RequestDelegate next, IAppProvider appProvider)
        {
            this.next = next;

            this.appProvider = appProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            var appId = await appProvider.FindAppIdByNameAsync(context.Request.Host.ToString().Split('.')[0]);

            if (appId.HasValue)
            {
                context.Features.Set<IAppFeature>(new AppFeature(appId.Value));
            }

            await next(context);
        }
    }
}
