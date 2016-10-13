// ==========================================================================
//  Startup.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
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
            services.AddMvc().AddAppSerializers();
            services.AddRouting();
            services.AddMemoryCache();
            services.AddOptions();
            services.AddEventFormatter();
            services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders();

            services.Configure<MongoDbModule>(
                Configuration.GetSection("MongoDb"));
            services.Configure<EventStoreOptions>(
                Configuration.GetSection("EventStore"));

            var builder = new ContainerBuilder();
            builder.RegisterModule<InfrastructureModule>();
            builder.RegisterModule<ReadModule>();
            builder.RegisterModule<WriteModule>();
            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseApps();
            app.UseMvc();
            app.UseStaticFiles();
            app.UseEventStore();
            app.UseDefaultUser();
        }
    }
}
