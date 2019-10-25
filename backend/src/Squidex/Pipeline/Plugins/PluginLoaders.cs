﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.NETCore.Plugins;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Plugins;
using Squidex.Web;

namespace Squidex.Pipeline.Plugins
{
    public static class PluginLoaders
    {
        private static readonly Type[] SharedTypes =
        {
            typeof(IPlugin),
            typeof(SquidexCoreModel),
            typeof(SquidexCoreOperations),
            typeof(SquidexEntities),
            typeof(SquidexEvents),
            typeof(SquidexInfrastructure),
            typeof(SquidexWeb)
        };

        public static PluginLoader? LoadPlugin(string pluginPath)
        {
            foreach (var candidate in GetPaths(pluginPath))
            {
                if (candidate.Extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    return PluginLoader.CreateFromAssemblyFile(candidate.FullName, config =>
                    {
                        config.PreferSharedTypes = true;

                        config.SharedAssemblies.AddRange(SharedTypes.Select(x => x.Assembly.GetName()));
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
}
