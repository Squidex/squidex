using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Loader;
using System.Text;
using Autofac;

namespace Squidex.Extensibility
{
    public static class ModuleLoader
    {
        public static void LoadPlugins(ContainerBuilder builder)
        {
            var assemblies = Directory.EnumerateFiles("./Plugins", "*.dll");
            foreach (var asmPath in assemblies)
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(asmPath);
                    builder.RegisterAssemblyTypes(asm)
                        .Where(t => typeof(ISquidexPlugin).IsAssignableFrom(t))
                        .As<ISquidexPlugin>();
                }
                catch (Exception e)
                {
                }
            }
        }
    }
}
