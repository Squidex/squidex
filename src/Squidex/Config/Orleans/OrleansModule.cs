// ==========================================================================
//  OrleansModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Config.Domain;
using Squidex.Infrastructure;

namespace Squidex.Config.Orleans
{
    public sealed class OrleansModule : Module
    {
        private IConfiguration Configuration { get; }

        public OrleansModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var storeType = Configuration.GetValue<string>("orleans:type");

            if (string.IsNullOrWhiteSpace(storeType))
            {
                throw new ConfigurationException("Configure Orleans type with 'orleans:type'.");
            }

            if (string.Equals(storeType, "MongoDB", StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterModule(new StoreMongoDbModule(Configuration));
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{storeType}' for 'stores:type', supported: MongoDb.");
            }
        }
    }
}
