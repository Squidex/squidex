﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Plugins;
using Squidex.Web;

namespace Squidex.Pipeline.Plugins
{
    public static class PluginExtensions
    {
        private static readonly AssemblyName[] SharedAssemblies = new[]
        {
            typeof(IPlugin),
            typeof(SquidexCoreModel),
            typeof(SquidexCoreOperations),
            typeof(SquidexEntities),
            typeof(SquidexEvents),
            typeof(SquidexInfrastructure),
            typeof(SquidexWeb)
        }.Select(x => x.Assembly.GetName()).ToArray();

        public static IMvcBuilder AddSquidexPlugins(this IMvcBuilder mvcBuilder, IConfiguration config)
        {
            var pluginManager = new PluginManager();

            var options = config.Get<PluginOptions>();

            if (options.Plugins != null)
            {
                foreach (var path in options.Plugins)
                {
                    var pluginAssembly = pluginManager.Load(path, SharedAssemblies);

                    if (pluginAssembly != null)
                    {
                        pluginAssembly.AddParts(mvcBuilder);
                    }
                }
            }

            pluginManager.ConfigureServices(mvcBuilder.Services, config);

            mvcBuilder.Services.AddSingleton(pluginManager);

            return mvcBuilder;
        }

        public static void UsePlugins(this IApplicationBuilder app)
        {
            var pluginManager = app.ApplicationServices.GetRequiredService<PluginManager>();

            pluginManager.Log(app.ApplicationServices.GetService<ISemanticLog>());
        }
    }
}
