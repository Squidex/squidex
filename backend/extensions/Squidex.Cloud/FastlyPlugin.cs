using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Cloud
{
    public class FastlyPlugin: IPlugin
    {
        void IPlugin.ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<IEventConsumer, FastlyInvalidator>();
        }
    }
}
