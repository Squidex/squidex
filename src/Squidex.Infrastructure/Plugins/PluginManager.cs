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
using Squidex.Infrastructure.Log;

namespace Squidex.Infrastructure.Plugins
{
    public sealed class PluginManager
    {
        private readonly HashSet<IPlugin> loadedPlugins = new HashSet<IPlugin>();
        private readonly List<(string Plugin, string Action, Exception Exception)> exceptions = new List<(string, string, Exception)>();

        public IReadOnlyCollection<IPlugin> Plugins
        {
            get { return loadedPlugins; }
        }

        public void Add(string name, Assembly assembly)
        {
            Guard.NotNull(assembly, nameof(assembly));

            var pluginTypes =
                assembly.GetTypes()
                    .Where(t => typeof(IPlugin).IsAssignableFrom(t))
                    .Where(t => !t.IsAbstract);

            foreach (var pluginType in pluginTypes)
            {
                try
                {
                    var plugin = (IPlugin)Activator.CreateInstance(pluginType)!;

                    loadedPlugins.Add(plugin);
                }
                catch (Exception ex)
                {
                    LogException(name, "Instantiating", ex);
                }
            }
        }

        public void LogException(string plugin, string action, Exception exception)
        {
            Guard.NotNull(plugin, nameof(plugin));
            Guard.NotNull(action, nameof(action));
            Guard.NotNull(exception, nameof(exception));

            exceptions.Add((plugin, action, exception));
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration config)
        {
            Guard.NotNull(services, nameof(services));
            Guard.NotNull(config, nameof(config));

            foreach (var plugin in loadedPlugins)
            {
                plugin.ConfigureServices(services, config);
            }
        }

        public void ConfigureBefore(IApplicationBuilder app)
        {
            Guard.NotNull(app, nameof(app));

            foreach (var plugin in loadedPlugins.OfType<IWebPlugin>())
            {
                plugin.ConfigureBefore(app);
            }
        }

        public void ConfigureAfter(IApplicationBuilder app)
        {
            Guard.NotNull(app, nameof(app));

            foreach (var plugin in loadedPlugins.OfType<IWebPlugin>())
            {
                plugin.ConfigureAfter(app);
            }
        }

        public void Log(ISemanticLog log)
        {
            Guard.NotNull(log, nameof(log));

            if (loadedPlugins.Count > 0 || exceptions.Count > 0)
            {
                var status = exceptions.Count > 0 ? "CompletedWithErrors" : "Completed";

                log.LogInformation(w => w
                    .WriteProperty("action", "pluginsLoaded")
                    .WriteProperty("status", status)
                    .WriteArray("errors", e =>
                    {
                        foreach (var error in exceptions)
                        {
                            e.WriteObject(x => x
                                .WriteProperty("plugin", error.Plugin)
                                .WriteProperty("action", error.Action)
                                .WriteException(error.Exception));
                        }
                    })
                    .WriteArray("plugins", a =>
                    {
                        foreach (var plugin in loadedPlugins)
                        {
                            a.WriteValue(plugin.GetType().ToString());
                        }
                    }));
            }
        }
    }
}
