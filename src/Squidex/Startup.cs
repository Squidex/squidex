// ==========================================================================
//  Startup.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Squidex.Configurations.Domain;
using Squidex.Configurations.EventStore;
using Squidex.Configurations.Identity;
using Squidex.Configurations.Web;
using Squidex.Store.MongoDb;

// ReSharper disable AccessToModifiedClosure

namespace Squidex
{
    public class Startup
    {
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
            services.Configure<MyEventStoreOptions>(
                Configuration.GetSection("stores:eventStore"));
            services.Configure<MyIdentityOptions>(
                Configuration.GetSection("identity"));

            var builder = new ContainerBuilder();
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterModule<EventStoreModule>();
            builder.RegisterModule<MongoDbModule>();
            builder.RegisterModule<ReadModule>();
            builder.RegisterModule<WriteModule>();
            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
        
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackProxy();
                app.UseDefaultFiles();
            }
            else
            {
                app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new List<string> { "build/index.html" } });
            }

            app.UseMyDefaultUser();
            app.UseMyEventStore();
            app.UseMyIdentity();
            app.UseMyIdentityServer();
            app.UseMyApiProtection();
            app.UseMyGoogleAuthentication();
            app.UseMyApps();
            app.UseStaticFiles();
            app.UseMvc();

        }
    }
}
