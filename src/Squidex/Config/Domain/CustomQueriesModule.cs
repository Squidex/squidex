using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Autofac;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;

namespace Squidex.Config.Domain
{
    public class CustomQueriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = Directory.EnumerateFiles("./Plugins", "*.dll");
            foreach (var asmPath in assemblies)
            {
                try
                {
                    var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(asmPath);
                    builder.RegisterAssemblyTypes(asm)
                        .Where(t => typeof(IQueryModule).IsAssignableFrom(t))
                        .As<IQueryModule>()
                        .SingleInstance();
                }
                catch (Exception e)
                {
                }
            }

            builder.RegisterType(typeof(QueryModulesService))
                .As<IQueryModulesService>()
                .SingleInstance();
        }
    }
}
