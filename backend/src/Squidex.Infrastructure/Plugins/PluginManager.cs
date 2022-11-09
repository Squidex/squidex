// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using McMaster.NETCore.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Log;

namespace Squidex.Infrastructure.Plugins;

public sealed class PluginManager : DisposableObjectBase
{
    private readonly HashSet<PluginLoader> pluginLoaders = new HashSet<PluginLoader>();
    private readonly HashSet<IPlugin> loadedPlugins = new HashSet<IPlugin>();
    private readonly List<(string Plugin, string Action, Exception Exception)> exceptions = new List<(string, string, Exception)>();

    protected override void DisposeObject(bool disposing)
    {
        if (disposing)
        {
            foreach (var loader in pluginLoaders)
            {
                loader.Dispose();
            }
        }
    }

    public Assembly? Load(string path, AssemblyName[] sharedAssemblies)
    {
        Guard.NotNullOrEmpty(path);
        Guard.NotNull(sharedAssemblies);

        Assembly? assembly = null;

        var loader = PluginLoaders.LoadPlugin(path, sharedAssemblies);

        if (loader != null)
        {
            try
            {
                assembly = loader.LoadDefaultAssembly();

                Add(path, assembly);

                pluginLoaders.Add(loader);
            }
            catch (Exception ex)
            {
                LogException(path, "LoadingAssembly", ex);

                loader.Dispose();
            }
        }
        else
        {
            LogException(path, "LoadingPlugin", new FileNotFoundException($"Cannot find plugin at {path}"));
        }

        return assembly;
    }

    private void Add(string name, Assembly assembly)
    {
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

    private void LogException(string plugin, string action, Exception exception)
    {
        exceptions.Add((plugin, action, exception));
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        Guard.NotNull(services);
        Guard.NotNull(config);

        foreach (var plugin in loadedPlugins)
        {
            plugin.ConfigureServices(services, config);
        }
    }

    public void Log(ISemanticLog log)
    {
        Guard.NotNull(log);

        if (loadedPlugins.Count > 0 || exceptions.Count > 0)
        {
            var status = exceptions.Count > 0 ? "CompletedWithErrors" : "Completed";

            log.LogInformation(w => w
                .WriteProperty("message", "Plugins loaded.")
                .WriteProperty("action", "pluginsLoaded")
                .WriteProperty("status", status)
                .WriteArray("errors", e =>
                {
                    foreach (var (plugin, action, exception) in exceptions)
                    {
                        e.WriteObject(x => x
                            .WriteProperty("plugin", plugin)
                            .WriteProperty("action", action)
                            .WriteException(exception));
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
