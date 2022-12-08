// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using McMaster.NETCore.Plugins;

namespace Squidex.Infrastructure.Plugins;

[ExcludeFromCodeCoverage]
public static class PluginLoaders
{
    public static PluginLoader? LoadPlugin(string pluginPath, AssemblyName[] sharedAssemblies)
    {
        foreach (var candidate in GetPaths(pluginPath))
        {
            if (candidate.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return PluginLoader.CreateFromAssemblyFile(candidate.FullName, config =>
                {
                    config.PreferSharedTypes = true;

                    config.SharedAssemblies.AddRange(sharedAssemblies);
                });
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
            var assembly = Assembly.GetEntryAssembly();

            if (assembly != null)
            {
                var directory = Path.GetDirectoryName(assembly.Location);

                if (directory != null)
                {
                    candidate = new FileInfo(Path.Combine(directory, pluginPath));

                    if (candidate.Exists)
                    {
                        yield return candidate;
                    }
                }
            }
        }
    }
}
