// ==========================================================================
//  Startup.cs
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
using Microsoft.Extensions.Logging;
using Squidex.Config;
using Squidex.Config.Domain;
using Squidex.Config.Identity;
using Squidex.Config.Swagger;
using Squidex.Config.Web;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Log.Adapter;

#pragma warning disable RECS0002 // Convert anonymous method to method group

namespace Squidex
{
    public class Startup
    {
        private static readonly string[] IdentityServerPaths =
        {
            "/client-callback-popup",
            "/client-callback-silent",
            "/account",
            "/error"
        };

        private IConfiguration Configuration { get; }

        private IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            Environment = env;

            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddMemoryCache();
            services.AddOptions();

            services.AddMyAssetServices(Configuration);
            services.AddMyAuthentication(Configuration);
            services.AddMyDataProtectection(Configuration);
            services.AddMyEventPublishersServices(Configuration);
            services.AddMyEventStoreServices(Configuration);
            services.AddMyIdentity();
            services.AddMyIdentityServer();
            services.AddMyInfrastructureServices(Configuration);
            services.AddMyMvc();
            services.AddMyPubSubServices(Configuration);
            services.AddMyReadServices(Configuration);
            services.AddMySerializers();
            services.AddMyStoreServices(Configuration);
            services.AddMySwaggerSettings();
            services.AddMyWriteServices();

            services.Configure<MyUrlsOptions>(
                Configuration.GetSection("urls"));
            services.Configure<MyIdentityOptions>(
                Configuration.GetSection("identity"));
            services.Configure<MyUIOptions>(
                Configuration.GetSection("ui"));
            services.Configure<MyUsageOptions>(
                Configuration.GetSection("usage"));
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddSemanticLog(app.ApplicationServices.GetRequiredService<ISemanticLog>());

            app.TestExternalSystems();

            app.UseMyCors();
            app.UseMyForwardingRules();
            app.UseMyTracking();

            MapAndUseIdentity(app);
            MapAndUseApi(app);
            MapAndUseFrontend(app);

            app.UseMyEventStore();

            var log = app.ApplicationServices.GetRequiredService<ISemanticLog>();

            log.LogInformation(w => w
                .WriteProperty("message", "Application started")
                .WriteObject("environment", c =>
                {
                    foreach (var kvp in Configuration.AsEnumerable().Where(kvp => kvp.Value != null))
                    {
                        c.WriteProperty(kvp.Key, kvp.Value);
                    }
                }));
        }

        private void MapAndUseIdentity(IApplicationBuilder app)
        {
            app.Map(Constants.IdentityPrefix, identityApp =>
            {
                if (Environment.IsDevelopment())
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

                identityApp.MapWhen(x => IsIdentityRequest(x), mvcApp =>
                {
                    mvcApp.UseMvc();
                });
            });
        }

        private void MapAndUseApi(IApplicationBuilder app)
        {
            app.Map(Constants.ApiPrefix, appApi =>
            {
                if (Environment.IsDevelopment())
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

        private void MapAndUseFrontend(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
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
