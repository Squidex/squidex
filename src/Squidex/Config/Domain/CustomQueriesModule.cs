// ==========================================================================
//  CustomQueriesModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.IO;
using System.Runtime.Loader;
using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;

namespace Squidex.Config.Domain
{
    public class CustomQueriesModule : Module
    {
        private IConfiguration Configuration { get; }

        public CustomQueriesModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (Configuration.GetValue<bool>("useGreaterQuery"))
            {
                builder.RegisterType<GreaterQuery>()
                    .As<ICustomQueryProvider>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<NoopQueryProvider>()
                    .As<ICustomQueryProvider>()
                    .SingleInstance();
            }

            if (!Directory.Exists("./Plugins"))
            {
                return;
            }

            var assemblies = Directory.EnumerateFiles("./Plugins", "*.dll");

            foreach (var assemblyPath in assemblies)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath));

                builder.RegisterAssemblyModules(assembly);
            }
        }
    }
}