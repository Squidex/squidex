// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using McMaster.NETCore.Plugins;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Pipeline.Plugins
{
    public static class PluginExtensions
    {
        private static readonly Type[] SharedTypes =
        {
            typeof(IPlugin),
            typeof(SquidexCoreModel),
            typeof(SquidexCoreOperations),
            typeof(SquidexEntities),
            typeof(SquidexEvents),
            typeof(SquidexInfrastructure)
        };

        public static IMvcBuilder AddMyPlugins(this IMvcBuilder mvcBuilder, IConfiguration configuration)
        {
            var pluginManager = new PluginManager();

            var options = configuration.Get<PluginOptions>();

            if (options.Plugins != null)
            {
                foreach (var path in options.Plugins)
                {
                    var plugin = LoadPlugin(path);

                    if (plugin != null)
                    {
                        try
                        {
                            var pluginAssembly = plugin.LoadDefaultAssembly();

                            AddParts(mvcBuilder, pluginAssembly);

                            foreach (var relatedAssembly in RelatedAssemblyAttribute.GetRelatedAssemblies(pluginAssembly, false))
                            {
                                AddParts(mvcBuilder, relatedAssembly);
                            }

                            pluginManager.Add(path, pluginAssembly);
                        }
                        catch (Exception ex)
                        {
                            pluginManager.LogException(path, "LoadingAssembly", ex);
                        }
                    }
                    else
                    {
                        pluginManager.LogException(path, "LoadingPlugin", new FileNotFoundException($"Cannot find plugin at {path}"));
                    }
                }
            }

            pluginManager.ConfigureServices(mvcBuilder.Services, configuration);

            mvcBuilder.Services.AddSingleton(pluginManager);

            return mvcBuilder;
        }

        private static PluginLoader LoadPlugin(string pluginPath)
        {
            var fullPath = GetPaths(pluginPath);

            foreach (var candidate in GetPaths(pluginPath))
            {
                if (candidate.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    return PluginLoader.CreateFromAssemblyFile(candidate.FullName, SharedTypes);
                }

                if (candidate.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    return PluginLoader.CreateFromConfigFile(candidate.FullName, SharedTypes);
                }
            }

            return null;
        }

        private static IEnumerable<FileInfo> GetPaths(string pluginPath)
        {
            var candidate = new FileInfo(Path.GetFullPath(pluginPath));

            if (candidate.Exists)
            {
                yield return candidate;
            }

            if (!Path.IsPathRooted(pluginPath))
            {
                candidate = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), pluginPath));

                if (candidate.Exists)
                {
                    yield return candidate;
                }
            }
        }

        public static void UsePlugins(this IApplicationBuilder app)
        {
            var pluginManager = app.ApplicationServices.GetRequiredService<PluginManager>();

            pluginManager.Configure(app);

            var log = app.ApplicationServices.GetService<ISemanticLog>();

            if (log != null)
            {
                pluginManager.Log(log);
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
