// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Reflection;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Pipeline.Plugins
{
    public static class PluginExtensions
    {
        private static readonly Type[] SharedTypes = { typeof(IPlugin) };

        public static void AddPlugins(IMvcBuilder mvcBuilder, IConfiguration configuration)
        {
            var options = configuration.Get<PluginOptions>();

            if (options.Plugins != null)
            {
                var pluginManager = new PluginManager();

                foreach (var pluginPath in options.Plugins)
                {
                    PluginLoader plugin = null;

                    if (pluginPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        plugin = PluginLoader.CreateFromAssemblyFile(pluginPath, SharedTypes);
                    }
                    else
                    {
                        plugin = PluginLoader.CreateFromConfigFile(pluginPath, SharedTypes);
                    }

                    if (plugin != null)
                    {
                        var pluginAssembly = plugin.LoadDefaultAssembly();

                        AddParts(mvcBuilder, pluginAssembly);

                        var relatedAssemblies = pluginAssembly.GetCustomAttributes<RelatedAssemblyAttribute>();

                        foreach (var relatedAssembly in relatedAssemblies)
                        {
                            var assembly = plugin.LoadAssembly(relatedAssembly.AssemblyFileName);

                            AddParts(mvcBuilder, assembly);
                        }

                        pluginManager.Add(pluginAssembly);
                    }
                }

                mvcBuilder.Services.AddSingleton(pluginManager);
            }
        }

        private static void AddParts(IMvcBuilder mvcBuilder, Assembly assembly)
        {
            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);

            foreach (var part in partFactory.GetApplicationParts(assembly))
            {
                mvcBuilder.PartManager.ApplicationParts.Add(part);
            }
        }
    }
}
