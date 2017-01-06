// ==========================================================================
//  RabbitMqEventChannelModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.RabbitMq;

namespace Squidex.Config.EventStore
{
    public class RabbitMqEventChannelModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context =>
            {
                var options = context.Resolve<IOptions<MyRabbitMqOptions>>().Value;

                var factory = new ConnectionFactory();

                factory.SetUri(new Uri(options.ConnectionString));

                return factory;
            }).As<IConnectionFactory>().SingleInstance();

            builder.RegisterType<RabbitMqEventChannel>()
                .As<IEventPublisher>()
                .As<IEventStream>()
                .SingleInstance();
        }
    }
}
