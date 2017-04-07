// ==========================================================================
//  RabbitMqModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Autofac;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.RabbitMq;

// ReSharper disable InvertIf

namespace Squidex.Config.Domain
{
    public sealed class RabbitMqModule : Module
    {
        private IConfiguration Configuration { get; }

        public RabbitMqModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var connectionString = Configuration.GetValue<string>("squidex:eventPublishers:rabbitMq:connectionString");
            var exchange = Configuration.GetValue<string>("squidex:eventPublishers:rabbitMq:exchange");
            var enabled = Configuration.GetValue<bool>("squidex:eventPublishers:rabbitMq:enabled");

            if (!string.IsNullOrWhiteSpace(connectionString) &&
                !string.IsNullOrWhiteSpace(exchange) &&
                enabled)
            {
                var streamFilter = Configuration.GetValue<string>("squidex:eventPublishers:rabbitMq:streamFilter");

                builder.Register(c => new RabbitMqEventConsumer(c.Resolve<JsonSerializerSettings>(), connectionString, exchange, streamFilter))
                    .As<IEventConsumer>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }
        }
    }
}
