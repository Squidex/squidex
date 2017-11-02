// ==========================================================================
//  CustomQueriesModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Runtime.Loader;
using Autofac;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;

namespace Squidex.Config.Domain
{
    public class CustomQueriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            if (!Directory.Exists("./Plugins"))
            {
                return;
            }

            builder.RegisterType(typeof(NoopQueryProvider))
                .As<ICustomQueryProvider>()
                .SingleInstance();

            var assemblies = Directory.EnumerateFiles("./Plugins", "*.dll");
            foreach (var asmPath in assemblies)
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.GetFullPath(asmPath));
                    builder.RegisterAssemblyModules(asm);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
    }
}