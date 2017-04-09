// ==========================================================================
//  StoreModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Configuration;
using Squidex.Infrastructure;

namespace Squidex.Config.Domain
{
    public class StoreModule : Module
    {
        private IConfiguration Configuration { get; }

        public StoreModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var storeType = Configuration.GetValue<string>("store:type");

            if (string.IsNullOrWhiteSpace(storeType))
            {
                throw new ConfigurationException("Configure the Store type with 'store:type'.");
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
