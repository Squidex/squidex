using Autofac;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;

namespace Squidex.Sample.Plugin
{
    public class SampleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SamplePlugin>().As<IQueryModule>().SingleInstance();
        }
    }
}
