// ==========================================================================
//  AppMiddleware.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using PinkParrot.Read.Apps.Services;

namespace PinkParrot.Pipeline
{
    public sealed class AppMiddleware
    {
        private readonly IAppProvider appProvider;
        private readonly IHostingEnvironment appEnvironment;
        private readonly RequestDelegate next;

        public AppMiddleware(RequestDelegate next, IAppProvider appProvider, IHostingEnvironment appEnvironment)
        {
            this.next = next;
            this.appProvider = appProvider;
            this.appEnvironment = appEnvironment;
        }

        public async Task Invoke(HttpContext context)
        {
            var hostParts = context.Request.Host.ToString().Split('.');

            if (appEnvironment.IsDevelopment() || hostParts.Length >= 3)
            {
                var appId = await appProvider.FindAppIdByNameAsync(hostParts[0]);

                if (appId.HasValue)
                {
                    context.Features.Set<IAppFeature>(new AppFeature(appId.Value));
                }
            }

            await next(context);
        }
    }
}
