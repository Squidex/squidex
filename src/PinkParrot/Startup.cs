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
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PinkParrot.Infrastructure.CQRS.Autofac;
using PinkParrot.Infrastructure.CQRS.Commands;
// ReSharper disable AccessToModifiedClosure

namespace PinkParrot
{
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddRouting();
            services.AddSwaggerGen();

            IContainer container = null;

            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);

            containerBuilder.Register(c => container)
                .As<IContainer>()
                .SingleInstance();

            containerBuilder.RegisterType<AutofacDomainObjectFactory>()
                .As<IDomainObjectFactory>()
                .SingleInstance();

            containerBuilder.RegisterType<InMemoryCommandBus>()
                .As<ICommandBus>()
                .SingleInstance();
            
            container = containerBuilder.Build();

            return new AutofacServiceProvider(container);
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            app.UseMvc();
            app.UseStaticFiles();
            app.UseSwagger();
            app.UseSwaggerUi();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
