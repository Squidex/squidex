// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Squidex.Infrastructure.Plugins
{
    public sealed class PluginManager
    {
        private readonly HashSet<IPlugin> plugins = new HashSet<IPlugin>();

        public void Add(Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            var pluginTypes =
                assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsAbstract);

            foreach (var pluginType in pluginTypes)
            {
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);

                plugins.Add(plugin);
            }
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(configuration, nameof(configuration));

            foreach (var plugin in plugins)
            {
                plugin.ConfigureServices(services, configuration);
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            Guard.NotNull(app, nameof(app));

            foreach (var plugin in plugins.OfType<IWebPlugin>())
            {
                plugin.Configure(app);
            }
        }
    }
}
