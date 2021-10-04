// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Frontend.Middlewares;
using Squidex.Pipeline.Squid;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.Frontend
{
    public static class Startup
    {
        public static void ConfigureFrontend(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            app.Map("/squid.svg", builder => builder.UseMiddleware<SquidMiddleware>());

            app.UseMiddleware<NotifoMiddleware>();

            var indexFile =
                environment.IsProduction() ?
                    new PathString("/build/index.html") :
                    new PathString("/index.html");

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
                    context.Request.Path = indexFile;
                }

                return next();
            });

            app.UseWhen(x => x.Request.Path.StartsWithSegments(indexFile, StringComparison.Ordinal), builder =>
            {
                builder.UseMiddleware<SetupMiddleware>();
            });

            app.UseMiddleware<IndexMiddleware>();

            app.ConfigureDev();

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

        public static void ConfigureDev(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            if (environment.IsDevelopment())
            {
                app.UseMiddleware<WebpackMiddleware>();
            }
        }
    }
}
