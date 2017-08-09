// ==========================================================================
//  WebUsages.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Net.Http.Headers;
using Squidex.Pipeline;

// ReSharper disable InvertIf

namespace Squidex.Config.Web
{
    public static class WebUsages
    {
        public static void UseMyCors(this IApplicationBuilder app)
        {
            app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        }

        public static void UseMyForwardingRules(this IApplicationBuilder app)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto,
                ForwardLimit = null,
                RequireHeaderSymmetry = false
            });

            app.UseMiddleware<EnforceHttpsMiddleware>();
        }

        public static void UseMyCachedStaticFiles(this IApplicationBuilder app)
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = context =>
                {
                    context.Context.Request.GetTypedHeaders();
                    var response = context.Context.Response;

                    var headers = response.GetTypedHeaders();

                    if (!string.Equals(response.ContentType, "text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            MaxAge = TimeSpan.FromDays(60)
                        };
                    }
                    else
                    {
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true
                        };
                    }
                }
            });
        }
    }
}
