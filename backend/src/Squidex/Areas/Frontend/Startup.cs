// ==========================================================================
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
        public static void ConfigureFrontend(this IApplicationBuilder app)
        {
            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            var fileProvider = environment.WebRootFileProvider;

            if (environment.IsProduction())
            {
                fileProvider = new CompositeFileProvider(fileProvider,
                    new PhysicalFileProvider(Path.Combine(environment.WebRootPath, "build")));

                app.Use((context, next) =>
                {
                    if (!Path.HasExtension(context.Request.Path.Value))
                    {
                        context.Request.Path = new PathString("/index.html");
                    }

                    return next();
                });
            }

            app.Map("/squid.svg", builder =>
            {
                builder.UseMiddleware<SquidMiddleware>();
            });

            app.UseMiddleware<NotifoMiddleware>();

            app.UseWhen(x => x.IsIndex(), builder =>
            {
                builder.UseMiddleware<SetupMiddleware>();
            });

            app.UseHtmlTransform(new HtmlTransformOptions
            {
                Transform = (html, context) =>
                {
                    if (context.IsIndex())
                    {
                        html = html.AddOptions(context);
                    }

                    return new ValueTask<string>(html);
                }
            });

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

            if (environment.IsDevelopment())
            {
                app.UseSpa(builder =>
                {
                    builder.UseProxyToSpaDevelopmentServer("https://localhost:3000");
                });
            }
        }
    }
}
