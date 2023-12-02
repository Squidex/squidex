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
using Squidex.Web;
using Squidex.Web.Pipeline;

namespace Squidex.Areas.Frontend;

public static class Startup
{
    public static void UseFrontend(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        var fileProvider = environment.WebRootFileProvider;

        app.UseMiddleware<EmbedMiddleware>();

        if (!environment.IsDevelopment())
        {
            var buildFolder = new PhysicalFileProvider(Path.Combine(environment.WebRootPath, "build"));
            var buildProvider = new IgnoreHashFileProvider(buildFolder);

            fileProvider = new CompositeFileProvider(fileProvider, buildFolder, buildProvider);
        }

        app.Map("/squid.svg", builder =>
        {
            builder.UseMiddleware<SquidMiddleware>();
        });

        app.UseMiddleware<NotifoMiddleware>();

        app.UseWhen(IsSpaFile, builder =>
        {
            builder.UseMiddleware<SetupMiddleware>();
        });

        app.UseWhen(IsSpaFileOrHtml, builder =>
        {
            // Adjust the base for all potential html files.
            builder.UseHtmlTransform(new HtmlTransformOptions
            {
                Transform = (html, context) =>
                {
                    return new ValueTask<string>(html.AddOptions(context));
                }
            });
        });

        app.Use((context, next) =>
        {
            return next();
        });

        app.UseSquidexStaticFiles(fileProvider);

        if (!environment.IsDevelopment())
        {
            // Try static files again to serve index.html.
            app.UsePathOverride("/index.html");
            app.UseSquidexStaticFiles(fileProvider);
        }
        else
        {
            // Forward requests to SPA development server.
            app.UseSpa(builder =>
            {
                builder.UseProxyToSpaDevelopmentServer("https://localhost:3000");
            });
        }
    }

    private static void UseSquidexStaticFiles(this IApplicationBuilder app, IFileProvider fileProvider)
    {
        static bool HasQueryString(HttpContext context)
        {
            return !string.IsNullOrWhiteSpace(context.Request.QueryString.ToString());
        }

        static bool IsHtml(HttpContext context)
        {
            return string.Equals(context.Response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase);
        }

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
            {
                var response = context.Context.Response;

                if (IsHtml(context.Context) || HasQueryString(context.Context))
                {
                    response.Headers[HeaderNames.CacheControl] = "no-cache";
                    response.Headers.Remove(HeaderNames.ETag);
                    response.Headers.Remove(HeaderNames.LastModified);
                }
                else
                {
                    response.Headers[HeaderNames.CacheControl] = "max-age=5184000";
                }
            },
            FileProvider = fileProvider
        });
    }

    private static bool IsSpaFileOrHtml(this HttpContext context)
    {
        return context.IsSpaFile() || context.IsHtmlPath();
    }

    private static bool IsSpaFile(this HttpContext context)
    {
        return (context.IsIndex() || !Path.HasExtension(context.Request.Path)) && !context.IsDevServer();
    }

    private static bool IsDevServer(this HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/ws", StringComparison.OrdinalIgnoreCase);
    }
}
