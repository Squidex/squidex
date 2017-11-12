// ==========================================================================
//  WebApp.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Config;
using Squidex.Config.Domain;
using Squidex.Config.Identity;
using Squidex.Config.Swagger;
using Squidex.Config.Web;
using Squidex.Infrastructure.Log;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex
{
    public static class WebApp
    {
        private static readonly string[] IdentityServerPaths =
        {
            "/client-callback-popup",
            "/client-callback-silent",
            "/account",
            "/error"
        };

        public static void ConfigureApp(this IApplicationBuilder app)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();

            app.TestExternalSystems();

            app.UseMyCors();
            app.UseMyForwardingRules();
            app.UseMyTracking();

            app.MapAndUseIdentityServer(env);
            app.MapAndUseApi(env);
            app.MapAndUseFrontend(env);

            var log = app.ApplicationServices.GetRequiredService<ISemanticLog>();

            var config = app.ApplicationServices.GetRequiredService<IConfiguration>();

            log.LogInformation(w => w
                .WriteProperty("message", "Application started")
                .WriteObject("environment", c =>
                {
                    foreach (var kvp in config.AsEnumerable().Where(kvp => kvp.Value != null))
                    {
                        c.WriteProperty(kvp.Key, kvp.Value);
                    }
                }));

            app.UseMyEventStore();
        }

        private static void MapAndUseIdentityServer(this IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Map(Constants.IdentityPrefix, identityApp =>
            {
                if (env.IsDevelopment())
                {
                    identityApp.UseDeveloperExceptionPage();
                }
                else
                {
                    identityApp.UseExceptionHandler("/error");
                }

                identityApp.UseMyAuthentication();
                identityApp.UseMyIdentityServer();
                identityApp.UseMyAdminRole();
                identityApp.UseMyAdmin();
                identityApp.UseStaticFiles();

                identityApp.MapWhen(IsIdentityRequest, mvcApp =>
                {
                    mvcApp.UseMvc();
                });
            });
        }

        private static void MapAndUseApi(this IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Map(Constants.ApiPrefix, appApi =>
            {
                if (env.IsDevelopment())
                {
                    appApi.UseDeveloperExceptionPage();
                }

                appApi.UseMySwagger();

                appApi.MapWhen(x => !IsIdentityRequest(x), mvcApp =>
                {
                    mvcApp.UseMvc();
                });
            });
        }

        private static void MapAndUseFrontend(this IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseWebpackProxy();

                app.Use((context, next) =>
                {
                    if (!Path.HasExtension(context.Request.Path.Value))
                    {
                        context.Request.Path = new PathString("/index.html");
                    }
                    return next();
                });
            }
            else
            {
                app.Use((context, next) =>
                {
                    if (!Path.HasExtension(context.Request.Path.Value))
                    {
                        context.Request.Path = new PathString("/build/index.html");
                    }

                    return next();
                });
            }

            app.UseMyCachedStaticFiles();
        }

        private static bool IsIdentityRequest(HttpContext context)
        {
            return IdentityServerPaths.Any(p => context.Request.Path.StartsWithSegments(p));
        }
    }
}
