// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Plugins;

namespace Squidex.Pipeline.Plugins
{
    public static class PluginExtensions
    {
        public static IMvcBuilder AddMyPlugins(this IMvcBuilder mvcBuilder, IConfiguration config)
        {
            var pluginManager = new PluginManager();

            var options = config.Get<PluginOptions>();

            if (options.Plugins != null)
            {
                foreach (var path in options.Plugins)
                {
                    var plugin = PluginLoaders.LoadPlugin(path);

                    if (plugin != null)
                    {
                        try
                        {
                            var pluginAssembly = plugin.LoadDefaultAssembly();

                            pluginAssembly.AddParts(mvcBuilder);
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

            pluginManager.ConfigureServices(mvcBuilder.Services, config);

            mvcBuilder.Services.AddSingleton(pluginManager);

            return mvcBuilder;
        }

        public static void UsePluginsBefore(this IApplicationBuilder app)
        {
            var pluginManager = app.ApplicationServices.GetRequiredService<PluginManager>();

            pluginManager.ConfigureBefore(app);
        }

        public static void UsePluginsAfter(this IApplicationBuilder app)
        {
            var pluginManager = app.ApplicationServices.GetRequiredService<PluginManager>();

            pluginManager.ConfigureAfter(app);
        }

        public static void UsePlugins(this IApplicationBuilder app)
        {
            var pluginManager = app.ApplicationServices.GetRequiredService<PluginManager>();

            pluginManager.Log(app.ApplicationServices.GetService<ISemanticLog>());
        }
    }
}
