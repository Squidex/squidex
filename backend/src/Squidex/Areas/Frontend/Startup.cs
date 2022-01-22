﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Frontend.Middlewares;
using Squidex.Hosting.Web;
using Squidex.Pipeline.Squid;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.Frontend
{
    public static class Startup
    {
        public static void UseFrontend(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            var fileProvider = environment.WebRootFileProvider;

            app.UseMiddleware<EmbedMiddleware>();

            if (environment.IsProduction())
            {
                fileProvider = new CompositeFileProvider(fileProvider,
                    new PhysicalFileProvider(Path.Combine(environment.WebRootPath, "build")));
            }

            app.Map("/squid.svg", builder =>
            {
                builder.UseMiddleware<SquidMiddleware>();
            });

            app.UseMiddleware<NotifoMiddleware>();

            app.UseWhen(x => !Path.IsPathRooted(x.Request.Path), builder =>
            {
                builder.UseMiddleware<SetupMiddleware>();
            });

            app.UseHtmlTransform(new HtmlTransformOptions
            {
                Transform = (html, context) =>
                {
                    if (context.Request.Path.StartsWithSegments("/index.html", StringComparison.Ordinal) || context.Items.ContainsKey("spa"))
                    {
                        html = html.AddOptions(context);
                    }

                    return new ValueTask<string>(html);
                }
            });

            app.UseSquidexStaticFiles(fileProvider);

            // Try static files again and serve index.html.
            if (environment.IsProduction())
            {
                app.Use((context, next) =>
                {
                    context.Request.Path = new PathString("/index.html");
                    return next();
                });

                app.UseSquidexStaticFiles(fileProvider);
            }

            if (environment.IsDevelopment())
            {
                app.Use((context, next) =>
                {
                    context.Items["spa"] = true;

                    return next();
                });

                app.UseSpa(builder =>
                {
                    builder.UseProxyToSpaDevelopmentServer("https://localhost:3000");
                });
            }
        }

        private static void UseSquidexStaticFiles(this IApplicationBuilder app, IFileProvider fileProvider)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    var response = context.Context.Response;

                    if (!string.IsNullOrWhiteSpace(context.Context.Request.QueryString.ToString()))
                    {
                        response.Headers[HeaderNames.CacheControl] = "max-age=5184000";
                    }
                    else if (string.Equals(response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Headers[HeaderNames.CacheControl] = "no-cache";
                    }
                },
                FileProvider = fileProvider
            });
        }
    }
}
