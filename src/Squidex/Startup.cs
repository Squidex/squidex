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
using Squidex.Config.EventStore;
using Squidex.Config.Identity;
using Squidex.Config.Swagger;
using Squidex.Config.Web;
using Squidex.Pipeline;
using Squidex.Store.MongoDb;
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
            "/account"
        };

        public IConfigurationRoot Configuration { get; }

        public IHostingEnvironment Environment { get; }

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
            services.AddMyEventFormatter();
            services.AddMyIdentity();
            services.AddMyIdentityServer(Environment);
            services.AddMyMvc();

            services.AddLogging();
            services.AddMemoryCache();
            services.AddOptions();
            services.AddRouting();
            services.AddWebpackBuilder();

            services.Configure<MyMongoDbOptions>(
                Configuration.GetSection("stores:mongoDb"));
            services.Configure<MyRabbitMqOptions>(
                Configuration.GetSection("stores:rabbitMq"));
            services.Configure<MyUrlsOptions>(
                Configuration.GetSection("urls"));
            services.Configure<MyIdentityOptions>(
                Configuration.GetSection("identity"));

            var builder = new ContainerBuilder();
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterModule<MongoDbEventStoreModule>();
            builder.RegisterModule<MongoDbModule>();
            builder.RegisterModule<RabbitMqEventChannelModule>();
            builder.RegisterModule<ReadModule>();
            builder.RegisterModule<WebModule>();
            builder.RegisterModule<WriteModule>();
            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
        
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug();

            if (!Environment.IsDevelopment())
            {
                app.UseMiddleware<SingleUrlsMiddleware>();
            }

            MapAndUseIdentity(app);
            MapAndUseApi(app);
            MapAndUseFrontend(app);

            app.UseMyEventStore();
        }

        private void MapAndUseIdentity(IApplicationBuilder app)
        {
            app.Map(Constants.IdentityPrefix, identityApp =>
            {
                if (Environment.IsDevelopment())
                {
                    identityApp.UseDeveloperExceptionPage();
                }

                identityApp.UseMyIdentity();
                identityApp.UseMyIdentityServer();
                identityApp.UseMyApiProtection();
                identityApp.UseMyGoogleAuthentication();
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
                app.UseDeveloperExceptionPage();
                app.UseWebpackProxy();
                
                app.Use((context, next) => {
                    if (!Path.HasExtension(context.Request.Path.Value))
                    {
                        context.Request.Path = new PathString("/index.html");
                    }
                    return next();
                });
            }
            else
            {
                app.Use((context, next) => {
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
