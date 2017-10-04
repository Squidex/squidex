using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Infrastructure.Scripting;

namespace Squidex.Config.Domain
{
    public class QueryScriptsModule : Module
    {
        public IConfiguration Configuration { get; }

        public QueryScriptsModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var scriptsRoot = Configuration.GetValue<string>("queryScripts:root");

            builder.Register(c => new QueryScriptFileService(scriptsRoot))
                .As<IQueryScriptFileService>()
                .SingleInstance();
        }
    }
}
