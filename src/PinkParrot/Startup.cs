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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PinkParrot.Configurations;

// ReSharper disable AccessToModifiedClosure

namespace PinkParrot
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddAppSerializers();
            services.AddRouting();
            services.AddAppSwagger();
            services.AddEventFormatter();

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.RegisterModule<WriteDependencies>();
            builder.RegisterModule<InfrastructureDependencies>();
            builder.RegisterModule<ReadDependencies>();

            return new AutofacServiceProvider(builder.Build());
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            app.UseMvc();
            app.UseStaticFiles();
            app.UseAppSwagger();
            app.UseAppEventBus();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }
    }
}
