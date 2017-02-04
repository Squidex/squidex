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
        public IConfiguration Configuration { get; }

        public StoreModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var storeType = Configuration.GetValue<string>("squidex:stores:type");

            if (string.IsNullOrWhiteSpace(storeType))
            {
                throw new ConfigurationException("You must specify the store type in the 'squidex:stores:type' configuration section.");
            }

            if (string.Equals(storeType, "MongoDB", StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterModule(new StoreMongoDbModule(Configuration));
            }
            else
            {
                throw new ConfigurationException($"Unsupported store type '{storeType}' for key 'squidex:stores:type', supported: MongoDb.");
            }
        }
    }
}
