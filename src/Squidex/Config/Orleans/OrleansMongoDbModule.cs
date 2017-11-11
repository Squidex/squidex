using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Orleans.Providers.MongoDB;

namespace Squidex.Config.Orleans
{
    public sealed class OrleansMongoDbModule : Module
    {
        private IConfiguration Configuration { get; }

        public OrleansMongoDbModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var mongoConfig = Configuration.GetSection("orleans:mongoDb");

            builder.RegisterInstance(Options.Create(mongoConfig.Get<MongoDBGatewayListProviderOptions>()))
                .As<IOptions<MongoDBGatewayListProviderOptions>>()
                .SingleInstance();

            builder.RegisterInstance(Options.Create(mongoConfig.Get<MongoDBMembershipTableOptions>()))
                .As<IOptions<MongoDBMembershipTableOptions>>()
                .SingleInstance();

            builder.RegisterInstance(Options.Create(mongoConfig.Get<MongoDBRemindersOptions>()))
                .As<IOptions<MongoDBRemindersOptions>>()
                .SingleInstance();

            builder.RegisterInstance(Options.Create(mongoConfig.Get<MongoDBStatisticsOptions>()))
                .As<IOptions<MongoDBStatisticsOptions>>()
                .SingleInstance();
        }
    }
}
