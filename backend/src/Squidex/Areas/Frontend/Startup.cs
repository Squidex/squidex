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
            fileProvider = new CompositeFileProvider(fileProvider,
                new PhysicalFileProvider(Path.Combine(environment.WebRootPath, "build")));
        }

        app.Map("/squid.svg", builder =>
        {
            builder.UseMiddleware<SquidMiddleware>();
        });

        app.UseMiddleware<NotifoMiddleware>();

        app.UseWhen(c => c.IsSpaFile(), builder =>
        {
            builder.UseMiddleware<SetupMiddleware>();
        });

        app.UseWhen(c => c.IsSpaFile() || c.IsHtmlPath(), builder =>
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

    private static bool IsSpaFile(this HttpContext context)
    {
        return (context.IsIndex() || !Path.HasExtension(context.Request.Path)) && !context.IsDevServer();
    }

    private static bool IsDevServer(this HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/ws", StringComparison.OrdinalIgnoreCase);
    }
}
