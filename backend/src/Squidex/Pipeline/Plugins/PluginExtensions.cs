// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Reflection;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Plugins;
using Squidex.Log;
using Squidex.Web;

namespace Squidex.Pipeline.Plugins;

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
        var pluginManager = PluginManager.Instance;
        var pluginOptions = config.Get<PluginOptions>();

        if (pluginOptions?.Plugins != null)
        {
            foreach (var path in pluginOptions.Plugins)
            {
                var pluginAssembly = pluginManager.Load(path, SharedAssemblies);

                pluginAssembly?.AddParts(mvcBuilder);
            }
        }

        pluginManager.ConfigureServices(mvcBuilder.Services, config);

        return mvcBuilder;
    }

    public static void UsePlugins(this IApplicationBuilder app)
    {
        var pluginManager = PluginManager.Instance;

        pluginManager.Log(app.ApplicationServices.GetRequiredService<ISemanticLog>());
    }

    public static void AddSquidexPluginsPost(this IServiceCollection services, IConfiguration config)
    {
        var pluginManager = PluginManager.Instance;

        pluginManager.ConfigureServicesPost(services, config);
    }
}
