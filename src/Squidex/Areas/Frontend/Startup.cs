// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Frontend.Middlewares;

namespace Squidex.Areas.Frontend
{
    public static class Startup
    {
        public static void ConfigureFrontend(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            if (environment.IsDevelopment())
            {
                app.UseMiddleware<WebpackMiddleware>();
            }

            app.Use((context, next) =>
            {
                if (context.Request.Path == "/client-callback-popup")
                {
                    context.Request.Path = new PathString("/client-callback-popup.html");
                }
                else if (context.Request.Path == "/client-callback-silent")
                {
                    context.Request.Path = new PathString("/client-callback-silent.html");
                }
                else if (!Path.HasExtension(context.Request.Path.Value))
                {
                    if (environment.IsDevelopment())
                    {
                        context.Request.Path = new PathString("/index.html");
                    }
                    else
                    {
                        context.Request.Path = new PathString("/build/index.html");
                    }
                }

                return next();
            });

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    var response = context.Context.Response;
                    var responseHeaders = response.GetTypedHeaders();

                    if (!string.Equals(response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        responseHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            MaxAge = TimeSpan.FromDays(60)
                        };
                    }
                    else
                    {
                        responseHeaders.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true
                        };
                    }
                }
            });
        }
    }
}
