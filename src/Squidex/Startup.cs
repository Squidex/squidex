// ==========================================================================
//  Startup.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
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

// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable AccessToModifiedClosure

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

        private IConfigurationRoot Configuration { get; }

        private IHostingEnvironment Environment { get; }

        public Startup(IHostingEnvironment env)
        {
            Environment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMySwaggerSettings();
            services.AddMyEventFormatter();
            services.AddMyDataProtectection(Configuration);
            services.AddMyIdentity();
            services.AddMyIdentityServer();
            services.AddMyMvc();

            services.AddCors();
            services.AddLogging();
            services.AddMemoryCache();
            services.AddOptions();
            services.AddRouting();

            services.Configure<MyUrlsOptions>(
                Configuration.GetSection("urls"));
            services.Configure<MyIdentityOptions>(
                Configuration.GetSection("identity"));
            services.Configure<MyUsageOptions>(
                Configuration.GetSection("usage"));

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule(new AssetStoreModule(Configuration));
            builder.RegisterModule(new EventPublishersModule(Configuration));
            builder.RegisterModule(new EventStoreModule(Configuration));
            builder.RegisterModule(new InfrastructureModule(Configuration));
            builder.RegisterModule(new PubSubModule(Configuration));
            builder.RegisterModule(new ReadModule(Configuration));
            builder.RegisterModule(new StoreModule(Configuration));
            builder.RegisterModule(new WebModule(Configuration));
            builder.RegisterModule(new WriteModule(Configuration));

            var container = builder.Build();

            container.Resolve<IApplicationLifetime>().ApplicationStopping.Register(() =>
            {
                container.Dispose();
            });
            
            return new AutofacServiceProvider(container);
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

                identityApp.UseMyIdentity();
                identityApp.UseMyIdentityServer();
                identityApp.UseMyAdminRole();
                identityApp.UseMyAdmin();
                identityApp.UseMyApiProtection();
                identityApp.UseMyGoogleAuthentication();
                identityApp.UseMyGithubAuthentication();
                identityApp.UseMyMicrosoftAuthentication();
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
                appApi.UseMyApiProtection();

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
