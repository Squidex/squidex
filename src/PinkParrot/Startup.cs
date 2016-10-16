// ==========================================================================
//  Startup.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PinkParrot.Configurations;
using PinkParrot.Store.MongoDb;

// ReSharper disable AccessToModifiedClosure

namespace PinkParrot
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddEventFormatter();
            services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders();
            services.AddLogging();
            services.AddMemoryCache();
            services.AddMvc().AddAppSerializers();
            services.AddOptions();
            services.AddRouting();
            services.AddWebpack();

            services.Configure<MongoDbOptions>(
                Configuration.GetSection("stores:mongoDb"));
            services.Configure<EventStoreOptions>(
                Configuration.GetSection("stores:eventStore"));

            var builder = new ContainerBuilder();
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterModule<EventStoreModule>();
            builder.RegisterModule<MongoDbModule>();
            builder.RegisterModule<ReadModule>();
            builder.RegisterModule<WriteModule>();
            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpack();
                app.UseDefaultFiles();
            }
            else
            {
                app.UseDefaultFiles(new DefaultFilesOptions { DefaultFileNames = new List<string> { "build/index.html" } });
            }

            app.UseApps();
            app.UseMvc();
            app.UseStaticFiles();
            app.UseEventStore();
            app.UseDefaultUser();
        }
    }
}
